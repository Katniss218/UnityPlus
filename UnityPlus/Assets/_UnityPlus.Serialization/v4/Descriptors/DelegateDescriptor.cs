
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Handles serialization of Delegates (Action, Func, Events).
    /// Stores the invocation list as an array of entries.
    /// Uses dynamic construction steps to resolve Target objects via reference.
    /// </summary>
    public class DelegateDescriptor : CompositeDescriptor
    {
        public override Type WrappedType => typeof( Delegate );

        public override int GetConstructionStepCount( object target )
        {
            // Deserialize: Target is buffer (object[] of length N) -> N steps.
            if( target is object[] buffer )
                return buffer.Length;

            // Serialize: Target is Delegate -> InvocationList.Length steps.
            if( target is Delegate del )
                return del.GetInvocationList().Length;

            return 0;
        }

        public override int GetStepCount( object target )
        {
            // All steps are construction steps because Delegates are immutable.
            return GetConstructionStepCount( target );
        }

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            // If data is an array, we need a buffer of that size.
            if( data is SerializedArray arr )
            {
                return new object[arr.Count];
            }
            return new object[0];
        }

        public override IMemberInfo GetMemberInfo( int stepIndex, object target )
        {
            // For both Serialize and Deserialize, we need a MemberInfo that points to the Target Object of the invocation.
            return new DelegateEntryMemberInfo( stepIndex );
        }

        public override object Construct( object initialTarget )
        {
            var buffer = (object[])initialTarget;
            var delegates = new List<Delegate>();

            foreach( var item in buffer )
            {
                if( item is DelegateEntry entry && entry.Method != null && entry.DelegateType != null )
                {
                    try
                    {
                        Delegate d = null;
                        if( entry.Method.IsStatic )
                        {
                            d = Delegate.CreateDelegate( entry.DelegateType, entry.Method );
                        }
                        else if( entry.Target != null )
                        {
                            d = Delegate.CreateDelegate( entry.DelegateType, entry.Target, entry.Method );
                        }

                        if( d != null ) delegates.Add( d );
                    }
                    catch( Exception ex )
                    {
                        UnityEngine.Debug.LogWarning( $"Failed to deserialize delegate: {ex.Message}" );
                    }
                }
            }

            if( delegates.Count == 0 ) return null;
            if( delegates.Count == 1 ) return delegates[0];
            return Delegate.Combine( delegates.ToArray() );
        }

        // --- Inner Types ---

        public struct DelegateEntry
        {
            public object Target;
            public MethodInfo Method;
            public Type DelegateType;
        }

        private struct DelegateEntryMemberInfo : IMemberInfo
        {
            private int _index;
            // We use a custom descriptor for the Entry struct
            private static ITypeDescriptor _entryDescriptor = new DelegateEntryDescriptor();

            public string Name => null;
            public Type MemberType => typeof( DelegateEntry );
            public ITypeDescriptor TypeDescriptor => _entryDescriptor;
            public bool IsValueType => true;

            public DelegateEntryMemberInfo( int index )
            {
                _index = index;
            }

            public object GetValue( object target )
            {
                if( target is Delegate del )
                {
                    var d = del.GetInvocationList()[_index];
                    return new DelegateEntry { Target = d.Target, Method = d.Method, DelegateType = del.GetType() };
                }
                return ((object[])target)[_index]; // Return boxed struct from buffer
            }

            public void SetValue( ref object target, object value )
            {
                if( target is object[] buffer )
                {
                    buffer[_index] = value;
                }
            }
        }

        // Serializes { target: $ref, method: { ... }, type: "..." }
        private class DelegateEntryDescriptor : CompositeDescriptor
        {
            public override Type WrappedType => typeof( DelegateEntry );

            // 3 members: Target, Method, Type
            public override int GetStepCount( object target ) => 3;

            public override IMemberInfo GetMemberInfo( int stepIndex, object target )
            {
                switch( stepIndex )
                {
                    case 0: return new TargetMemberInfo();
                    case 1: return new MethodMemberInfo();
                    case 2: return new DelegateTypeMemberInfo();
                }
                return null;
            }

            public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
            {
                return new DelegateEntry();
            }

            // --- Member Infos for Entry ---

            struct TargetMemberInfo : IMemberInfo
            {
                public string Name => "target";
                public Type MemberType => typeof( object );
                // Use Ref context for the target!
                public ITypeDescriptor TypeDescriptor => TypeDescriptorRegistry.GetDescriptor( typeof( object ), ObjectContext.Ref );
                public bool IsValueType => false;

                public object GetValue( object target ) => ((DelegateEntry)target).Target;
                public void SetValue( ref object target, object value )
                {
                    var entry = (DelegateEntry)target;
                    entry.Target = value;
                    target = entry;
                }
            }

            struct MethodMemberInfo : IMemberInfo
            {
                public string Name => "method";
                public Type MemberType => typeof( MethodInfo );
                // Custom descriptor for MethodInfo serialization
                public ITypeDescriptor TypeDescriptor => new MethodInfoDescriptor();
                public bool IsValueType => false;

                public object GetValue( object target ) => ((DelegateEntry)target).Method;
                public void SetValue( ref object target, object value )
                {
                    var entry = (DelegateEntry)target;
                    entry.Method = (MethodInfo)value;
                    target = entry;
                }
            }

            struct DelegateTypeMemberInfo : IMemberInfo
            {
                public string Name => "type";
                public Type MemberType => typeof( Type );
                public ITypeDescriptor TypeDescriptor => new SystemTypeDescriptor();
                public bool IsValueType => false;

                public object GetValue( object target ) => ((DelegateEntry)target).DelegateType;
                public void SetValue( ref object target, object value )
                {
                    var entry = (DelegateEntry)target;
                    entry.DelegateType = (Type)value;
                    target = entry;
                }
            }
        }

        // Serializes MethodInfo as { declaringType: "...", name: "...", parameters: [...] }
        private class MethodInfoDescriptor : CompositeDescriptor
        {
            public override Type WrappedType => typeof( MethodInfo );
            public override int GetStepCount( object target ) => 3;
            public override int GetConstructionStepCount( object target ) => 3; // Immutable construction

            public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
            {
                return new object[3];
            }

            public override IMemberInfo GetMemberInfo( int stepIndex, object target )
            {
                // Serialize: target is MethodInfo
                if( target is MethodInfo mi )
                {
                    if( stepIndex == 0 ) return new FixedValueMemberInfo( 0, "declaringType", typeof( Type ), new SystemTypeDescriptor(), mi.DeclaringType );
                    if( stepIndex == 1 ) return new FixedValueMemberInfo( 1, "name", typeof( string ), new StringDescriptor(), mi.Name );
                    if( stepIndex == 2 ) return new FixedValueMemberInfo( 2, "parameters", typeof( Type[] ), new ArrayDescriptor<Type>(), mi.GetParameters().Select( p => p.ParameterType ).ToArray() );
                }
                // Deserialize: target is object[] buffer
                return new BufferMemberInfo( stepIndex,
                    stepIndex == 0 ? "declaringType" : stepIndex == 1 ? "name" : "parameters",
                    stepIndex == 0 ? typeof( Type ) : stepIndex == 1 ? typeof( string ) : typeof( Type[] ),
                    stepIndex == 0 ? (ITypeDescriptor)new SystemTypeDescriptor() : stepIndex == 1 ? (ITypeDescriptor)new StringDescriptor() : new ArrayDescriptor<Type>() );
            }

            public override object Construct( object initialTarget )
            {
                var buffer = (object[])initialTarget;
                Type declaringType = (Type)buffer[0];
                string name = (string)buffer[1];
                Type[] parameters = (Type[])buffer[2];

                if( declaringType == null || name == null ) return null;

                try
                {
                    var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                    return declaringType.GetMethod( name, flags, null, parameters ?? Type.EmptyTypes, null );
                }
                catch { return null; }
            }
        }

        private struct FixedValueMemberInfo : IMemberInfo
        {
            public string Name { get; }
            public Type MemberType { get; }
            public ITypeDescriptor TypeDescriptor { get; }
            public bool IsValueType => false;

            private object _value;

            public FixedValueMemberInfo( int index, string name, Type type, ITypeDescriptor desc, object val )
            {
                Name = name;
                MemberType = type;
                TypeDescriptor = desc;
                _value = val;
            }

            public object GetValue( object target ) => _value;
            public void SetValue( ref object target, object value ) { }
        }

        private struct BufferMemberInfo : IMemberInfo
        {
            public string Name { get; }
            public Type MemberType { get; }
            public ITypeDescriptor TypeDescriptor { get; }
            public bool IsValueType => false;

            private int _index;

            public BufferMemberInfo( int index, string name, Type type, ITypeDescriptor desc )
            {
                _index = index;
                Name = name;
                MemberType = type;
                TypeDescriptor = desc;
            }

            public object GetValue( object target ) => ((object[])target)[_index];
            public void SetValue( ref object target, object value ) => ((object[])target)[_index] = value;
        }

        // Helper for Type serialization to avoid reliance on registry having it
        private class SystemTypeDescriptor : PrimitiveDescriptor<Type>
        {
            public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
            {
                if( target == null ) data = null;
                else data = (SerializedPrimitive)((Type)target).AssemblyQualifiedName;
            }

            public override DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
            {
                result = null;
                if( data is SerializedPrimitive prim && prim._type == SerializedPrimitive.DataType.String )
                {
                    result = ctx.TypeResolver.ResolveType( (string)prim );
                    return DeserializeResult.Success;
                }
                return DeserializeResult.Success; // Null/Missing is success (null type)
            }
        }
    }
}
