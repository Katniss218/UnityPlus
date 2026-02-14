using System;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// v4 Implementation of the main Serialization API.
    /// </summary>
    public static partial class SerializationUnit
    {
        // --- Serialize ---

        public static SerializedData Serialize<T>( T obj )
            => Serialize( ObjectContext.Default, obj, null );

        public static SerializedData Serialize<T>( ContextKey context, T obj )
            => Serialize( context, obj, null );

        public static SerializedData Serialize<T>( T obj, IReverseReferenceMap s )
            => Serialize( ObjectContext.Default, obj, s );

        public static SerializedData Serialize<T>( ContextKey context, T obj, IReverseReferenceMap s )
        {
            var ctx = new SerializationContext( new SerializationConfiguration() )
            {
                ReverseMap = s ?? new BidirectionalReferenceStore(),
                ForwardMap = new ForwardReferenceStore()
            };

            var driver = new StackMachineDriver( ctx );
            var descriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), context );

            driver.Initialize( obj, descriptor, new SerializationStrategy() );

            while( !driver.IsFinished )
            {
                driver.Tick( float.PositiveInfinity );
            }

            return driver.Result as SerializedData;
        }

        // --- Deserialize ---

        public static T Deserialize<T>( SerializedData data )
            => Deserialize<T>( ObjectContext.Default, data, null );

        public static T Deserialize<T>( ContextKey context, SerializedData data )
            => Deserialize<T>( context, data, null );

        public static T Deserialize<T>( SerializedData data, IForwardReferenceMap l )
            => Deserialize<T>( ObjectContext.Default, data, l );

        public static T Deserialize<T>( ContextKey context, SerializedData data, IForwardReferenceMap l )
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

        public static void Populate<T>( T obj, SerializedData data ) where T : class
            => Populate( ObjectContext.Default, obj, data, null );

        public static void Populate<T>( ContextKey context, T obj, SerializedData data ) where T : class
            => Populate( context, obj, data, null );

        public static void Populate<T>( T obj, SerializedData data, IForwardReferenceMap l ) where T : class
            => Populate( ObjectContext.Default, obj, data, l );

        public static void Populate<T>( ContextKey context, T obj, SerializedData data, IForwardReferenceMap l ) where T : class
        {
            if( obj == null ) throw new ArgumentNullException( nameof( obj ) );

            var ctx = new SerializationContext( new SerializationConfiguration() )
            {
                ForwardMap = l ?? new BidirectionalReferenceStore(),
                ReverseMap = new ReverseReferenceStore()
            };

            var driver = new StackMachineDriver( ctx );
            var descriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), context );

            driver.Initialize( obj, descriptor, new DeserializationStrategy(), data );

            while( !driver.IsFinished )
            {
                driver.Tick( float.PositiveInfinity );
            }
        }

        // --- Populate (Struct) ---

        public static void Populate<T>( ref T obj, SerializedData data ) where T : struct
            => Populate( ObjectContext.Default, ref obj, data, null );

        public static void Populate<T>( ContextKey context, ref T obj, SerializedData data ) where T : struct
            => Populate( context, ref obj, data, null );

        public static void Populate<T>( ref T obj, SerializedData data, IForwardReferenceMap l ) where T : struct
            => Populate( ObjectContext.Default, ref obj, data, l );

        public static void Populate<T>( ContextKey context, ref T obj, SerializedData data, IForwardReferenceMap l ) where T : struct
        {
            var ctx = new SerializationContext( new SerializationConfiguration() )
            {
                ForwardMap = l ?? new BidirectionalReferenceStore(),
                ReverseMap = new ReverseReferenceStore()
            };

            var driver = new StackMachineDriver( ctx );
            var descriptor = TypeDescriptorRegistry.GetDescriptor( typeof( T ), context );

            driver.Initialize( obj, descriptor, new DeserializationStrategy(), data );

            while( !driver.IsFinished )
            {
                driver.Tick( float.PositiveInfinity );
            }

            obj = (T)driver.Result;
        }
    }
}