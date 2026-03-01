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
        public override Type MappedType => typeof( Delegate );

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

        public override IMemberInfo GetMemberInfo( int stepIndex )
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
            // We use a custom descriptor for the Entry struct, cached statically to avoid allocation
            private static readonly IDescriptor _entryDescriptor = new DelegateEntryDescriptor();

            public readonly string Name => null;
            public readonly int Index => _index;
            public readonly Type MemberType => typeof( DelegateEntry );
            public readonly IDescriptor TypeDescriptor => _entryDescriptor;
            public readonly bool RequiresWriteBack => true;

            public DelegateEntryMemberInfo( int index )
            {
                _index = index;
            }

            public ContextKey GetContext( object target ) => default;

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
#warning TODO - pretty sure this can be handled with a normal immutable memberwise descriptor.
        private class DelegateEntryDescriptor : CompositeDescriptor
        {
            private static readonly IDescriptor _methodDescriptor = new MethodInfoDescriptor();
            private static readonly IDescriptor _typeDescriptor = new PrimitiveConfigurableDescriptor<Type>(
                ( v, w, c ) => w.Data = v.SerializeType(),
                ( d, c ) => d.DeserializeType() );

            public override Type MappedType => typeof( DelegateEntry );

            // 3 members: Target, Method, Type
            public override int GetStepCount( object target ) => 3;

            public override IMemberInfo GetMemberInfo( int stepIndex )
            {
                switch( stepIndex )
                {
                    case 0: return new TargetMemberInfo();
                    case 1: return new MethodMemberInfo( _methodDescriptor );
                    case 2: return new DelegateTypeMemberInfo( _typeDescriptor );
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
                public readonly string Name => "target";
                public readonly int Index => -1;
                public readonly Type MemberType => typeof( object );
                // Use Ref context for the target!
                public readonly IDescriptor TypeDescriptor => TypeDescriptorRegistry.GetDescriptor( typeof( object ), ContextRegistry.GetID( typeof( Ctx.Ref ) ) );
                public readonly bool RequiresWriteBack => false;

                public ContextKey GetContext( object target ) => ContextRegistry.GetID( typeof( Ctx.Ref ) );

                public object GetValue( object target ) => ((DelegateEntry)target).Target;
                public void SetValue( ref object target, object value )
                {
                    var entry = (DelegateEntry)target;
                    entry.Target = value;
                    target = entry;
                }
            }

            readonly struct MethodMemberInfo : IMemberInfo
            {
                public readonly string Name => "method";
                public readonly int Index => -1;
                public readonly Type MemberType => typeof( MethodInfo );
                public IDescriptor TypeDescriptor { get; }
                public readonly bool RequiresWriteBack => false;

                public MethodMemberInfo( IDescriptor desc ) { TypeDescriptor = desc; }

                public ContextKey GetContext( object target ) => default;

                public object GetValue( object target ) => ((DelegateEntry)target).Method;
                public void SetValue( ref object target, object value )
                {
                    var entry = (DelegateEntry)target;
                    entry.Method = (MethodInfo)value;
                    target = entry;
                }
            }

            readonly struct DelegateTypeMemberInfo : IMemberInfo
            {
                public readonly string Name => "type";
                public readonly int Index => -1;
                public readonly Type MemberType => typeof( Type );
                public IDescriptor TypeDescriptor { get; }
                public readonly bool RequiresWriteBack => false;

                public DelegateTypeMemberInfo( IDescriptor desc ) { TypeDescriptor = desc; }

                public ContextKey GetContext( object target ) => default;

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

#warning TODO - pretty sure this can be handled with a normal immutable memberwise descriptor.
        private class MethodInfoDescriptor : CompositeDescriptor
        {
            // Cache common descriptors
            private static readonly IDescriptor _sysTypeDescriptor = new PrimitiveConfigurableDescriptor<Type>(
                ( v, w, c ) => w.Data = v.SerializeType(),
                ( d, c ) => d.DeserializeType() );
            private static readonly IDescriptor _stringDescriptor = new StringDescriptor();
            private static readonly IDescriptor _typeArrayDescriptor = new ArrayDescriptor<Type>();

            public override Type MappedType => typeof( MethodInfo );
            public override int GetStepCount( object target ) => 3;
            public override int GetConstructionStepCount( object target ) => 3; // Immutable construction

            public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
            {
                return new object[3];
            }

            public override IMemberInfo GetMemberInfo( int stepIndex )
            {
                if( stepIndex == 0 ) return new DeclaringTypeMember( _sysTypeDescriptor );
                if( stepIndex == 1 ) return new NameMember( _stringDescriptor );
                if( stepIndex == 2 ) return new ParametersMember( _typeArrayDescriptor );
                return null;
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

            private readonly struct DeclaringTypeMember : IMemberInfo
            {
                public string Name => "declaringType";
                public int Index => -1;
                public Type MemberType => typeof( Type );
                public IDescriptor TypeDescriptor { get; }
                public bool RequiresWriteBack => false;

                public DeclaringTypeMember( IDescriptor desc ) { TypeDescriptor = desc; }

                public ContextKey GetContext( object target ) => default;

                public object GetValue( object target )
                {
                    if( target is MethodInfo mi ) return mi.DeclaringType;
                    return ((object[])target)[0];
                }

                public void SetValue( ref object target, object value ) => ((object[])target)[0] = value;
            }

            private readonly struct NameMember : IMemberInfo
            {
                public string Name => "name";
                public int Index => -1;
                public Type MemberType => typeof( string );
                public IDescriptor TypeDescriptor { get; }
                public bool RequiresWriteBack => false;

                public NameMember( IDescriptor desc ) { TypeDescriptor = desc; }

                public ContextKey GetContext( object target ) => default;

                public object GetValue( object target )
                {
                    if( target is MethodInfo mi ) return mi.Name;
                    return ((object[])target)[1];
                }

                public void SetValue( ref object target, object value ) => ((object[])target)[1] = value;
            }

            private readonly struct ParametersMember : IMemberInfo
            {
                public string Name => "parameters";
                public int Index => -1;
                public Type MemberType => typeof( Type[] );
                public IDescriptor TypeDescriptor { get; }
                public bool RequiresWriteBack => false;

                public ParametersMember( IDescriptor desc ) { TypeDescriptor = desc; }

                public ContextKey GetContext( object target ) => default;

                public object GetValue( object target )
                {
                    if( target is MethodInfo mi ) return mi.GetParameters().Select( p => p.ParameterType ).ToArray();
                    return ((object[])target)[2];
                }

                public void SetValue( ref object target, object value ) => ((object[])target)[2] = value;
            }
        }
    }
}