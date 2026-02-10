using System;
using System.Threading.Tasks;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Async extensions for SerializationUnitV4 using MainThreadDispatcher for time-sliced execution.
    /// </summary>
    public static partial class SerializationUnit
	{
		// --- Serialize Async ---

		public static Task<SerializedData> SerializeAsync<T>( T obj, float timeBudgetMs = 2f )
			=> SerializeAsync( ObjectContext.Default, obj, null, timeBudgetMs );

		public static Task<SerializedData> SerializeAsync<T>( int context, T obj, float timeBudgetMs = 2f )
			=> SerializeAsync( context, obj, null, timeBudgetMs );

		public static Task<SerializedData> SerializeAsync<T>( T obj, IReverseReferenceMap refs, float timeBudgetMs = 2f )
			=> SerializeAsync( ObjectContext.Default, obj, refs, timeBudgetMs );

		public static Task<SerializedData> SerializeAsync<T>( int context, T obj, IReverseReferenceMap refs, float timeBudgetMs = 2f )
		{
			var ctx = new SerializationContext
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

		public static Task<T> DeserializeAsync<T>( int context, SerializedData data, float timeBudgetMs = 2f )
			=> DeserializeAsync<T>( context, data, null, timeBudgetMs );

		public static Task<T> DeserializeAsync<T>( SerializedData data, IForwardReferenceMap refs, float timeBudgetMs = 2f )
			=> DeserializeAsync<T>( ObjectContext.Default, data, refs, timeBudgetMs );

		public static Task<T> DeserializeAsync<T>( int context, SerializedData data, IForwardReferenceMap refs, float timeBudgetMs = 2f )
		{
			var ctx = new SerializationContext
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

		/// <summary>
		/// Populates the object with data asynchronously. 
		/// Returns the populated object (if T is a struct, the result is the modified boxed copy).
		/// </summary>
		public static Task<T> PopulateAsync<T>( T obj, SerializedData data, float timeBudgetMs = 2f )
			=> PopulateAsync( ObjectContext.Default, obj, data, null, timeBudgetMs );

		public static Task<T> PopulateAsync<T>( int context, T obj, SerializedData data, float timeBudgetMs = 2f )
			=> PopulateAsync( context, obj, data, null, timeBudgetMs );

		public static Task<T> PopulateAsync<T>( T obj, SerializedData data, IForwardReferenceMap refs, float timeBudgetMs = 2f )
			=> PopulateAsync( ObjectContext.Default, obj, data, refs, timeBudgetMs );

		public static Task<T> PopulateAsync<T>( int context, T obj, SerializedData data, IForwardReferenceMap refs, float timeBudgetMs = 2f )
		{
			if( obj == null ) throw new ArgumentNullException( nameof( obj ) );

			var ctx = new SerializationContext
			{
				ForwardMap = refs ?? new BidirectionalReferenceStore(),
				ReverseMap = new ReverseReferenceStore()
			};

			var driver = new StackMachineDriver( ctx );
			var descriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), context );

			// PopulateExisting logic: Target is provided during Initialize
			// Note: If T is struct, 'obj' is a copy. The result Task returns the modified copy.
			driver.Initialize( obj, descriptor, new DeserializationStrategy(), data );

			return RunDriverAsync<T>( driver, timeBudgetMs );
		}

		// --- Driver Helper ---

		private static Task<TReturn> RunDriverAsync<TReturn>( StackMachineDriver driver, float timeBudgetMs )
		{
			var tcs = new TaskCompletionSource<TReturn>();

			// We use a self-scheduling action to tick the driver via MainThreadDispatcher.
			// This distributes the work across frames if the budget is exceeded.
			Action tick = null;
			tick = () =>
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
#warning TODO - consider max total time / cancellation token
						// Schedule next tick
						MainThreadDispatcher.Enqueue( tick );
					}
				}
				catch( Exception ex )
				{
					tcs.SetException( ex );
				}
			};

			MainThreadDispatcher.Enqueue( tick );
			return tcs.Task;
		}
	}
}