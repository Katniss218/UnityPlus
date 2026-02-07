using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnityPlus.Serialization
{
    public enum SerializationOperation
    {
        Serialize,
        Deserialize
    }

    public class StackMachineDriver
    {
        private enum StepResult
        {
            Continue,           // Keep processing the current cursor
            Finished,           // Pop the current cursor
            Paused,             // Exit the tick loop (time budget)
            PushedDependency,   // A new cursor was pushed; loop should restart to process top
            Deferred            // The current cursor cannot proceed due to missing deps; pop it (it's queued)
        }

        // Internal result for member resolution
        private enum ResolveResult
        {
            Resolved,
            RequiresPush,
            Deferred,
            Failed
        }

        private readonly Stack<SerializationCursor> _stack = new Stack<SerializationCursor>( 64 );
        private readonly SerializationContext _context;
        private readonly Stopwatch _timer = new Stopwatch();

        public bool IsFinished => _stack.Count == 0 && _context.DeferredOperations.Count == 0;

        /// <summary>
        /// The final result of the operation.
        /// </summary>
        public object Result { get; private set; }

        public StackMachineDriver( SerializationContext context )
        {
            _context = context;
        }

        public void Initialize( object root, ITypeDescriptor rootDescriptor, SerializationOperation op, SerializedData rootData = null )
        {
            _stack.Clear();
            _context.DeferredOperations.Clear();
            Result = root;

            // [Root Polymorphism Support]
            if( op == SerializationOperation.Deserialize && rootData is SerializedObject rootObj && rootObj.TryGetValue( KeyNames.TYPE, out SerializedData typeData ) )
            {
                Type actualType = _context.TypeResolver.ResolveType( (string)typeData );
                if( actualType != null )
                {
                    rootDescriptor = TypeDescriptorRegistry.GetDescriptor( actualType );
                }
            }

            if( !(rootDescriptor is ICompositeTypeDescriptor compositeDesc) )
                throw new ArgumentException( "Root descriptor must be an ICompositeTypeDescriptor." );

            SerializedData cursorDataNode = rootData;

            // [Deserialize] Index building and Collection unwrapping
            if( op == SerializationOperation.Deserialize )
            {
                if( rootData != null ) BuildIdIndex( rootData );

                // Auto-Unwrap: If descriptor is collection but data is object, extract "values" array
                if( rootDescriptor is ICollectionTypeDescriptor )
                {
                    if( rootData is SerializedObject boxed && boxed.TryGetValue( "values", out SerializedData inner ) )
                    {
                        cursorDataNode = inner;
                    }
                }

                if( cursorDataNode is SerializedArray arr && rootDescriptor is ICollectionTypeDescriptor colDesc && root != null )
                    root = colDesc.Resize( root, arr.Count );
            }

            var cursor = new SerializationCursor
            {
                Target = root,
                Descriptor = rootDescriptor,
                StepIndex = 0,
                ConstructionStepCount = compositeDesc.GetConstructionStepCount( root ),
                PopulationStepCount = compositeDesc.GetStepCount( root ) - compositeDesc.GetConstructionStepCount( root ),
                DataNode = cursorDataNode,
                NeedsWriteBack = false,
                Phase = CursorPhase.Population // Default to Population unless we determine otherwise below
            };

            // [Serialize Only] Create Root Data Node
            if( op == SerializationOperation.Serialize && rootData == null )
            {
                SerializedData createdNode;

                if( rootDescriptor is ICollectionTypeDescriptor )
                {
                    var arrayNode = new SerializedArray();

                    if( !_context.ForceStandardJson && root != null )
                    {
                        // Boxed Collection: Root Node is Object, Inner Node (Cursor Data) is Array
                        var wrapperNode = new SerializedObject();
                        Guid rootId = _context.ReverseMap.GetID( root );
                        wrapperNode[KeyNames.ID] = rootId.ToString( "D" );
                        wrapperNode["values"] = arrayNode;

                        createdNode = wrapperNode; // Result will be the wrapper
                        cursor.DataNode = arrayNode; // Cursor iterates the array
                    }
                    else
                    {
                        // Standard: Root Node is Array
                        createdNode = arrayNode;
                        cursor.DataNode = arrayNode;
                    }
                }
                else
                {
                    var objNode = new SerializedObject();
                    if( root != null && !rootDescriptor.WrappedType.IsValueType )
                    {
                        Guid rootId = _context.ReverseMap.GetID( root );
                        objNode[KeyNames.ID] = rootId.ToString( "D" );
                    }
                    createdNode = objNode;
                    cursor.DataNode = objNode;
                }

                // If Result was requested, it points to the top-level node (Wrapper or Object)
                if( Result == null || Result == root ) Result = createdNode;

                if( root != null )
                {
                    // Handle Polymorphism (Applies to the wrapper/object level)
                    HandlePolymorphism( root, rootDescriptor.WrappedType, createdNode, ref cursor.Descriptor );

                    // Update cursor counts if descriptor changed due to polymorphism
                    if( cursor.Descriptor != rootDescriptor && cursor.Descriptor is ICompositeTypeDescriptor newComposite )
                    {
                        cursor.ConstructionStepCount = newComposite.GetConstructionStepCount( root );
                        cursor.PopulationStepCount = newComposite.GetStepCount( root ) - cursor.ConstructionStepCount;
                    }
                }
            }
            // [Deserialize Only] Setup Construction Phase / Collection Counts
            else if( op == SerializationOperation.Deserialize )
            {
                if( root == null && cursor.ConstructionStepCount > 0 )
                {
                    cursor.Phase = CursorPhase.Construction;
                    cursor.ConstructionBuffer = new object[cursor.ConstructionStepCount];
                }

                // Fix for Dictionary/Collections: If data is an array, rely on its count for population steps
                if( cursor.DataNode is SerializedArray arr && rootDescriptor is ICollectionTypeDescriptor )
                {
                    cursor.PopulationStepCount = arr.Count;
                }
            }

            _stack.Push( cursor );
        }

        private void BuildIdIndex( SerializedData rootData )
        {
            if( rootData == null ) return;

            var stack = new Stack<SerializedData>();
            stack.Push( rootData );

            while( stack.Count > 0 )
            {
                var data = stack.Pop();

                if( data is SerializedObject obj )
                {
                    if( obj.TryGetValue( KeyNames.ID, out SerializedData idVal ) && idVal is SerializedPrimitive primId && Guid.TryParse( (string)primId, out Guid guid ) )
                    {
                        if( !_context.GlobalIdIndex.ContainsKey( guid ) )
                            _context.GlobalIdIndex[guid] = obj;
                    }

                    foreach( var child in obj.Values )
                    {
                        if( child != null ) stack.Push( child );
                    }
                }
                else if( data is SerializedArray arr )
                {
                    foreach( var child in arr )
                    {
                        if( child != null ) stack.Push( child );
                    }
                }
            }
        }

        public void Tick( float timeBudgetMs, SerializationOperation op )
        {
            _timer.Restart();
            bool isSerializing = op == SerializationOperation.Serialize;

            // --- PASS 1: Main Stack Processing ---
            while( _stack.Count > 0 )
            {
                if( _timer.ElapsedMilliseconds > timeBudgetMs ) return;

                // Peek, because we might push a child on top
                // We only Pop when 'Finished'
                SerializationCursor currentCursor = _stack.Peek();
                StepResult result;

                if( isSerializing )
                {
                    result = StepSerialize( ref currentCursor );
                }
                else
                {
                    if( currentCursor.Phase == CursorPhase.Construction )
                        result = ProcessConstructionPhase( ref currentCursor );
                    else
                        result = ProcessPopulationPhase( ref currentCursor );
                }

                // Apply updates to the stack state (since struct is copied on Peek)
                if( result == StepResult.Continue )
                {
                    _stack.Pop();
                    _stack.Push( currentCursor );
                }
                else if( result == StepResult.Finished )
                {
                    _stack.Pop();
                    HandlePop( currentCursor, isSerializing );
                }
                else if( result == StepResult.PushedDependency )
                {
                    // Update the cursor state (e.g. paused at index) but keep it on stack
                    // The new dependency is already on top thanks to ProcessConstructionPhase
                    var newTop = _stack.Pop(); // The dependency
                    _stack.Pop(); // The old parent state
                    _stack.Push( currentCursor ); // Push parent back
                    _stack.Push( newTop ); // Push dependency back on top
                }
                else if( result == StepResult.Deferred )
                {
                    // The current cursor failed to proceed and has enqueued itself.
                    _stack.Pop();

                    // Cascade Deferral:
                    // If the parent was waiting for this child (Construction Phase), the parent must ALSO defer.
                    while( _stack.Count > 0 )
                    {
                        var parent = _stack.Peek();
                        if( parent.Phase == CursorPhase.Construction )
                        {
                            // Parent is in construction -> Cannot proceed without child -> Defer Parent
                            _context.DeferredOperations.Enqueue( new DeferredOperation
                            {
                                Target = parent.ParentTarget,
                                Member = parent.ParentMemberInfo,
                                Data = parent.DataNode,
                                Descriptor = parent.Descriptor,
                                ConstructionBuffer = parent.ConstructionBuffer,
                                ConstructionIndex = parent.StepIndex
                            } );
                            _stack.Pop();
                        }
                        else
                        {
                            // Parent is Populating -> Can handle missing child by skipping member -> Advance Step
                            var p = _stack.Pop();
                            p.StepIndex++;
                            _stack.Push( p );
                            break; // Stop cascading
                        }
                    }
                }
            }

            // --- PASS 2: Deferred Resolution ---
            if( _stack.Count == 0 && op == SerializationOperation.Deserialize )
            {
                ProcessDeferredQueue( timeBudgetMs );
            }
        }

        private StepResult ProcessConstructionPhase( ref SerializationCursor cursor )
        {
            var parentDesc = (ICompositeTypeDescriptor)cursor.Descriptor;

            // 1. Process all constructor arguments
            while( cursor.StepIndex < cursor.ConstructionStepCount )
            {
                int activeIndex = cursor.StepIndex;
                IMemberInfo memberInfo = parentDesc.GetMemberInfo( activeIndex, cursor.ConstructionBuffer ); // Pass buffer as target

                if( memberInfo == null || memberInfo.TypeDescriptor == null ) // Skipped or Invalid
                {
                    cursor.StepIndex++;
                    continue;
                }

                ResolveResult result = TryResolveMember( memberInfo, cursor.DataNode, activeIndex, out object val );

                if( result == ResolveResult.Resolved )
                {
                    memberInfo.SetValue( ref cursor.ConstructionBuffer[0], val ); // Hack: BufferMemberInfo handles array indexing
                    cursor.StepIndex++;
                }
                else if( result == ResolveResult.RequiresPush )
                {
                    // Push Child Object
                    PushChildCursor( ref cursor, memberInfo, activeIndex, true );
                    return StepResult.PushedDependency;
                }
                else if( result == ResolveResult.Deferred )
                {
                    // Missing dependency during construction.
                    // We cannot finish this object now.
                    // Defer the entire construction cursor state.
                    _context.DeferredOperations.Enqueue( new DeferredOperation
                    {
                        Target = cursor.ParentTarget,
                        Member = cursor.ParentMemberInfo,
                        Data = cursor.DataNode,
                        Descriptor = cursor.Descriptor,
                        ConstructionBuffer = cursor.ConstructionBuffer,
                        ConstructionIndex = cursor.StepIndex
                    } );

                    return StepResult.Deferred;
                }
                else // Failed
                {
                    UnityEngine.Debug.LogWarning( $"Failed to resolve construction argument {memberInfo.Name}" );
                    cursor.StepIndex++;
                }
            }

            // 2. All args ready -> Instantiate
            object instance = parentDesc.Construct( cursor.ConstructionBuffer );
            if( instance == null ) throw new Exception( "Factory returned null." );

            cursor.Target = instance;
            cursor.Phase = CursorPhase.Population;
            cursor.StepIndex = 0; // Reset index for Population phase (relative to 0)

            // 3. Register ID immediately (for circular refs)
            // Note: If boxed collection, we need to check if the ID is on the parent (wrapper) or current node.
            // But cursor.DataNode usually points to the inner array for collections. 
            // However, GlobalIdIndex maps ID -> Wrapper.
            // For standard objects, DataNode is the object.

            // Check if we have an ID for this object instance via Reverse lookup in context (only helps if we knew the ID beforehand, which we don't for construction).
            // Actually, we need to look at the DataNode's ID.
            // If it's a collection, the DataNode is the Array, which has no ID. The ID was on the wrapper.
            // In v4 Deserialize, we don't hold a reference to the wrapper in the cursor easily.
            // However, we populated GlobalIdIndex. If we found this object via reference, it's fine.
            // If we are just instantiating, we need to register it so subsequent refs can find it.
            // This logic is tricky for unwrapped collections.
            // Simplify: If DataNode has ID, register.
            if( cursor.DataNode is SerializedObject objNode
                && objNode.TryGetValue( KeyNames.ID, out var idData )
                && Guid.TryParse( (string)idData, out Guid guid ) )
            {
                _context.ForwardMap.SetObj( guid, instance );
            }

            return StepResult.Continue;
        }

        private StepResult ProcessPopulationPhase( ref SerializationCursor cursor )
        {
            var parentDesc = (ICompositeTypeDescriptor)cursor.Descriptor;
            int totalSteps = cursor.PopulationStepCount;
            // The step index in cursor is 0-based for Population. 
            // But GetMemberInfo might expect absolute index if Ctor args are included.
            int offset = cursor.ConstructionStepCount;

            while( cursor.StepIndex < totalSteps )
            {
                int absoluteIndex = cursor.StepIndex + offset;
                IMemberInfo memberInfo = parentDesc.GetMemberInfo( absoluteIndex, cursor.Target );

                if( memberInfo == null || memberInfo.TypeDescriptor == null ) // Skipped member (e.g. conditional)
                {
                    cursor.StepIndex++;
                    continue;
                }

                ResolveResult result = TryResolveMember( memberInfo, cursor.DataNode, absoluteIndex, out object val );

                if( result == ResolveResult.Resolved )
                {
                    object t = cursor.Target;
                    memberInfo.SetValue( ref t, val );
                    cursor.Target = t; // Update target in case of struct
                    cursor.StepIndex++;
                }
                else if( result == ResolveResult.RequiresPush )
                {
                    PushChildCursor( ref cursor, memberInfo, absoluteIndex, false );
                    cursor.StepIndex++; // We assume child will write back successfully or be deferred.
                    return StepResult.PushedDependency;
                }
                else if( result == ResolveResult.Deferred )
                {
                    // In Population phase, we CAN defer just this member.
                    SerializedData failedData = GetDataNode( cursor.DataNode, memberInfo.Name, absoluteIndex );
                    _context.EnqueueDeferred( cursor.Target, memberInfo, failedData );
                    cursor.StepIndex++;
                }
                else
                {
                    // Failed (handled silently or logged)
                    cursor.StepIndex++;
                }
            }
            return StepResult.Finished;
        }

        private void PushChildCursor( ref SerializationCursor parentCursor, IMemberInfo memberInfo, int absoluteIndex, bool isConstructionPhase )
        {
            ITypeDescriptor memberDesc = memberInfo.TypeDescriptor;
            SerializedData childNode = GetDataNode( parentCursor.DataNode, memberInfo.Name, absoluteIndex );

            // Try to use existing object (e.g. Unity Component) unless we are in construction phase (where no target exists yet)
            object childTarget = null;
            if( !isConstructionPhase && parentCursor.Target != null )
            {
                childTarget = memberInfo.GetValue( parentCursor.Target );
            }

            // If no existing object or we are constructing, create from data
            if( childTarget == null )
            {
                childTarget = memberDesc.CreateInitialTarget( childNode, _context );
            }

            // Register early if possible (ID check)
            if( memberDesc is ICompositeTypeDescriptor comp && comp.GetConstructionStepCount( childTarget ) == 0 )
            {
                // Handle standard objects
                if( childNode is SerializedObject idObj && idObj.TryGetValue( KeyNames.ID, out SerializedData idVal ) )
                {
                    if( Guid.TryParse( (string)idVal, out Guid objId ) )
                        _context.ForwardMap.SetObj( objId, childTarget );
                }
                // Handle boxed collections where childNode is the array inside the wrapper
                // (This is hard to reach here because we passed the childNode, which is the array itself)
                // But CreateInitialTarget for array might return the array instance.
                // If the array was boxed, the ID is on the parent property's value (the wrapper).
                // But TryResolveMember extracts the value directly.
                // This edge case (circular ref to collection during load) is handled by GlobalIdIndex deferral usually.
            }

            // Handle Collection Unwrapping for Child Nodes
            SerializedData cursorDataNode = childNode;
            if( memberDesc is ICollectionTypeDescriptor )
            {
                if( childNode is SerializedObject boxed && boxed.TryGetValue( "values", out SerializedData inner ) )
                {
                    cursorDataNode = inner;
                }
            }

            var childCursor = CreateCursor( childTarget, memberDesc, cursorDataNode );

            // IMPORTANT: We almost always need write-back for Deserialize to ensure the created object is actually assigned to the parent.
            // Exception: If we populated an existing Reference Type object (childTarget != null), assignment might be redundant but is safe.
            childCursor.NeedsWriteBack = true;

            // Write back target: If constructing, write to buffer. Else write to target.
            childCursor.ParentTarget = isConstructionPhase ? (object)parentCursor.ConstructionBuffer : parentCursor.Target;
            childCursor.ParentMemberInfo = memberInfo;
            _stack.Push( childCursor );
        }

        private ResolveResult TryResolveMember( IMemberInfo memberInfo, SerializedData parentData, int index, out object value )
        {
            value = null;
            ITypeDescriptor memberDesc = memberInfo.TypeDescriptor;

            if( memberDesc is IPrimitiveTypeDescriptor primitiveDesc )
            {
                SerializedData primitiveData = GetDataNode( parentData, memberInfo.Name, index );
                DeserializeResult result = primitiveDesc.DeserializeDirect( primitiveData, _context, out value );

                if( result == DeserializeResult.Success ) return ResolveResult.Resolved;
                if( result == DeserializeResult.Deferred ) return ResolveResult.Deferred;
                return ResolveResult.Failed;
            }

            // It's a composite (class/struct/collection)
            SerializedData childNode = GetDataNode( parentData, memberInfo.Name, index );
            if( childNode == null )
            {
                // Null composite object
                value = null;
                return ResolveResult.Resolved;
            }
            return ResolveResult.RequiresPush;
        }

        private SerializationCursor CreateCursor( object target, ITypeDescriptor desc, SerializedData data )
        {
            var compDesc = desc as ICompositeTypeDescriptor;
            int ctorCount = compDesc?.GetConstructionStepCount( target ) ?? 0;
            int totalCount = compDesc?.GetStepCount( target ) ?? 0;

            // Fix for collections: If we are deserializing a collection, we iterate the data count,
            // regardless of the target's current capacity.
            if( desc is ICollectionTypeDescriptor && data is SerializedArray arr )
            {
                totalCount = arr.Count;
            }

            var cursor = new SerializationCursor
            {
                Target = target,
                Descriptor = desc,
                DataNode = data,
                ConstructionStepCount = ctorCount,
                PopulationStepCount = totalCount - ctorCount,
                StepIndex = 0,
                Phase = ctorCount > 0 ? CursorPhase.Construction : CursorPhase.Population,
                NeedsWriteBack = false
            };

            if( cursor.Phase == CursorPhase.Construction )
            {
                cursor.ConstructionBuffer = new object[ctorCount];
            }

            return cursor;
        }

        private void ProcessDeferredQueue( float timeBudgetMs )
        {
            int count = _context.DeferredOperations.Count;
            for( int i = 0; i < count; i++ )
            {
                if( _timer.ElapsedMilliseconds > timeBudgetMs ) return;

                var op = _context.DeferredOperations.Dequeue();

                // 1. Resume Interrupted Construction
                if( op.ConstructionBuffer != null )
                {
                    // Recreate cursor in Construction Phase
                    var cursor = new SerializationCursor
                    {
                        Target = null, // Constructing
                        Descriptor = op.Descriptor,
                        DataNode = op.Data,
                        ConstructionBuffer = op.ConstructionBuffer,
                        StepIndex = op.ConstructionIndex,
                        Phase = CursorPhase.Construction,

                        ParentTarget = op.Target,
                        ParentMemberInfo = op.Member,
                        NeedsWriteBack = op.Member != null // Must write back buffer
                    };

                    var comp = (ICompositeTypeDescriptor)op.Descriptor;
                    // We use the buffer to determine step count (important for delegates)
                    cursor.ConstructionStepCount = comp.GetConstructionStepCount( op.ConstructionBuffer );
                    cursor.PopulationStepCount = comp.GetStepCount( op.ConstructionBuffer ) - cursor.ConstructionStepCount;

                    _stack.Push( cursor );
                }
                // 2. Root Deferral (New Object)
                else if( op.Member == null )
                {
                    object target = op.Descriptor.CreateInitialTarget( op.Data, _context );
                    var cursor = CreateCursor( target, op.Descriptor, op.Data );
                    _stack.Push( cursor );
                }
                // 3. Member Deferral (Retry Dependency)
                else
                {
                    // This logic handles a deferred member in Population Phase.

                    // Direct handling for deferred items since TryResolveMember expects parentData
                    ITypeDescriptor desc = op.Member.TypeDescriptor;

                    if( desc is IPrimitiveTypeDescriptor prim )
                    {
                        var res = prim.DeserializeDirect( op.Data, _context, out object value );
                        if( res == DeserializeResult.Success )
                        {
                            object t = op.Target;
                            op.Member.SetValue( ref t, value );
                        }
                        else
                        {
                            // Still deferred or failed - Re-queue
                            _context.DeferredOperations.Enqueue( op );
                        }
                    }
                    else
                    {
                        // Composite - Start creating the child object
                        object childTarget = desc.CreateInitialTarget( op.Data, _context );
                        var cursor = CreateCursor( childTarget, desc, op.Data );
                        cursor.NeedsWriteBack = true;
                        cursor.ParentTarget = op.Target;
                        cursor.ParentMemberInfo = op.Member;
                        _stack.Push( cursor );
                    }
                }
            }
        }

        private StepResult StepSerialize( ref SerializationCursor cursor )
        {
            var parentDesc = (ICompositeTypeDescriptor)cursor.Descriptor;

            if( cursor.StepIndex == 0 )
            {
                parentDesc.OnSerializing( cursor.Target, _context );
            }

            if( cursor.StepIndex >= cursor.PopulationStepCount ) // Use generic StepCount for serialize
            {
                return StepResult.Finished;
            }

            int activeStepIndex = cursor.StepIndex;
            IMemberInfo memberInfo = parentDesc.GetMemberInfo( activeStepIndex, cursor.Target );

            if( memberInfo == null || memberInfo.TypeDescriptor == null ) // Skipped
            {
                cursor.StepIndex++;
                return StepResult.Continue;
            }

            ITypeDescriptor memberDescriptor = memberInfo.TypeDescriptor;

            cursor.StepIndex++;

            if( memberDescriptor is IPrimitiveTypeDescriptor primitiveDesc )
            {
                object val = memberInfo.GetValue( cursor.Target );
                SerializedData primitiveData = null;
                primitiveDesc.SerializeDirect( val, ref primitiveData, _context );
                LinkDataNode( cursor.DataNode, memberInfo.Name, primitiveData, activeStepIndex );
                return StepResult.Continue;
            }

            object childTarget = memberInfo.GetValue( cursor.Target );

            SerializedData childNode;
            SerializedData cursorDataNode; // The node the child cursor will iterate (Array for collections)

            bool isCollection = memberDescriptor is ICollectionTypeDescriptor;

            if( isCollection )
            {
                var arrayNode = new SerializedArray();
                if( !_context.ForceStandardJson && childTarget != null )
                {
                    // Box it: Wrapper -> Array
                    var wrapper = new SerializedObject();
                    Guid id = _context.ReverseMap.GetID( childTarget );
                    wrapper[KeyNames.ID] = id.ToString( "D" );
                    wrapper["values"] = arrayNode;

                    childNode = wrapper; // Link wrapper to parent
                    cursorDataNode = arrayNode; // Cursor iterates array
                }
                else
                {
                    // Standard: Array
                    childNode = arrayNode;
                    cursorDataNode = arrayNode;
                }
            }
            else
            {
                var objNode = new SerializedObject();
                childNode = objNode;
                cursorDataNode = objNode;
            }

            // Note: We don't link childNode immediately if there's a chance we swap to primitive serialization (boxed value)

            if( childTarget != null )
            {
                Type actualType = childTarget.GetType();

                // 1. Polymorphism Update
                // This might change memberDescriptor from 'ObjectDescriptor' to 'IntDescriptor' (Boxed Value)
                HandlePolymorphism( childTarget, memberInfo.MemberType, childNode, ref memberDescriptor );

                // 2. Check for Boxed Primitive (Polymorphism switch result)
                if( memberDescriptor is IPrimitiveTypeDescriptor primitiveDescSwitched )
                {
                    // We must discard the 'SerializedObject' we created and serialize as primitive directly.
                    // Boxed values cannot have IDs or $type headers in this format (SerializedPrimitive).
                    SerializedData primitiveData = null;
                    primitiveDescSwitched.SerializeDirect( childTarget, ref primitiveData, _context );
                    LinkDataNode( cursor.DataNode, memberInfo.Name, primitiveData, activeStepIndex );
                    return StepResult.Continue;
                }

                // 3. Write ID (If not Value Type and not already handled by Collection Boxing)
                if( !actualType.IsValueType && !isCollection )
                {
                    Guid id = _context.ReverseMap.GetID( childTarget );
                    if( childNode is SerializedObject objNode )
                        objNode[KeyNames.ID] = id.ToString( "D" );
                }
            }

            // Link Object/Array Node
            LinkDataNode( cursor.DataNode, memberInfo.Name, childNode, activeStepIndex );

            var childCursor = CreateCursor( childTarget, memberDescriptor, cursorDataNode );
            // Serialize only has one phase
            childCursor.Phase = CursorPhase.Population;
            childCursor.PopulationStepCount = (memberDescriptor as ICompositeTypeDescriptor)?.GetStepCount( childTarget ) ?? 0;

            _stack.Push( childCursor );
            return StepResult.PushedDependency;
        }

        private void HandlePolymorphism( object target, Type declaredType, SerializedData dataNode, ref ITypeDescriptor descriptor )
        {
            if( target == null ) return;
            Type actualType = target.GetType();

            if( actualType != declaredType )
            {
                descriptor = TypeDescriptorRegistry.GetDescriptor( actualType );
            }

            // If it swapped to primitive, don't write headers
            if( descriptor is IPrimitiveTypeDescriptor ) return;

            // Constraint: Variable type is Value Type or Sealed Class -> No $type
            if( declaredType.IsValueType || declaredType.IsSealed ) return;

            // Constraint: Types match -> No $type
            if( declaredType == actualType ) return;

            // Constraint: Variable type is Delegate -> No $type
            if( typeof( Delegate ).IsAssignableFrom( declaredType ) ) return;

            if( dataNode is SerializedObject objNode )
                objNode[KeyNames.TYPE] = (SerializedPrimitive)actualType.AssemblyQualifiedName;
        }

        private void HandlePop( SerializationCursor finishedCursor, bool isSerializing )
        {
            if( finishedCursor.NeedsWriteBack && finishedCursor.ParentTarget != null )
            {
                object parent = finishedCursor.ParentTarget;

                // If parent is a buffer (Construction Phase), we index it directly (BufferMemberInfo handles logic)
                // If parent is an object (Population Phase), we use SetValue
                finishedCursor.ParentMemberInfo.SetValue( ref parent, finishedCursor.Target );

                // If the parent was a boxed struct, we need to update the parent's cursor target
                if( !ReferenceEquals( parent, finishedCursor.ParentTarget ) && _stack.Count > 0 )
                {
                    var parentCursor = _stack.Pop();
                    if( ReferenceEquals( parentCursor.Target, finishedCursor.ParentTarget ) )
                    {
                        parentCursor.Target = parent;
                    }
                    _stack.Push( parentCursor );
                }
            }

            if( !isSerializing && finishedCursor.Target != null && finishedCursor.Descriptor is ICompositeTypeDescriptor comp )
            {
                comp.OnDeserialized( finishedCursor.Target, _context );
            }

            if( _stack.Count == 0 && finishedCursor.ParentTarget == null )
            {
                Result = finishedCursor.Target;
            }
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

        private SerializedData GetDataNode( SerializedData parent, string key, int index )
        {
            if( parent is SerializedObject obj && key != null && obj.TryGetValue( key, out var res ) ) return res;
            if( parent is SerializedArray arr && index >= 0 && index < arr.Count ) return arr[index];
            return null;
        }
    }
}
