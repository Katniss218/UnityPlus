
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnityPlus.Serialization
{
    public class StackMachineDriver
    {
        private SerializationState _state;
        private IOperationStrategy _strategy;
        private readonly Stopwatch _timer = new Stopwatch();

        public bool IsFinished => _state != null && _state.Stack.Count == 0 && _state.Context.DeferredOperations.Count == 0;

        /// <summary>
        /// The final result of the operation.
        /// </summary>
        public object Result => _state?.RootResult;

        public StackMachineDriver( SerializationContext context )
        {
            _state = new SerializationState( context );
        }

        /// <summary>
        /// Initializes the driver with a specific strategy.
        /// </summary>
        public void Initialize( object root, ITypeDescriptor rootDescriptor, IOperationStrategy strategy, SerializedData rootData = null )
        {
            if( strategy == null ) throw new ArgumentNullException( nameof( strategy ) );

            _state.Stack.Clear();
            _state.Context.DeferredOperations.Clear();
            _state.VisitedObjects.Clear();
            _state.RootResult = null;

            _strategy = strategy;

            try
            {
                _strategy.InitializeRoot( root, rootDescriptor, rootData, _state );
            }
            catch( Exception ex )
            {
                HandleFatalException( ex, "Initialization" );
            }
        }

        public void Tick( float timeBudgetMs )
        {
            _timer.Restart();

            // --- PASS 1: Main Stack Processing ---
            while( _state.Stack.Count > 0 )
            {
                if( _timer.ElapsedMilliseconds > timeBudgetMs ) return;

                try
                {
                    SerializationCursor currentCursor = _state.Stack.Peek();
                    StepResult result = _strategy.Process( ref currentCursor, _state );

                    // Apply updates to the stack state
                    if( result == StepResult.Continue )
                    {
                        // Apply phase/index updates
                        var popped = _state.Stack.Discard(); // Pop old state (Discard prevents writeback)
                        _state.Stack.Push( currentCursor ); // Push new state
                    }
                    else if( result == StepResult.Finished )
                    {
                        var finishedCursor = _state.Stack.Pop(); // Handles WriteBack
                        _strategy.OnCursorFinished( finishedCursor, _state );
                    }
                    else if( result == StepResult.PushedDependency )
                    {
                        var newTop = _state.Stack.Discard(); // The dependency
                        var parent = _state.Stack.Discard(); // The old parent state

                        _state.Stack.Push( currentCursor ); // Push updated parent back
                        _state.Stack.Push( newTop ); // Push dependency back on top
                    }
                    else if( result == StepResult.Deferred )
                    {
                        _state.Stack.Discard(); // Discard from stack

                        // Cascade Deferral Logic
                        while( _state.Stack.Count > 0 )
                        {
                            var parent = _state.Stack.Peek();
                            if( parent.Phase == CursorPhase.Construction )
                            {
                                // Parent is in construction -> Cannot proceed without child -> Defer Parent
                                _state.Context.DeferredOperations.Enqueue( new DeferredOperation
                                {
                                    Target = parent.Tracker.Parent, // The grandparent
                                    Member = parent.Tracker.Member,
                                    Data = parent.DataNode,
                                    Descriptor = parent.Descriptor,
                                    ConstructionBuffer = parent.ConstructionBuffer,
                                    ConstructionIndex = parent.StepIndex
                                } );
                                _state.Stack.Discard();
                            }
                            else
                            {
                                // Parent is Populating -> Can handle missing child by skipping member -> Advance Step
                                var p = _state.Stack.Discard();
                                p.StepIndex++;
                                _state.Stack.Push( p );
                                break;
                            }
                        }
                    }
                }
                catch( Exception ex )
                {
                    HandleFatalException( ex, "Stack Processing" );
                }
            }

            // --- PASS 2: Deferred Resolution ---
            if( _state.Stack.Count == 0 && _state.Context.DeferredOperations.Count > 0 )
            {
                ProcessDeferredQueue( timeBudgetMs );
            }
        }

        private void ProcessDeferredQueue( float timeBudgetMs )
        {
            int count = _state.Context.DeferredOperations.Count;
            if( count == 0 ) return;

            bool progressMade = false;
            int processedCount = 0;

            for( int i = 0; i < count; i++ )
            {
                if( _timer.ElapsedMilliseconds > timeBudgetMs ) return;

                var op = _state.Context.DeferredOperations.Dequeue();
                processedCount++;

                bool opSuccess = false;

                try
                {
                    // 1. Resume Interrupted Construction
                    if( op.ConstructionBuffer != null )
                    {
                        var cursor = new SerializationCursor
                        {
                            Tracker = new TrackedObject( null, op.Target, op.Member ),
                            Descriptor = op.Descriptor,
                            DataNode = op.Data,
                            ConstructionBuffer = op.ConstructionBuffer,
                            StepIndex = op.ConstructionIndex,
                            Phase = CursorPhase.Construction,
                        };

                        var comp = (ICompositeTypeDescriptor)op.Descriptor;
                        cursor.ConstructionStepCount = comp.GetConstructionStepCount( op.ConstructionBuffer );
                        cursor.PopulationStepCount = comp.GetStepCount( op.ConstructionBuffer ) - cursor.ConstructionStepCount;

                        _state.Stack.Push( cursor );
                        opSuccess = true;
                    }
                    // 2. Root Deferral
                    else if( op.Member == null )
                    {
                        var cursor = new SerializationCursor
                        {
                            Tracker = new TrackedObject( null ),
                            Descriptor = op.Descriptor,
                            DataNode = op.Data,
                            Phase = CursorPhase.PreProcessing,
                            StepIndex = 0
                        };
                        _state.Stack.Push( cursor );
                        opSuccess = true;
                    }
                    // 3. Member Deferral
                    else
                    {
                        ITypeDescriptor desc = op.Member.TypeDescriptor;

                        if( desc is IPrimitiveTypeDescriptor prim )
                        {
                            var res = prim.DeserializeDirect( op.Data, _state.Context, out object value );
                            if( res == DeserializeResult.Success )
                            {
                                object t = op.Target;
                                op.Member.SetValue( ref t, value );
                                opSuccess = true;
                            }
                            else
                            {
                                _state.Context.DeferredOperations.Enqueue( op );
                            }
                        }
                        else
                        {
                            var cursor = new SerializationCursor
                            {
                                Tracker = new TrackedObject( null, op.Target, op.Member ),
                                Descriptor = desc,
                                DataNode = op.Data,
                                Phase = CursorPhase.PreProcessing,
                                StepIndex = 0
                            };
                            _state.Stack.Push( cursor );
                            opSuccess = true;
                        }
                    }
                }
                catch( Exception ex )
                {
                    // Exceptions during deferred processing are tricky.
                    // We can't easily "path" them because the stack is empty (we just pushed).
                    // But we know the Operation context.
                    string contextInfo = op.Member != null ? $"Deferred Member {op.Member.Name}" : "Deferred Root";
                    HandleFatalException( ex, contextInfo );
                }

                if( opSuccess ) progressMade = true;
            }

            if( !progressMade && processedCount == count && _state.Stack.Count == 0 && _state.Context.DeferredOperations.Count > 0 )
            {
                string msg = $"Circular Dependency Deadlock: {_state.Context.DeferredOperations.Count} items could not be resolved.";
                _state.Context.Report.Log( LogLevel.Error, msg, _state );
                // We clear to prevent infinite loops, effectively "skipping" the deadlocked items.
                // This is safer than throwing an exception for the whole load.
                _state.Context.DeferredOperations.Clear();
            }
        }

        private void HandleFatalException( Exception ex, string stage )
        {
            // If it's already one of our wrapped exceptions, just rethrow to avoid double-wrapping
            if( ex is UPSSerializationException && !(ex is UPSMissingReferenceException) )
            {
                throw ex;
            }

            // Build the path to the current object
            string path = PathBuilder.BuildPath( _state.Stack );

            string message = $"Serialization Fatal Error during {stage} at '{path}': {ex.Message}";

            // Log to the report container
            _state.Context.Report.Log( LogLevel.Fatal, message );

            // Re-throw wrapped exception for the caller to handle/crash
            if( ex is UPSMissingReferenceException )
            {
                // Preserve specific type for missing refs
                throw new UPSMissingReferenceException( $"{message} (Original: {ex.Message})", ex );
            }

            throw new UPSSerializationException( message, ex );
        }
    }
}
