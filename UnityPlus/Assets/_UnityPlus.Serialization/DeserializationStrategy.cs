
using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    public class DeserializationStrategy : IOperationStrategy
    {
        public void InitializeRoot( object root, IDescriptor rootDescriptor, SerializedData rootData, SerializationState state )
        {
            // [Primitive Root]
            if( rootDescriptor is IPrimitiveDescriptor primitiveRoot )
            {
                if( primitiveRoot.DeserializeDirect( rootData, state.Context, out object res ) == DeserializationResult.Success )
                    state.RootResult = res;
                return;
            }

            var cursor = new SerializationCursor
            {
                TargetObj = new TrackedObject( root ), // root Parent is null
                Descriptor = rootDescriptor,
                StepIndex = 0,
                DataNode = rootData,
                Phase = SerializationCursorPhase.PreProcessing,
                WriteBackOnPop = false // Root has no parent to write back to
            };

            state.Stack.Push( cursor );
        }

        public SerializationCursorResult Process( ref SerializationCursor cursor, SerializationState state )
        {
            switch( cursor.Phase )
            {
                case SerializationCursorPhase.PreProcessing:
                    return PhasePreProcessing( ref cursor, state );
                case SerializationCursorPhase.Construction:
                    return PhaseConstruction( ref cursor, state );
                case SerializationCursorPhase.Instantiation:
                    return PhaseInstantiation( ref cursor, state );
                case SerializationCursorPhase.Population:
                    return PhasePopulation( ref cursor, state );
                case SerializationCursorPhase.PostProcessing:
                    return PhasePostProcessing( ref cursor, state );
                default:
                    return SerializationCursorResult.Finished;
            }
        }

        private SerializationCursorResult PhasePreProcessing( ref SerializationCursor cursor, SerializationState state )
        {
            if( cursor.DataNode is SerializedObject rootObj && rootObj.TryGetValue( KeyNames.TYPE, out SerializedData typeData ) )
            {
#warning TODO - use the v3 extension method for types instead.
                Type actualType = state.Context.Config.TypeResolver.ResolveType( (string)(SerializedPrimitive)typeData );
                if( actualType != null )
                {
                    cursor.Descriptor = TypeDescriptorRegistry.GetDescriptor( actualType );
                }
            }

            if( cursor.Descriptor is ICollectionDescriptor )
            {
                var unwrapped = SerializationHelpers.GetCollectionArrayNode( cursor.DataNode );
                if( unwrapped != null ) cursor.DataNode = unwrapped;
            }

            var compDesc = (ICompositeDescriptor)cursor.Descriptor;
            cursor.ConstructionStepCount = compDesc.GetConstructionStepCount( cursor.TargetObj.Target );

            if( cursor.DataNode is SerializedArray arrNode && cursor.Descriptor is ICollectionDescriptor )
            {
                cursor.PopulationStepCount = arrNode.Count;
            }
            else
            {
                cursor.PopulationStepCount = compDesc.GetStepCount( cursor.TargetObj.Target ) - cursor.ConstructionStepCount;
            }

            if( cursor.TargetObj.Target == null && cursor.ConstructionStepCount > 0 )
            {
                cursor.ConstructionBuffer = new object[cursor.ConstructionStepCount];
                cursor.Phase = SerializationCursorPhase.Construction;
            }
            else
            {
                cursor.Phase = SerializationCursorPhase.Instantiation;
            }

            cursor.StepIndex = 0;
            return SerializationCursorResult.Jump;
        }

        private SerializationCursorResult PhaseConstruction( ref SerializationCursor cursor, SerializationState state )
        {
            if( cursor.StepIndex >= cursor.ConstructionStepCount )
            {
                cursor.Phase = SerializationCursorPhase.Instantiation;
                return SerializationCursorResult.Jump;
            }

            var parentDesc = (ICompositeDescriptor)cursor.Descriptor;
            int activeIndex = cursor.StepIndex;
            IMemberInfo memberInfo = parentDesc.GetMemberInfo( activeIndex, cursor.ConstructionBuffer );

            if( memberInfo == null || memberInfo.TypeDescriptor == null )
            {
                return SerializationCursorResult.Advance;
            }

            MemberResolutionResult result = TryResolveMember( memberInfo, cursor.DataNode, activeIndex, state, out object val );

            if( result == MemberResolutionResult.Resolved )
            {
                object buffer = cursor.ConstructionBuffer;
                memberInfo.SetValue( ref buffer, val );
                return SerializationCursorResult.Advance;
            }
            else if( result == MemberResolutionResult.RequiresPush )
            {
                PushChildCursor( ref cursor, memberInfo, activeIndex, true, state );
                return SerializationCursorResult.Push; // Driver handles parent increment
            }
            else if( result == MemberResolutionResult.Deferred )
            {
                state.Context.DeferredOperations.Enqueue( new DeferredOperation()
                {
                    Target = cursor.TargetObj.Parent,
                    Member = cursor.TargetObj.Member,
                    Data = cursor.DataNode,
                    Descriptor = cursor.Descriptor,
                    ConstructionBuffer = cursor.ConstructionBuffer,
                    ConstructionIndex = cursor.StepIndex
                } );
                return SerializationCursorResult.Deferred;
            }
            else
            {
                UnityEngine.Debug.LogWarning( $"Failed to resolve construction argument {memberInfo.Name}" );
                return SerializationCursorResult.Advance;
            }
        }

        private SerializationCursorResult PhaseInstantiation( ref SerializationCursor cursor, SerializationState state )
        {
            var compositeDesc = (ICompositeDescriptor)cursor.Descriptor;

            if( cursor.TargetObj.Target == null )
            {
                object newInstance;
                if( cursor.ConstructionBuffer != null )
                {
                    newInstance = compositeDesc.Construct( cursor.ConstructionBuffer );
                }
                else
                {
                    newInstance = compositeDesc.CreateInitialTarget( cursor.DataNode, state.Context );
                }
                cursor.TargetObj = cursor.TargetObj.WithTarget( newInstance );
            }

            if( cursor.TargetObj.Target == null )
                throw new Exception( "Factory returned null." );

            if( cursor.Descriptor is ICollectionDescriptor colDesc && cursor.DataNode is SerializedArray arr )
            {
                var resized = colDesc.Resize( cursor.TargetObj.Target, arr.Count );
                cursor.TargetObj = cursor.TargetObj.WithTarget( resized );
            }

            if( cursor.DataNode is SerializedObject objNode
                && objNode.TryGetValue( KeyNames.ID, out var idData )
                && Guid.TryParse( (string)(SerializedPrimitive)idData, out Guid guid ) )
            {
                state.Context.ForwardMap.SetObj( guid, cursor.TargetObj.Target );
            }

            if( cursor.TargetObj.IsRoot && state.Stack.Count == 1 )
            {
                state.RootResult = cursor.TargetObj.Target;
            }

            cursor.Phase = SerializationCursorPhase.Population;
            cursor.StepIndex = 0;
            return SerializationCursorResult.Jump;
        }

        private SerializationCursorResult PhasePopulation( ref SerializationCursor cursor, SerializationState state )
        {
            if( cursor.StepIndex >= cursor.PopulationStepCount )
            {
                cursor.Phase = SerializationCursorPhase.PostProcessing;
                return SerializationCursorResult.Jump;
            }

            var parentDesc = (ICompositeDescriptor)cursor.Descriptor;
            int offset = cursor.ConstructionStepCount;
            int absoluteIndex = cursor.StepIndex + offset;

            IMemberInfo memberInfo = parentDesc.GetMemberInfo( absoluteIndex, cursor.TargetObj.Target );

            if( memberInfo == null || memberInfo.TypeDescriptor == null )
            {
                return SerializationCursorResult.Advance;
            }

            MemberResolutionResult result = TryResolveMember( memberInfo, cursor.DataNode, absoluteIndex, state, out object val );

            if( result == MemberResolutionResult.Resolved )
            {
                object t = cursor.TargetObj.Target;
                memberInfo.SetValue( ref t, val );

                // Update cursor target in case of value type replacement
                cursor.TargetObj = cursor.TargetObj.WithTarget( t );
                return SerializationCursorResult.Advance;
            }
            else if( result == MemberResolutionResult.RequiresPush )
            {
                PushChildCursor( ref cursor, memberInfo, absoluteIndex, false, state );
                return SerializationCursorResult.Push; // Driver increments
            }
            else if( result == MemberResolutionResult.Deferred )
            {
                SerializedData failedData = GetDataNode( cursor.DataNode, memberInfo.Name, absoluteIndex );
                state.Context.EnqueueDeferred( cursor.TargetObj.Target, memberInfo, failedData );
                return SerializationCursorResult.Advance; // Skip member and continue
            }
            else
            {
                return SerializationCursorResult.Advance;
            }
        }

        private SerializationCursorResult PhasePostProcessing( ref SerializationCursor cursor, SerializationState state )
        {
            if( cursor.TargetObj.Target != null && cursor.Descriptor is ICompositeDescriptor comp )
            {
                comp.OnDeserialized( cursor.TargetObj.Target, state.Context );
            }
            return SerializationCursorResult.Finished;
        }

        public void OnCursorFinished( SerializationCursor cursor, SerializationState state )
        {
        }

        private MemberResolutionResult TryResolveMember( IMemberInfo memberInfo, SerializedData parentData, int index, SerializationState state, out object value )
        {
            IDescriptor memberDesc = memberInfo.TypeDescriptor;

            if( memberDesc is IPrimitiveDescriptor primitiveDesc )
            {
                SerializedData primitiveData = GetDataNode( parentData, memberInfo.Name, index );
                DeserializationResult result = primitiveDesc.DeserializeDirect( primitiveData, state.Context, out value );

                if( result == DeserializationResult.Success )
                    return MemberResolutionResult.Resolved;
                if( result == DeserializationResult.Deferred )
                    return MemberResolutionResult.Deferred;
                return MemberResolutionResult.Failed;
            }

            SerializedData childNode = GetDataNode( parentData, memberInfo.Name, index );
            if( childNode == null )
            {
                value = null;
                return MemberResolutionResult.Resolved;
            }

            MemberResolutionResult refResult = TryResolveReference( childNode, state, out value );
            if( refResult == MemberResolutionResult.Resolved )
                return MemberResolutionResult.Resolved;
            if( refResult == MemberResolutionResult.Deferred )
                return MemberResolutionResult.Deferred;

            return MemberResolutionResult.RequiresPush;
        }

        private MemberResolutionResult TryResolveReference( SerializedData data, SerializationState state, out object value )
        {
            value = null;
            if( data is SerializedObject refObj && refObj.TryGetValue( KeyNames.REF, out var refVal ) )
            {
                if( refVal is SerializedPrimitive refPrim && Guid.TryParse( (string)refPrim, out Guid refGuid ) )
                {
                    if( state.Context.ForwardMap.TryGetObj( refGuid, out object existingObj ) )
                    {
                        value = existingObj;
                        return MemberResolutionResult.Resolved;
                    }

                    // Optimistic Deferral: If we can't find it now, assume it's coming later.
                    // If it never shows up, the Driver's deadlock detector will catch it.
                    return MemberResolutionResult.Deferred;
                }
            }
            return MemberResolutionResult.Failed;
        }

        private void PushChildCursor( ref SerializationCursor parentCursor, IMemberInfo memberInfo, int absoluteIndex, bool isConstructionPhase, SerializationState state )
        {
            IDescriptor memberDesc = memberInfo.TypeDescriptor;
            SerializedData childNode = GetDataNode( parentCursor.DataNode, memberInfo.Name, absoluteIndex );

            // Determine parent for the child. 
            // If construction phase, parent is the buffer. If population, parent is the actual target.
            object parentObj = isConstructionPhase ? (object)parentCursor.ConstructionBuffer : parentCursor.TargetObj.Target;

            // Optimization: If not construction phase, and parent exists, try to get existing object (PopulateExisting)
            object existingChild = null;
            if( !isConstructionPhase && parentObj != null )
            {
                existingChild = memberInfo.GetValue( parentObj );
            }

            var childCursor = new SerializationCursor
            {
                TargetObj = new TrackedObject( existingChild, parentObj, memberInfo ),
                Descriptor = memberDesc,
                DataNode = childNode,
                Phase = SerializationCursorPhase.PreProcessing,
                StepIndex = 0,
                WriteBackOnPop = true // Always write back deserialized children to their parents
            };

            state.Stack.Push( childCursor );
        }

        private SerializedData GetDataNode( SerializedData parent, string key, int index )
        {
            if( parent is SerializedObject obj && key != null && obj.TryGetValue( key, out var res ) )
                return res;
            if( parent is SerializedArray arr && index >= 0 && index < arr.Count )
                return arr[index];
            return null;
        }
    }
}