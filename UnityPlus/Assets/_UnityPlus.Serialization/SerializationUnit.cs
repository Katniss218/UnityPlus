
using System;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// v4 Implementation of the main Serialization API.
    /// Provides simple static helpers for blocking serialization operations.
    /// Uses the StackMachineDriver internally.
    /// </summary>
    public static partial class SerializationUnit
    {
        // --- Serialize ---

        /// <summary>
        /// Serializes an object to a SerializedData tree using the default context.
        /// </summary>
        public static SerializedData Serialize<T>( T obj )
            => Serialize( ObjectContext.Default, obj, null );

        /// <summary>
        /// Serializes an object to a SerializedData tree using a specific context.
        /// </summary>
        public static SerializedData Serialize<T>( int context, T obj )
            => Serialize( context, obj, null );

        /// <summary>
        /// Serializes an object to a SerializedData tree using a specific reference map.
        /// </summary>
        public static SerializedData Serialize<T>( T obj, IReverseReferenceMap s )
            => Serialize( ObjectContext.Default, obj, s );

        /// <summary>
        /// Serializes an object to a SerializedData tree.
        /// </summary>
        public static SerializedData Serialize<T>( int context, T obj, IReverseReferenceMap s )
        {
            var ctx = new SerializationContext( new SerializationConfiguration() )
            {
                ReverseMap = s ?? new BidirectionalReferenceStore(),
                ForwardMap = new ForwardReferenceStore() // Not strictly used for serialize but initialized for consistency
            };

            var driver = new StackMachineDriver( ctx );
            var descriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), context );

            driver.Initialize( obj, descriptor, new SerializationStrategy() );

            // Run synchronously until finished
            while( !driver.IsFinished )
            {
                driver.Tick( float.PositiveInfinity );
            }

            return driver.Result as SerializedData;
        }

        // --- Deserialize ---

        /// <summary>
        /// Deserializes a SerializedData tree to an object of type T using the default context.
        /// </summary>
        public static T Deserialize<T>( SerializedData data )
            => Deserialize<T>( ObjectContext.Default, data, null );

        /// <summary>
        /// Deserializes a SerializedData tree to an object of type T using a specific context.
        /// </summary>
        public static T Deserialize<T>( int context, SerializedData data )
            => Deserialize<T>( context, data, null );

        /// <summary>
        /// Deserializes a SerializedData tree to an object of type T using a specific reference map.
        /// </summary>
        public static T Deserialize<T>( SerializedData data, IForwardReferenceMap l )
            => Deserialize<T>( ObjectContext.Default, data, l );

        /// <summary>
        /// Deserializes a SerializedData tree to an object of type T.
        /// </summary>
        public static T Deserialize<T>( int context, SerializedData data, IForwardReferenceMap l )
        {
            var ctx = new SerializationContext( new SerializationConfiguration() )
            {
                ForwardMap = l ?? new BidirectionalReferenceStore(),
                ReverseMap = new ReverseReferenceStore()
            };

            var driver = new StackMachineDriver( ctx );
            var descriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), context );

            driver.Initialize( null, descriptor, new DeserializationStrategy(), data );

            while( !driver.IsFinished )
            {
                driver.Tick( float.PositiveInfinity );
            }

            return (T)driver.Result;
        }

        // --- Populate (Class) ---

        /// <summary>
        /// Populates an existing object with data from SerializedData.
        /// </summary>
        public static void Populate<T>( T obj, SerializedData data ) where T : class
            => Populate( ObjectContext.Default, obj, data, null );

        public static void Populate<T>( int context, T obj, SerializedData data ) where T : class
            => Populate( context, obj, data, null );

        public static void Populate<T>( T obj, SerializedData data, IForwardReferenceMap l ) where T : class
            => Populate( ObjectContext.Default, obj, data, l );

        public static void Populate<T>( int context, T obj, SerializedData data, IForwardReferenceMap l ) where T : class
        {
            if( obj == null ) throw new ArgumentNullException( nameof( obj ) );

            var ctx = new SerializationContext( new SerializationConfiguration() )
            {
                ForwardMap = l ?? new BidirectionalReferenceStore(),
                ReverseMap = new ReverseReferenceStore()
            };

            var driver = new StackMachineDriver( ctx );
            var descriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), context );

            // Passing 'obj' as root tells DeserializerStrategy to use it (PopulateExisting)
            driver.Initialize( obj, descriptor, new DeserializationStrategy(), data );

            while( !driver.IsFinished )
            {
                driver.Tick( float.PositiveInfinity );
            }
        }

        // --- Populate (Struct) ---

        /// <summary>
        /// Populates an existing struct with data from SerializedData.
        /// </summary>
        public static void Populate<T>( ref T obj, SerializedData data ) where T : struct
            => Populate( ObjectContext.Default, ref obj, data, null );

        public static void Populate<T>( int context, ref T obj, SerializedData data ) where T : struct
            => Populate( context, ref obj, data, null );

        public static void Populate<T>( ref T obj, SerializedData data, IForwardReferenceMap l ) where T : struct
            => Populate( ObjectContext.Default, ref obj, data, l );

        public static void Populate<T>( int context, ref T obj, SerializedData data, IForwardReferenceMap l ) where T : struct
        {
            var ctx = new SerializationContext( new SerializationConfiguration() )
            {
                ForwardMap = l ?? new BidirectionalReferenceStore(),
                ReverseMap = new ReverseReferenceStore()
            };

            var driver = new StackMachineDriver( ctx );
            var descriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), context );

            // Struct is boxed here. The driver will operate on the boxed copy.
            driver.Initialize( obj, descriptor, new DeserializationStrategy(), data );

            while( !driver.IsFinished )
            {
                driver.Tick( float.PositiveInfinity );
            }

            // Unbox result to get the modified struct back
            obj = (T)driver.Result;
        }
    }
}