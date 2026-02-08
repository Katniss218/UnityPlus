
using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    public class DeserializerStrategy : IOperationStrategy
    {
        public void InitializeRoot( object root, ITypeDescriptor rootDescriptor, SerializedData rootData, SerializationState state )
        {
            // [Primitive Root]
            if( rootDescriptor is IPrimitiveTypeDescriptor primitiveRoot )
            {
                if( primitiveRoot.DeserializeDirect( rootData, state.Context, out object res ) == DeserializeResult.Success )
                    state.RootResult = res;
                return;
            }

            if( rootData != null ) BuildIdIndex( rootData, state );

            var cursor = new SerializationCursor
            {
                Tracker = new TrackedObject( root ), // root Parent is null
                Descriptor = rootDescriptor,
                StepIndex = 0,
                DataNode = rootData,
                Phase = CursorPhase.PreProcessing,
                WriteBackOnPop = false // Root has no parent to write back to
            };

            state.Stack.Push( cursor );
        }

        private void BuildIdIndex( SerializedData rootData, SerializationState state )
        {
            var stack = new Stack<SerializedData>();
            stack.Push( rootData );

            while( stack.Count > 0 )
            {
                var data = stack.Pop();
                if( data is SerializedObject obj )
                {
                    if( obj.TryGetValue( KeyNames.ID, out SerializedData idVal ) && idVal is SerializedPrimitive primId && Guid.TryParse( (string)primId, out Guid guid ) )
                    {
                        if( !state.Context.GlobalIdIndex.ContainsKey( guid ) )
                            state.Context.GlobalIdIndex[guid] = obj;
                    }
                    foreach( var child in obj.Values ) if( child != null ) stack.Push( child );
                }
                else if( data is SerializedArray arr )
                {
                    foreach( var child in arr ) if( child != null ) stack.Push( child );
                }
            }
        }

        public StepResult Process( ref SerializationCursor cursor, SerializationState state )
        {
            switch( cursor.Phase )
            {
                case CursorPhase.PreProcessing:
                    return PhasePreProcessing( ref cursor, state );
                case CursorPhase.Construction:
                    return PhaseConstruction( ref cursor, state );
                case CursorPhase.Instantiation:
                    return PhaseInstantiation( ref cursor, state );
                case CursorPhase.Population:
                    return PhasePopulation( ref cursor, state );
                case CursorPhase.PostProcessing:
                    return PhasePostProcessing( ref cursor, state );
                default:
                    return StepResult.Finished;
            }
        }

        private StepResult PhasePreProcessing( ref SerializationCursor cursor, SerializationState state )
        {
            if( cursor.DataNode is SerializedObject rootObj && rootObj.TryGetValue( KeyNames.TYPE, out SerializedData typeData ) )
            {
                Type actualType = state.Context.TypeResolver.ResolveType( (string)(SerializedPrimitive)typeData );
                if( actualType != null )
                {
                    cursor.Descriptor = TypeDescriptorRegistry.GetDescriptor( actualType );
                }
            }

            if( cursor.Descriptor is ICollectionTypeDescriptor )
            {
                var unwrapped = SerializationHelpers.GetCollectionArrayNode( cursor.DataNode );
                if( unwrapped != null ) cursor.DataNode = unwrapped;
            }

            var compDesc = (ICompositeTypeDescriptor)cursor.Descriptor;
            cursor.ConstructionStepCount = compDesc.GetConstructionStepCount( cursor.Tracker.Target );

            if( cursor.DataNode is SerializedArray arrNode && cursor.Descriptor is ICollectionTypeDescriptor )
            {
                cursor.PopulationStepCount = arrNode.Count;
            }
            else
            {
                cursor.PopulationStepCount = compDesc.GetStepCount( cursor.Tracker.Target ) - cursor.ConstructionStepCount;
            }

            if( cursor.Tracker.Target == null && cursor.ConstructionStepCount > 0 )
            {
                cursor.ConstructionBuffer = new object[cursor.ConstructionStepCount];
                cursor.Phase = CursorPhase.Construction;
            }
            else
            {
                cursor.Phase = CursorPhase.Instantiation;
            }

            cursor.StepIndex = 0;
            return StepResult.Continue;
        }

        private StepResult PhaseConstruction( ref SerializationCursor cursor, SerializationState state )
        {
            var parentDesc = (ICompositeTypeDescriptor)cursor.Descriptor;

            while( cursor.StepIndex < cursor.ConstructionStepCount )
            {
                int activeIndex = cursor.StepIndex;
                IMemberInfo memberInfo = parentDesc.GetMemberInfo( activeIndex, cursor.ConstructionBuffer );

                if( memberInfo == null || memberInfo.TypeDescriptor == null )
                {
                    cursor.StepIndex++;
                    continue;
                }

                ResolveResult result = TryResolveMember( memberInfo, cursor.DataNode, activeIndex, state, out object val );

                if( result == ResolveResult.Resolved )
                {
                    object buffer = cursor.ConstructionBuffer;
                    memberInfo.SetValue( ref buffer, val );
                    cursor.StepIndex++;
                }
                else if( result == ResolveResult.RequiresPush )
                {
                    PushChildCursor( ref cursor, memberInfo, activeIndex, true, state );
                    return StepResult.PushedDependency;
                }
                else if( result == ResolveResult.Deferred )
                {
                    state.Context.DeferredOperations.Enqueue( new DeferredOperation
                    {
                        Target = cursor.Tracker.Parent,
                        Member = cursor.Tracker.Member,
                        Data = cursor.DataNode,
                        Descriptor = cursor.Descriptor,
                        ConstructionBuffer = cursor.ConstructionBuffer,
                        ConstructionIndex = cursor.StepIndex
                    } );
                    return StepResult.Deferred;
                }
                else
                {
                    UnityEngine.Debug.LogWarning( $"Failed to resolve construction argument {memberInfo.Name}" );
                    cursor.StepIndex++;
                }
            }

            cursor.Phase = CursorPhase.Instantiation;
            return StepResult.Continue;
        }

        private StepResult PhaseInstantiation( ref SerializationCursor cursor, SerializationState state )
        {
            var compositeDesc = (ICompositeTypeDescriptor)cursor.Descriptor;

            if( cursor.Tracker.Target == null )
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
                cursor.Tracker = cursor.Tracker.WithTarget( newInstance );
            }

            if( cursor.Tracker.Target == null ) throw new Exception( "Factory returned null." );

            if( cursor.Descriptor is ICollectionTypeDescriptor colDesc && cursor.DataNode is SerializedArray arr )
            {
                var resized = colDesc.Resize( cursor.Tracker.Target, arr.Count );
                cursor.Tracker = cursor.Tracker.WithTarget( resized );
            }

            if( cursor.DataNode is SerializedObject objNode
                && objNode.TryGetValue( KeyNames.ID, out var idData )
                && Guid.TryParse( (string)(SerializedPrimitive)idData, out Guid guid ) )
            {
                state.Context.ForwardMap.SetObj( guid, cursor.Tracker.Target );
            }

            if( cursor.Tracker.IsRoot && state.Stack.Count == 1 )
            {
                state.RootResult = cursor.Tracker.Target;
            }

            cursor.Phase = CursorPhase.Population;
            cursor.StepIndex = 0;
            return StepResult.Continue;
        }

        private StepResult PhasePopulation( ref SerializationCursor cursor, SerializationState state )
        {
            var parentDesc = (ICompositeTypeDescriptor)cursor.Descriptor;
            int totalSteps = cursor.PopulationStepCount;
            int offset = cursor.ConstructionStepCount;

            while( cursor.StepIndex < totalSteps )
            {
                int absoluteIndex = cursor.StepIndex + offset;
                IMemberInfo memberInfo = parentDesc.GetMemberInfo( absoluteIndex, cursor.Tracker.Target );

                if( memberInfo == null || memberInfo.TypeDescriptor == null )
                {
                    cursor.StepIndex++;
                    continue;
                }

                ResolveResult result = TryResolveMember( memberInfo, cursor.DataNode, absoluteIndex, state, out object val );

                if( result == ResolveResult.Resolved )
                {
                    object t = cursor.Tracker.Target;
                    memberInfo.SetValue( ref t, val );

                    // Update cursor target in case of value type replacement
                    cursor.Tracker = cursor.Tracker.WithTarget( t );
                    cursor.StepIndex++;
                }
                else if( result == ResolveResult.RequiresPush )
                {
                    PushChildCursor( ref cursor, memberInfo, absoluteIndex, false, state );
                    cursor.StepIndex++;
                    return StepResult.PushedDependency;
                }
                else if( result == ResolveResult.Deferred )
                {
                    SerializedData failedData = GetDataNode( cursor.DataNode, memberInfo.Name, absoluteIndex );
                    state.Context.EnqueueDeferred( cursor.Tracker.Target, memberInfo, failedData );
                    cursor.StepIndex++;
                }
                else
                {
                    cursor.StepIndex++;
                }
            }

            cursor.Phase = CursorPhase.PostProcessing;
            return StepResult.Continue;
        }

        private StepResult PhasePostProcessing( ref SerializationCursor cursor, SerializationState state )
        {
            if( cursor.Tracker.Target != null && cursor.Descriptor is ICompositeTypeDescriptor comp )
            {
                comp.OnDeserialized( cursor.Tracker.Target, state.Context );
            }
            return StepResult.Finished;
        }

        public void OnCursorFinished( SerializationCursor cursor, SerializationState state )
        {
        }

        private ResolveResult TryResolveMember( IMemberInfo memberInfo, SerializedData parentData, int index, SerializationState state, out object value )
        {
            value = null;
            ITypeDescriptor memberDesc = memberInfo.TypeDescriptor;

            if( memberDesc is IPrimitiveTypeDescriptor primitiveDesc )
            {
                SerializedData primitiveData = GetDataNode( parentData, memberInfo.Name, index );
                DeserializeResult result = primitiveDesc.DeserializeDirect( primitiveData, state.Context, out value );

                if( result == DeserializeResult.Success ) return ResolveResult.Resolved;
                if( result == DeserializeResult.Deferred ) return ResolveResult.Deferred;
                return ResolveResult.Failed;
            }

            SerializedData childNode = GetDataNode( parentData, memberInfo.Name, index );
            if( childNode == null )
            {
                value = null;
                return ResolveResult.Resolved;
            }

            ResolveResult refResult = TryResolveReference( childNode, state, out value );
            if( refResult == ResolveResult.Resolved ) return ResolveResult.Resolved;
            if( refResult == ResolveResult.Deferred ) return ResolveResult.Deferred;

            return ResolveResult.RequiresPush;
        }

        private ResolveResult TryResolveReference( SerializedData data, SerializationState state, out object value )
        {
            value = null;
            if( data is SerializedObject refObj && refObj.TryGetValue( KeyNames.REF, out var refVal ) )
            {
                if( refVal is SerializedPrimitive refPrim && Guid.TryParse( (string)refPrim, out Guid refGuid ) )
                {
                    if( state.Context.ForwardMap.TryGetObj( refGuid, out object existingObj ) )
                    {
                        value = existingObj;
                        return ResolveResult.Resolved;
                    }

                    if( state.Context.GlobalIdIndex.ContainsKey( refGuid ) )
                    {
                        return ResolveResult.Deferred;
                    }

                    throw new UPSMissingReferenceException( $"Missing Reference: {refGuid}" );
                }
            }
            return ResolveResult.Failed;
        }

        private void PushChildCursor( ref SerializationCursor parentCursor, IMemberInfo memberInfo, int absoluteIndex, bool isConstructionPhase, SerializationState state )
        {
            ITypeDescriptor memberDesc = memberInfo.TypeDescriptor;
            SerializedData childNode = GetDataNode( parentCursor.DataNode, memberInfo.Name, absoluteIndex );

            // Determine parent for the child. 
            // If construction phase, parent is the buffer. If population, parent is the actual target.
            object parentObj = isConstructionPhase ? (object)parentCursor.ConstructionBuffer : parentCursor.Tracker.Target;

            // Optimization: If not construction phase, and parent exists, try to get existing object (PopulateExisting)
            object existingChild = null;
            if( !isConstructionPhase && parentObj != null )
            {
                existingChild = memberInfo.GetValue( parentObj );
            }

            var childCursor = new SerializationCursor
            {
                Tracker = new TrackedObject( existingChild, parentObj, memberInfo ),
                Descriptor = memberDesc,
                DataNode = childNode,
                Phase = CursorPhase.PreProcessing,
                StepIndex = 0,
                WriteBackOnPop = true // Always write back deserialized children to their parents
            };

            state.Stack.Push( childCursor );
        }

        private SerializedData GetDataNode( SerializedData parent, string key, int index )
        {
            if( parent is SerializedObject obj && key != null && obj.TryGetValue( key, out var res ) ) return res;
            if( parent is SerializedArray arr && index >= 0 && index < arr.Count ) return arr[index];
            return null;
        }
    }
}
