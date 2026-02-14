using System;
using System.Threading.Tasks;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization
{
    public static partial class SerializationUnit
    {
        // --- Serialize Async ---

        public static Task<SerializedData> SerializeAsync<T>( T obj, float timeBudgetMs = 2f )
            => SerializeAsync( ObjectContext.Default, obj, null, timeBudgetMs );

        public static Task<SerializedData> SerializeAsync<T>( ContextKey context, T obj, float timeBudgetMs = 2f )
            => SerializeAsync( context, obj, null, timeBudgetMs );

        public static Task<SerializedData> SerializeAsync<T>( T obj, IReverseReferenceMap refs, float timeBudgetMs = 2f )
            => SerializeAsync( ObjectContext.Default, obj, refs, timeBudgetMs );

        public static Task<SerializedData> SerializeAsync<T>( ContextKey context, T obj, IReverseReferenceMap refs, float timeBudgetMs = 2f )
        {
            var ctx = new SerializationContext( new SerializationConfiguration() )
            {
                ReverseMap = refs ?? new BidirectionalReferenceStore(),
                ForwardMap = new ForwardReferenceStore()
            };

            var driver = new StackMachineDriver( ctx );
            var descriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), context );

            driver.Initialize( obj, descriptor, new SerializationStrategy() );

            return RunDriverAsync<SerializedData>( driver, timeBudgetMs );
        }

        // --- Deserialize Async ---

        public static Task<T> DeserializeAsync<T>( SerializedData data, float timeBudgetMs = 2f )
            => DeserializeAsync<T>( ObjectContext.Default, data, null, timeBudgetMs );

        public static Task<T> DeserializeAsync<T>( ContextKey context, SerializedData data, float timeBudgetMs = 2f )
            => DeserializeAsync<T>( context, data, null, timeBudgetMs );

        public static Task<T> DeserializeAsync<T>( SerializedData data, IForwardReferenceMap refs, float timeBudgetMs = 2f )
            => DeserializeAsync<T>( ObjectContext.Default, data, refs, timeBudgetMs );

        public static Task<T> DeserializeAsync<T>( ContextKey context, SerializedData data, IForwardReferenceMap refs, float timeBudgetMs = 2f )
        {
            var ctx = new SerializationContext( new SerializationConfiguration() )
            {
                ForwardMap = refs ?? new BidirectionalReferenceStore(),
                ReverseMap = new ReverseReferenceStore()
            };

            var driver = new StackMachineDriver( ctx );
            var descriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), context );

            driver.Initialize( null, descriptor, new DeserializationStrategy(), data );

            return RunDriverAsync<T>( driver, timeBudgetMs );
        }

        // --- Populate Async ---

        public static Task<T> PopulateAsync<T>( T obj, SerializedData data, float timeBudgetMs = 2f )
            => PopulateAsync( ObjectContext.Default, obj, data, null, timeBudgetMs );

        public static Task<T> PopulateAsync<T>( ContextKey context, T obj, SerializedData data, float timeBudgetMs = 2f )
            => PopulateAsync( context, obj, data, null, timeBudgetMs );

        public static Task<T> PopulateAsync<T>( T obj, SerializedData data, IForwardReferenceMap refs, float timeBudgetMs = 2f )
            => PopulateAsync( ObjectContext.Default, obj, data, refs, timeBudgetMs );

        public static Task<T> PopulateAsync<T>( ContextKey context, T obj, SerializedData data, IForwardReferenceMap refs, float timeBudgetMs = 2f )
        {
            if( obj == null ) throw new ArgumentNullException( nameof( obj ) );

            var ctx = new SerializationContext( new SerializationConfiguration() )
            {
                ForwardMap = refs ?? new BidirectionalReferenceStore(),
                ReverseMap = new ReverseReferenceStore()
            };

            var driver = new StackMachineDriver( ctx );
            var descriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), context );

            driver.Initialize( obj, descriptor, new DeserializationStrategy(), data );

            return RunDriverAsync<T>( driver, timeBudgetMs );
        }

        // --- Driver Helper ---

        private class AsyncDriverRunner<TReturn>
        {
            public TaskCompletionSource<TReturn> tcs;
            public StackMachineDriver driver;
            public float timeBudgetMs;
            public Action tickAction;

            public void Tick()
            {
                try
                {
                    driver.Tick( timeBudgetMs );

                    if( driver.IsFinished )
                    {
                        tcs.SetResult( (TReturn)driver.Result );
                    }
                    else
                    {
                        MainThreadDispatcher.Enqueue( tickAction );
                    }
                }
                catch( Exception ex )
                {
                    tcs.SetException( ex );
                }
            }
        }

        private static Task<TReturn> RunDriverAsync<TReturn>( StackMachineDriver driver, float timeBudgetMs )
        {
            var runner = new AsyncDriverRunner<TReturn>
            {
                tcs = new TaskCompletionSource<TReturn>(),
                driver = driver,
                timeBudgetMs = timeBudgetMs
            };

            runner.tickAction = runner.Tick;

            MainThreadDispatcher.Enqueue( runner.tickAction );
            return runner.tcs.Task;
        }
    }
}