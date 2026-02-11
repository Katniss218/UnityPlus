
using System;

namespace UnityPlus.Serialization
{
    public class SerializationStrategy : IOperationStrategy
    {
        public void InitializeRoot( object root, IDescriptor rootDescriptor, SerializedData rootData, SerializationState state )
        {
            // [Primitive Root Support]
            if( rootDescriptor is IPrimitiveDescriptor primitiveRoot )
            {
                SerializedData d = null;
                primitiveRoot.SerializeDirect( root, ref d, state.Context );
                state.RootResult = d;
                return; // Stack remains empty, IsFinished = true
            }

            // Create Root Node if needed
            SerializedData createdNode = rootData;
            if( createdNode == null )
            {
                if( rootDescriptor is ICollectionDescriptor )
                {
                    createdNode = SerializationHelpers.CreateCollectionNode(
                        root,
                        state.Context.ReverseMap,
                        state.Context.Config.ForceStandardJson,
                        out SerializedArray arrayNode );

                    // For collections, the DataNode in the cursor tracks the array we are populating
                    // But we store the wrapper (if exists) in RootResult
                    state.RootResult = createdNode;

                    // If we created a wrapper, the cursor data node should be the inner array
                    if( createdNode != arrayNode ) createdNode = arrayNode;
                }
                else
                {
                    var objNode = new SerializedObject();
                    if( root != null && !rootDescriptor.MappedType.IsValueType )
                    {
                        Guid rootId = state.Context.ReverseMap.GetID( root );
                        objNode[KeyNames.ID] = (SerializedPrimitive)rootId.ToString( "D" );
                    }
                    createdNode = objNode;
                    state.RootResult = createdNode;
                }
            }
            else
            {
                state.RootResult = rootData;
            }

            var cursor = new SerializationCursor
            {
                TargetObj = new TrackedObject( root ),
                Descriptor = rootDescriptor,
                StepIndex = 0,
                DataNode = createdNode,
                Phase = SerializationCursorPhase.PreProcessing,
                WriteBackOnPop = false // Serialization is read-only for the object graph
            };

            state.Stack.Push( cursor );
        }

        public SerializationCursorResult Process( ref SerializationCursor cursor, SerializationState state )
        {
            switch( cursor.Phase )
            {
                case SerializationCursorPhase.PreProcessing:
                    return PhasePreProcessing( ref cursor, state );
                case SerializationCursorPhase.Population:
                    return PhasePopulation( ref cursor, state );
                case SerializationCursorPhase.PostProcessing:
                    return SerializationCursorResult.Finished;
                default:
                    // Serialize doesn't use construction/instantiation
                    throw new Exception( $"Invalid cursor phase for serialization: {cursor.Phase}" );
            }
        }

        private SerializationCursorResult PhasePreProcessing( ref SerializationCursor cursor, SerializationState state )
        {
            if( cursor.TargetObj.Target != null )
            {
                // 1. Polymorphism
                if( cursor.DataNode is SerializedObject )
                {
                    HandlePolymorphism( cursor.TargetObj.Target, cursor.Descriptor.MappedType, cursor.DataNode, ref cursor.Descriptor );
                }

                // 2. Lifecycle Callback
                if( cursor.Descriptor is ICompositeDescriptor comp )
                {
                    comp.OnSerializing( cursor.TargetObj.Target, state.Context );
                }

                // 3. Cycle Tracking
                if( !cursor.Descriptor.MappedType.IsValueType )
                {
                    state.VisitedObjects.Add( cursor.TargetObj.Target );
                }

                // 4. Counts & Enumerators
                if( cursor.Descriptor is ICompositeDescriptor newComposite )
                {
                    cursor.MemberEnumerator = newComposite.GetMemberEnumerator( cursor.TargetObj.Target );
                    if( cursor.MemberEnumerator == null )
                    {
                        cursor.PopulationStepCount = newComposite.GetStepCount( cursor.TargetObj.Target );
                    }
                }
            }

            cursor.Phase = SerializationCursorPhase.Population;
            cursor.StepIndex = 0;
            return SerializationCursorResult.Jump; // Phase change
        }

        private SerializationCursorResult PhasePopulation( ref SerializationCursor cursor, SerializationState state )
        {
            IMemberInfo memberInfo = null;
            int activeStepIndex = cursor.StepIndex;

            if( cursor.MemberEnumerator != null )
            {
                if( !cursor.MemberEnumerator.MoveNext() )
                {
                    cursor.Phase = SerializationCursorPhase.PostProcessing;
                    return SerializationCursorResult.Jump;
                }
                memberInfo = cursor.MemberEnumerator.Current;
                // Note: For enumeration, we still increment StepIndex (Advance) to track count/progress, 
                // even though 'activeStepIndex' isn't used for lookups in this mode.
            }
            else
            {
                if( cursor.StepIndex >= cursor.PopulationStepCount )
                {
                    cursor.Phase = SerializationCursorPhase.PostProcessing;
                    return SerializationCursorResult.Jump;
                }

                var parentDesc = (ICompositeDescriptor)cursor.Descriptor;
                memberInfo = parentDesc.GetMemberInfo( activeStepIndex, cursor.TargetObj.Target );
            }

            if( memberInfo == null || memberInfo.TypeDescriptor == null ) // Skipped
            {
                return SerializationCursorResult.Advance;
            }

            IDescriptor memberDescriptor = memberInfo.TypeDescriptor;

            // 1. Primitive
            if( memberDescriptor is IPrimitiveDescriptor primitiveDesc )
            {
                object val = memberInfo.GetValue( cursor.TargetObj.Target );
                SerializedData primitiveData = null;
                primitiveDesc.SerializeDirect( val, ref primitiveData, state.Context );
                LinkDataNode( cursor.DataNode, memberInfo.Name, primitiveData, activeStepIndex );
                return SerializationCursorResult.Advance;
            }

            // 2. Composite
            object childTarget = memberInfo.GetValue( cursor.TargetObj.Target );

            // If the child is null, we just link null and don't push a cursor
            if( childTarget == null )
            {
                LinkDataNode( cursor.DataNode, memberInfo.Name, null, activeStepIndex );
                return SerializationCursorResult.Advance;
            }

            SerializedData childNode;
            SerializedData cursorDataNode;

            bool isCollection = memberDescriptor is ICollectionDescriptor;

            if( isCollection )
            {
                childNode = SerializationHelpers.CreateCollectionNode(
                    childTarget,
                    state.Context.ReverseMap,
                    state.Context.Config.ForceStandardJson,
                    out SerializedArray arrayNode );
                cursorDataNode = arrayNode;

                if( childNode is SerializedObject wrapper )
                {
                    if( state.VisitedObjects.Contains( childTarget ) )
                    {
                        Guid id = state.Context.ReverseMap.GetID( childTarget );
                        SerializedData refNode = new SerializedObject { { KeyNames.REF, (SerializedPrimitive)id.ToString( "D" ) } };
                        LinkDataNode( cursor.DataNode, memberInfo.Name, refNode, activeStepIndex );
                        return SerializationCursorResult.Advance;
                    }
                    state.VisitedObjects.Add( childTarget );
                }
            }
            else
            {
                var objNode = new SerializedObject();
                childNode = objNode;
                cursorDataNode = objNode;
            }

            if( childTarget != null )
            {
                Type actualType = childTarget.GetType();
                bool isRefType = !actualType.IsValueType;

                HandlePolymorphism( childTarget, memberInfo.MemberType, childNode, ref memberDescriptor );

                if( memberDescriptor is IPrimitiveDescriptor primitiveDescSwitched )
                {
                    SerializedData primitiveData = null;
                    primitiveDescSwitched.SerializeDirect( childTarget, ref primitiveData, state.Context );
                    LinkDataNode( cursor.DataNode, memberInfo.Name, primitiveData, activeStepIndex );
                    return SerializationCursorResult.Advance;
                }

                if( isRefType && !isCollection )
                {
                    Guid id = state.Context.ReverseMap.GetID( childTarget );

                    if( state.VisitedObjects.Contains( childTarget ) )
                    {
                        SerializedData refNode = new SerializedObject { { KeyNames.REF, (SerializedPrimitive)id.ToString( "D" ) } };
                        LinkDataNode( cursor.DataNode, memberInfo.Name, refNode, activeStepIndex );
                        return SerializationCursorResult.Advance;
                    }

                    state.VisitedObjects.Add( childTarget );

#warning TODO - use the v3 extension method for guid instead.
                    if( childNode is SerializedObject objNode )
                        objNode[KeyNames.ID] = (SerializedPrimitive)id.ToString( "D" );
                }
            }

            LinkDataNode( cursor.DataNode, memberInfo.Name, childNode, activeStepIndex );

            var childCursor = new SerializationCursor
            {
                TargetObj = new TrackedObject( childTarget, cursor.TargetObj.Target, memberInfo ),
                Descriptor = memberDescriptor,
                DataNode = cursorDataNode,
                StepIndex = 0,
                PopulationStepCount = (memberDescriptor as ICompositeDescriptor)?.GetStepCount( childTarget ) ?? 0,
                Phase = SerializationCursorPhase.PreProcessing,
                WriteBackOnPop = false // Serializer is read-only
            };

            state.Stack.Push( childCursor );
            return SerializationCursorResult.Push; // Driver will Increment StepIndex on Parent
        }

        public void OnCursorFinished( SerializationCursor cursor, SerializationState state )
        {
        }

        private void LinkDataNode( SerializedData parent, string key, SerializedData child, int index )
        {
            if( parent is SerializedObject obj && key != null ) obj[key] = child;
            else if( parent is SerializedArray arr )
            {
                if( index >= arr.Count ) arr.Add( child );
                else arr[index] = child;
            }
        }

        private void HandlePolymorphism( object target, Type declaredType, SerializedData dataNode, ref IDescriptor descriptor )
        {
            if( target == null ) return;
            Type actualType = target.GetType();

            if( actualType != declaredType )
            {
                descriptor = TypeDescriptorRegistry.GetDescriptor( actualType );
            }

            if( descriptor is IPrimitiveDescriptor ) return;
            if( declaredType.IsValueType || declaredType.IsSealed ) return;
            if( declaredType == actualType ) return;
            if( typeof( Delegate ).IsAssignableFrom( declaredType ) ) return;

#warning TODO - use the v3 extension method for types instead.
            if( dataNode is SerializedObject objNode )
                objNode[KeyNames.TYPE] = (SerializedPrimitive)actualType.AssemblyQualifiedName;
        }
    }
}