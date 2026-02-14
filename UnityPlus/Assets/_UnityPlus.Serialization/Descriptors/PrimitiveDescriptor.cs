using System;
using System.Collections.Generic;
using System.Globalization;

namespace UnityPlus.Serialization
{
    public abstract class PrimitiveDescriptor<T> : IPrimitiveDescriptor
    {
        public Type MappedType => typeof( T );

        public abstract void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx );
        public abstract DeserializationResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result );

        public object CreateInitialTarget( SerializedData data, SerializationContext ctx ) => default( T );
    }

    public class IntDescriptor : PrimitiveDescriptor<int>
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx ) => data = (SerializedPrimitive)(int)target;
        public override DeserializationResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = (int)data;
            return DeserializationResult.Success;
        }
    }

    public class FloatDescriptor : PrimitiveDescriptor<float>
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx ) => data = (SerializedPrimitive)(float)target;
        public override DeserializationResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = (float)data;
            return DeserializationResult.Success;
        }
    }

    public class StringDescriptor : PrimitiveDescriptor<string>
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx ) => data = (SerializedPrimitive)(string)target;
        public override DeserializationResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = (string)data;
            return DeserializationResult.Success;
        }
    }

    public class BoolDescriptor : PrimitiveDescriptor<bool>
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx ) => data = (SerializedPrimitive)(bool)target;
        public override DeserializationResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = (bool)data;
            return DeserializationResult.Success;
        }
    }
}

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Provides built-in descriptors for standard .NET types via the Provider system.
    /// This allows these types to be overridden by user providers if registered in a specific context.
    /// </summary>
    public static class BuiltInDescriptorProviders
    {
        [MapsInheritingFrom( typeof( char ) )]
        private static IDescriptor ProvideChar() => new PrimitiveConfigurableDescriptor<char>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v.ToString(),
            ( d, c ) => { string s = (string)d; return string.IsNullOrEmpty( s ) ? '\0' : s[0]; }
        );

        [MapsInheritingFrom( typeof( string ) )]
        private static IDescriptor ProvideString() => new StringDescriptor();

        [MapsInheritingFrom( typeof( bool ) )]
        private static IDescriptor ProvideBool() => new BoolDescriptor();

        // --- Numeric Types (Explicit) ---

        [MapsInheritingFrom( typeof( byte ) )]
        private static IDescriptor ProvideByte() => new PrimitiveConfigurableDescriptor<byte>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (byte)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( sbyte ) )]
        private static IDescriptor ProvideSByte() => new PrimitiveConfigurableDescriptor<sbyte>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (sbyte)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( short ) )]
        private static IDescriptor ProvideInt16() => new PrimitiveConfigurableDescriptor<short>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (short)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( ushort ) )]
        private static IDescriptor ProvideUInt16() => new PrimitiveConfigurableDescriptor<ushort>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (ushort)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( int ) )]
        private static IDescriptor ProvideInt32() => new IntDescriptor();

        [MapsInheritingFrom( typeof( uint ) )]
        private static IDescriptor ProvideUInt32() => new PrimitiveConfigurableDescriptor<uint>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (uint)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( long ) )]
        private static IDescriptor ProvideInt64() => new PrimitiveConfigurableDescriptor<long>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (long)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( ulong ) )]
        private static IDescriptor ProvideUInt64() => new PrimitiveConfigurableDescriptor<ulong>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (ulong)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( float ) )]
        private static IDescriptor ProvideSingle() => new FloatDescriptor();

        [MapsInheritingFrom( typeof( double ) )]
        private static IDescriptor ProvideDouble() => new PrimitiveConfigurableDescriptor<double>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (double)(SerializedPrimitive)d );

        [MapsInheritingFrom( typeof( decimal ) )]
        private static IDescriptor ProvideDecimal() => new PrimitiveConfigurableDescriptor<decimal>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v, ( d, c ) => (decimal)(SerializedPrimitive)d );

        // --- Extended System Types ---

        [MapsInheritingFrom( typeof( Guid ) )]
        private static IDescriptor ProvideGuid() => new PrimitiveConfigurableDescriptor<Guid>(
            ( v, w, c ) => w.Data = Persistent_Guid.SerializeGuid( v ),
            ( d, c ) => Persistent_Guid.DeserializeGuid( d )
        );

        [MapsInheritingFrom( typeof( DateTime ) )]
        private static IDescriptor ProvideDateTime() => new PrimitiveConfigurableDescriptor<DateTime>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v.ToString( "o", CultureInfo.InvariantCulture ),
            ( d, c ) => DateTime.Parse( (string)d, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind )
        );

        [MapsInheritingFrom( typeof( DateTimeOffset ) )]
        private static IDescriptor ProvideDateTimeOffset() => new PrimitiveConfigurableDescriptor<DateTimeOffset>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v.ToString( "o", CultureInfo.InvariantCulture ),
            ( d, c ) => DateTimeOffset.Parse( (string)d, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind )
        );

        [MapsInheritingFrom( typeof( TimeSpan ) )]
        private static IDescriptor ProvideTimeSpan() => new PrimitiveConfigurableDescriptor<TimeSpan>(
            ( v, w, c ) => w.Data = (SerializedPrimitive)v.ToString( "c", CultureInfo.InvariantCulture ),
            ( d, c ) => TimeSpan.ParseExact( (string)d, "c", CultureInfo.InvariantCulture )
        );

        // --- Value Tuples ---

        [MapsInheritingFrom( typeof( ValueTuple<,> ) )]
        private static IDescriptor ProvideValueTuple2<T1, T2>()
        {
            return new ClassOrStructDescriptor<ValueTuple<T1, T2>>()
                .WithMember( "1", t => t.Item1, ( ref ValueTuple<T1, T2> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", t => t.Item2, ( ref ValueTuple<T1, T2> t, T2 v ) => t.Item2 = v );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,> ) )]
        private static IDescriptor ProvideValueTuple3<T1, T2, T3>()
        {
            return new ClassOrStructDescriptor<ValueTuple<T1, T2, T3>>()
                .WithMember( "1", t => t.Item1, ( ref ValueTuple<T1, T2, T3> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", t => t.Item2, ( ref ValueTuple<T1, T2, T3> t, T2 v ) => t.Item2 = v )
                .WithMember( "3", t => t.Item3, ( ref ValueTuple<T1, T2, T3> t, T3 v ) => t.Item3 = v );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,> ) )]
        private static IDescriptor ProvideValueTuple4<T1, T2, T3, T4>()
        {
            return new ClassOrStructDescriptor<ValueTuple<T1, T2, T3, T4>>()
                .WithMember( "1", t => t.Item1, ( ref ValueTuple<T1, T2, T3, T4> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", t => t.Item2, ( ref ValueTuple<T1, T2, T3, T4> t, T2 v ) => t.Item2 = v )
                .WithMember( "3", t => t.Item3, ( ref ValueTuple<T1, T2, T3, T4> t, T3 v ) => t.Item3 = v )
                .WithMember( "4", t => t.Item4, ( ref ValueTuple<T1, T2, T3, T4> t, T4 v ) => t.Item4 = v );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,,> ) )]
        private static IDescriptor ProvideValueTuple5<T1, T2, T3, T4, T5>()
        {
            return new ClassOrStructDescriptor<ValueTuple<T1, T2, T3, T4, T5>>()
                .WithMember( "1", t => t.Item1, ( ref ValueTuple<T1, T2, T3, T4, T5> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", t => t.Item2, ( ref ValueTuple<T1, T2, T3, T4, T5> t, T2 v ) => t.Item2 = v )
                .WithMember( "3", t => t.Item3, ( ref ValueTuple<T1, T2, T3, T4, T5> t, T3 v ) => t.Item3 = v )
                .WithMember( "4", t => t.Item4, ( ref ValueTuple<T1, T2, T3, T4, T5> t, T4 v ) => t.Item4 = v )
                .WithMember( "5", t => t.Item5, ( ref ValueTuple<T1, T2, T3, T4, T5> t, T5 v ) => t.Item5 = v );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,,,> ) )]
        private static IDescriptor ProvideValueTuple6<T1, T2, T3, T4, T5, T6>()
        {
            return new ClassOrStructDescriptor<ValueTuple<T1, T2, T3, T4, T5, T6>>()
                .WithMember( "1", t => t.Item1, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", t => t.Item2, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T2 v ) => t.Item2 = v )
                .WithMember( "3", t => t.Item3, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T3 v ) => t.Item3 = v )
                .WithMember( "4", t => t.Item4, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T4 v ) => t.Item4 = v )
                .WithMember( "5", t => t.Item5, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T5 v ) => t.Item5 = v )
                .WithMember( "6", t => t.Item6, ( ref ValueTuple<T1, T2, T3, T4, T5, T6> t, T6 v ) => t.Item6 = v );
        }

        [MapsInheritingFrom( typeof( ValueTuple<,,,,,,> ) )]
        private static IDescriptor ProvideValueTuple7<T1, T2, T3, T4, T5, T6, T7>()
        {
            return new ClassOrStructDescriptor<ValueTuple<T1, T2, T3, T4, T5, T6, T7>>()
                .WithMember( "1", t => t.Item1, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T1 v ) => t.Item1 = v )
                .WithMember( "2", t => t.Item2, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T2 v ) => t.Item2 = v )
                .WithMember( "3", t => t.Item3, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T3 v ) => t.Item3 = v )
                .WithMember( "4", t => t.Item4, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T4 v ) => t.Item4 = v )
                .WithMember( "5", t => t.Item5, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T5 v ) => t.Item5 = v )
                .WithMember( "6", t => t.Item6, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T6 v ) => t.Item6 = v )
                .WithMember( "7", t => t.Item7, ( ref ValueTuple<T1, T2, T3, T4, T5, T6, T7> t, T7 v ) => t.Item7 = v );
        }


        [MapsInheritingFrom( typeof( Array ) )]
        private static IDescriptor ProvideArray<T>( ContextKey context )
        {
            var desc = new ArrayDescriptor<T>();
            var args = ContextRegistry.GetContextArguments( context );
            desc.ElementContext = args.Length > 0 ? args[0] : ContextIDs.Default;
            return desc;
        }

        [MapsInheritingFrom( typeof( List<> ) )]
        private static IDescriptor ProvideList<T>( ContextKey context )
        {
            var desc = new ListDescriptor<T>();
            var args = ContextRegistry.GetContextArguments( context );
            desc.ElementContext = args.Length > 0 ? args[0] : ContextIDs.Default;
            return desc;
        }

        [MapsInheritingFrom( typeof( Dictionary<,> ) )]
        private static IDescriptor ProvideDictionary<TKey, TValue>( ContextKey context )
        {
            var desc = new DictionaryDescriptor<Dictionary<TKey, TValue>, TKey, TValue>();
            var args = ContextRegistry.GetContextArguments( context );
            if( args.Length >= 2 )
            {
                desc.KeyContext = args[0];
                desc.ValueContext = args[1];
            }

            return desc;
        }

        [MapsInheritingFrom( typeof( Enum ) )]
        private static IDescriptor ProvideEnum<T>() where T : struct, Enum
        {
            return new EnumDescriptor<T>();
        }

        [MapsAnyInterface( ContextType = typeof( Ctx.Ref ) )]
        [MapsAnyClass( ContextType = typeof( Ctx.Ref ) )]
        private static IDescriptor ProvideReference<T>() where T : class
        {
            return new ReferenceDescriptor<T>();
        }
    }
}