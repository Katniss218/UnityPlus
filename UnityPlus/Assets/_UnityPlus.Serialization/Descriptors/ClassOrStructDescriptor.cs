
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// A concrete descriptor for a class or struct, composed of named members.
    /// </summary>
    /// <typeparam name="T">The type being described.</typeparam>
    public class ClassOrStructDescriptor<T> : CompositeDescriptor
    {
        public readonly struct MemberModifier
        {
            private readonly ClassOrStructDescriptor<T> _descriptor;
            private readonly int _index;

            public MemberModifier( ClassOrStructDescriptor<T> descriptor, int index )
            {
                _descriptor = descriptor;
                _index = index;
            }

            /// <summary>
            /// Applies a condition to the selected member. The member will only be serialized/deserialized if the condition returns true.
            /// </summary>
            public ClassOrStructDescriptor<T> When( Predicate<T> condition )
            {
                if( _index == -1 ) return _descriptor;
                var member = _descriptor._members[_index];

                // Chain predicates (AND logic)
                var old = member.ShouldSerialize;
                member.ShouldSerialize = old == null ? (o => condition( (T)o )) : (o => old( o ) && condition( (T)o ));

                return _descriptor;
            }

            /// <summary>
            /// Applies a context-aware condition to the selected member.
            /// </summary>
            public ClassOrStructDescriptor<T> When( Func<T, SerializationContext, bool> condition )
            {
                if( _index == -1 ) return _descriptor;
                var member = _descriptor._members[_index];

                var old = member.ShouldSerializeWithContext;
                member.ShouldSerializeWithContext = old == null ? (( o, c ) => condition( (T)o, c )) : (( o, c ) => old( o, c ) && condition( (T)o, c ));

                return _descriptor;
            }

            /// <summary>
            /// Removes the selected member from the serialization descriptor.
            /// </summary>
            public ClassOrStructDescriptor<T> Delete()
            {
                if( _index != -1 )
                {
                    _descriptor._members.RemoveAt( _index );
                }
                return _descriptor;
            }
        }

        public override Type MappedType => typeof( T );

        private readonly List<MemberDefinition<object>> _members = new List<MemberDefinition<object>>();
        private readonly List<IMethodInfo> _methods = new List<IMethodInfo>();

        // Factories
        private Func<object> _simpleFactory;
        private Func<SerializedData, SerializationContext, object> _rawFactory;
        private Delegate _constructor;
        private (string name, Type type)[] _constructorParams;

        // Lifecycle
        private Action<T, SerializationContext> _onSerializing;
        private Action<T, SerializationContext> _onDeserialized;

        // --- Fluent API: Member Modification ---

        /// <summary>
        /// Selects an existing member by name for modification (applying conditions, removing, etc).
        /// </summary>
        public MemberModifier Member( string name )
        {
            int index = _members.FindIndex( m => m.Name == name );
            if( index == -1 )
            {
                UnityEngine.Debug.LogError( $"Member '{name}' not found on type '{typeof( T ).Name}'." );
                return new MemberModifier( this, -1 );
            }
            return new MemberModifier( this, index );
        }

        // --- Fluent API: Conditionals (Last Member Shortcut) ---

        /// <summary>
        /// Applies a condition to the LAST added member.
        /// </summary>
        public ClassOrStructDescriptor<T> When( Predicate<T> condition )
        {
            if( _members.Count == 0 ) throw new InvalidOperationException( "No members defined." );
            return new MemberModifier( this, _members.Count - 1 ).When( condition );
        }

        public ClassOrStructDescriptor<T> When( Func<T, SerializationContext, bool> condition )
        {
            if( _members.Count == 0 ) throw new InvalidOperationException( "No members defined." );
            return new MemberModifier( this, _members.Count - 1 ).When( condition );
        }

        // --- Fluent API: Members (Expression Based) ---

        public ClassOrStructDescriptor<T> WithMember<TMember>( string name, Expression<Func<T, TMember>> accessor )
        {
            return WithMember( name, ObjectContext.Default, accessor );
        }

        public ClassOrStructDescriptor<T> WithMember<TMember>( string name, Type contextType, Expression<Func<T, TMember>> accessor )
        {
            var contextId = ContextRegistry.GetID( contextType );
            return WithMember( name, contextId, accessor );
        }

        public ClassOrStructDescriptor<T> WithMember<TMember>( string name, ContextKey context, Expression<Func<T, TMember>> accessor )
        {
            var getter = AccessorUtils.CreateGetter( accessor );
            Setter<T, TMember> setter = null;
            RefSetter<T, TMember> refSetter = null;
            MemberInfo nativeMember = null;

            if( accessor.Body is MemberExpression me )
            {
                nativeMember = me.Member;
            }

            if( typeof( T ).IsValueType )
                refSetter = AccessorUtils.CreateStructSetter( accessor );
            else
                setter = AccessorUtils.CreateSetter( accessor );

            return RegisterMember( name, context, getter, setter, refSetter, nativeMember );
        }

        // --- Fluent API: Members (Delegate/v3 Compatibility) ---

        public ClassOrStructDescriptor<T> WithMember<TMember>( string name, Getter<T, TMember> getter, Setter<T, TMember> setter )
        {
            return WithMember( name, ObjectContext.Default, getter, setter );
        }

        public ClassOrStructDescriptor<T> WithMember<TMember>( string name, Type contextType, Getter<T, TMember> getter, Setter<T, TMember> setter )
        {
            return WithMember( name, ContextRegistry.GetID( contextType ), getter, setter );
        }

        public ClassOrStructDescriptor<T> WithMember<TMember>( string name, ContextKey context, Getter<T, TMember> getter, Setter<T, TMember> setter )
        {
            if( typeof( T ).IsValueType )
                throw new InvalidOperationException( $"Cannot use Action<T, Member> setter for struct type {typeof( T )}. Use expressions or RefSetter." );

            return RegisterMember( name, context, getter, setter, null, null );
        }

        public ClassOrStructDescriptor<T> WithMember<TMember>( string name, Getter<T, TMember> getter, RefSetter<T, TMember> refSetter )
        {
            return WithMember( name, ObjectContext.Default, getter, refSetter );
        }

        public ClassOrStructDescriptor<T> WithMember<TMember>( string name, Type contextType, Getter<T, TMember> getter, RefSetter<T, TMember> refSetter )
        {
            return WithMember( name, ContextRegistry.GetID( contextType ), getter, refSetter );
        }

        public ClassOrStructDescriptor<T> WithMember<TMember>( string name, ContextKey context, Getter<T, TMember> getter, RefSetter<T, TMember> refSetter )
        {
            return RegisterMember( name, context, getter, null, refSetter, null );
        }

        // --- Internal Registration Helper ---

        private ClassOrStructDescriptor<T> RegisterMember<TMember>( string name, ContextKey context, Getter<T, TMember> getter, Setter<T, TMember> setter, RefSetter<T, TMember> refSetter, MemberInfo nativeMember )
        {
            _members.Add( new MemberDefinition<object>(
                name,
                context,
                t => (object)getter( (T)t ),
                setter != null ? ( t, v ) => setter( (T)t, (TMember)v ) : (Action<object, object>)null,
                refSetter != null ? ( ref object t, object v ) =>
                {
                    T typed = (T)t;
                    refSetter( ref typed, (TMember)v );
                    t = typed;
                }
            : (RefSetter<object, object>)null,
                nativeMember,
                typeof( TMember )
            ) );
            return this;
        }

        public ClassOrStructDescriptor<T> WithReadonlyMember<TMember>( string name, Func<T, TMember> getter )
        {
            return WithReadonlyMember( name, ObjectContext.Default, getter );
        }

        public ClassOrStructDescriptor<T> WithReadonlyMember<TMember>( string name, int context, Func<T, TMember> getter )
        {
            _members.Add( new MemberDefinition<object>(
                name,
                context,
                t => (object)getter( (T)t ),
                null,
                null,
                null,
                typeof( TMember )
            ) );
            return this;
        }

        // --- Fluent API: Construction & Factories ---

        /// <summary>
        /// Defines a factory that creates the object using parameters loaded from serialized data.
        /// </summary>
        /// <param name="constructor">The delegate to create the object.</param>
        /// <param name="parameters">The list of (name, type) pairs for the parameters, matching the order of the delegate.</param>
        public ClassOrStructDescriptor<T> WithConstructor( Func<object[], T> constructor, params (string name, Type type)[] parameters )
        {
            _constructor = constructor;
            _constructorParams = parameters;
            return this;
        }

        /// <summary>
        /// Defines a simple parameterless factory.
        /// </summary>
        public ClassOrStructDescriptor<T> WithFactory( Func<object> factory )
        {
            _simpleFactory = factory;
            return this;
        }

        /// <summary>
        /// Defines a factory that inspects the raw serialized data before creating the object.
        /// Useful for ScriptableObject/Prefab instantiation.
        /// </summary>
        public ClassOrStructDescriptor<T> WithRawFactory( Func<SerializedData, SerializationContext, T> factory )
        {
            _rawFactory = ( d, c ) => factory( d, c );
            return this;
        }

        // --- Strongly Typed Factory Overloads ---

        public ClassOrStructDescriptor<T> WithFactory<P1>( Func<P1, T> factory, string n1 )
        {
            return WithConstructor(
                args => factory( (P1)args[0] ),
                (n1, typeof( P1 ))
            );
        }

        public ClassOrStructDescriptor<T> WithFactory<P1, P2>( Func<P1, P2, T> factory, string n1, string n2 )
        {
            return WithConstructor(
                args => factory( (P1)args[0], (P2)args[1] ),
                (n1, typeof( P1 )),
                (n2, typeof( P2 ))
            );
        }

        public ClassOrStructDescriptor<T> WithFactory<P1, P2, P3>( Func<P1, P2, P3, T> factory, string n1, string n2, string n3 )
        {
            return WithConstructor(
                args => factory( (P1)args[0], (P2)args[1], (P3)args[2] ),
                (n1, typeof( P1 )),
                (n2, typeof( P2 )),
                (n3, typeof( P3 ))
            );
        }

        public ClassOrStructDescriptor<T> WithFactory<P1, P2, P3, P4>( Func<P1, P2, P3, P4, T> factory, string n1, string n2, string n3, string n4 )
        {
            return WithConstructor(
                args => factory( (P1)args[0], (P2)args[1], (P3)args[2], (P4)args[3] ),
                (n1, typeof( P1 )),
                (n2, typeof( P2 )),
                (n3, typeof( P3 )),
                (n4, typeof( P4 ))
            );
        }

        public ClassOrStructDescriptor<T> WithFactory<P1, P2, P3, P4, P5>( Func<P1, P2, P3, P4, P5, T> factory, string n1, string n2, string n3, string n4, string n5 )
        {
            return WithConstructor(
                args => factory( (P1)args[0], (P2)args[1], (P3)args[2], (P4)args[3], (P5)args[4] ),
                (n1, typeof( P1 )),
                (n2, typeof( P2 )),
                (n3, typeof( P3 )),
                (n4, typeof( P4 )),
                (n5, typeof( P5 ))
            );
        }

        public ClassOrStructDescriptor<T> WithFactory<P1, P2, P3, P4, P5, P6>( Func<P1, P2, P3, P4, P5, P6, T> factory, string n1, string n2, string n3, string n4, string n5, string n6 )
        {
            return WithConstructor(
                args => factory( (P1)args[0], (P2)args[1], (P3)args[2], (P4)args[3], (P5)args[4], (P6)args[5] ),
                (n1, typeof( P1 )),
                (n2, typeof( P2 )),
                (n3, typeof( P3 )),
                (n4, typeof( P4 )),
                (n5, typeof( P5 )),
                (n6, typeof( P6 ))
            );
        }

        // --- Method / UI Support ---

        public ClassOrStructDescriptor<T> WithMethod( string name, Action<T> action, string displayName = null )
        {
            _methods.Add( new ActionMethodInfo( name, displayName, action ) );
            return this;
        }

        // --- Fluent API: Lifecycle ---

        public ClassOrStructDescriptor<T> OnSerializing( Action<T, SerializationContext> callback )
        {
            _onSerializing += callback;
            return this;
        }

        public ClassOrStructDescriptor<T> OnDeserialized( Action<T, SerializationContext> callback )
        {
            _onDeserialized += callback;
            return this;
        }

        // --- ICompositeTypeDescriptor Implementation ---

        public override int GetConstructionStepCount( object target )
        {
            return _constructorParams?.Length ?? 0;
        }

        public override int GetStepCount( object target )
        {
            return GetConstructionStepCount( target ) + _members.Count;
        }

        public override IMemberInfo GetMemberInfo( int stepIndex, object target )
        {
            int ctorCount = GetConstructionStepCount( target );

            if( stepIndex < ctorCount )
            {
                // Optimization: If serializing an existing instance, we skip constructor args 
                // IF we assume members cover the data.
                if( target is T )
                {
                    return null; // Skip during serialization of instance
                }

                var (name, type) = _constructorParams[stepIndex];
                IDescriptor typeDesc = TypeDescriptorRegistry.GetDescriptor( type, 0 );
                return new BufferMemberInfo( stepIndex, name, type, typeDesc );
            }

            int memberIndex = stepIndex - ctorCount;
            if( memberIndex < 0 || memberIndex >= _members.Count )
            {
                // This shouldn't happen if StepCount is correct, but safety first.
                return null;
            }

            var def = _members[memberIndex];
            return def.Resolve( target );
        }

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            int ctorCount = GetConstructionStepCount( null );

            if( ctorCount > 0 )
                return new object[ctorCount];

            if( _rawFactory != null )
                return _rawFactory( data, ctx );

            if( _simpleFactory != null )
                return _simpleFactory.Invoke();

            if( typeof( T ).IsValueType ) return Activator.CreateInstance<T>();
            try { return Activator.CreateInstance<T>(); }
            catch { return null; }
        }

        public override object Construct( object initialTarget )
        {
            int ctorCount = GetConstructionStepCount( initialTarget );

            if( ctorCount > 0 && _constructor != null && initialTarget is object[] )
            {
                return _constructor.DynamicInvoke( initialTarget );
            }
            return initialTarget;
        }

        public override void OnSerializing( object target, SerializationContext context )
        {
            _onSerializing?.Invoke( (T)target, context );
        }

        public override void OnDeserialized( object target, SerializationContext context )
        {
            _onDeserialized?.Invoke( (T)target, context );
        }

        public override int GetMethodCount() => _methods.Count;
        public override IMethodInfo GetMethodInfo( int methodIndex ) => _methods[methodIndex];

        // --- Internal Definitions ---

        private readonly struct BufferMemberInfo : IMemberInfo
        {
            public string Name { get; }
            public int Index => -1; // Constructor args are named
            public Type MemberType { get; }
            public IDescriptor TypeDescriptor { get; }
            public bool RequiresWriteBack => MemberType.IsValueType;

            private readonly int _index;

            public BufferMemberInfo( int index, string name, Type type, IDescriptor desc )
            {
                _index = index;
                Name = name;
                MemberType = type;
                TypeDescriptor = desc;
            }

            public object GetValue( object target ) => ((object[])target)[_index];
            public void SetValue( ref object target, object value ) => ((object[])target)[_index] = value;
        }

        private class MemberDefinition<TMember>
        {
            public string Name { get; }
            public ContextKey Context { get; }
            public Func<object, TMember> Getter;
            public Action<object, TMember> Setter;
            public RefSetter<object, TMember> RefSetter;
            public MemberInfo NativeMember;
            public Type MemberType;

            public Predicate<object> ShouldSerialize;
            public Func<object, SerializationContext, bool> ShouldSerializeWithContext;

            public MemberDefinition( string name, ContextKey context, Func<object, TMember> getter, Action<object, TMember> setter, RefSetter<object, TMember> refSetter, MemberInfo nativeMember, Type memberType = null )
            {
                Name = name;
                Context = context;
                Getter = getter;
                Setter = setter;
                RefSetter = refSetter;
                NativeMember = nativeMember;
                MemberType = memberType ?? typeof( TMember );
            }

            public IMemberInfo Resolve( object target )
            {
                TMember val = default;
                if( target != null && target is not object[] ) // Ensure we don't try to get member from buffer
                    val = Getter( target );

                if( ShouldSerialize != null && !ShouldSerialize( target ) )
                    return new SkippedMemberInfo( Name, MemberType );

                if( ShouldSerializeWithContext != null )
                    return new ConditionalMemberInfo<TMember>( this, val );

                Type actualType = val == null ? MemberType : val.GetType();

                IDescriptor desc = TypeDescriptorRegistry.GetDescriptor( actualType, Context )
                                    ?? TypeDescriptorRegistry.GetDescriptor( MemberType, Context );

                return new RuntimeMemberInfo<TMember>( Name, desc, Getter, Setter, RefSetter, NativeMember, MemberType );
            }
        }

        private class ConditionalMemberInfo<TMember> : IMemberInfo
        {
            private readonly MemberDefinition<TMember> _def;
            private readonly TMember _val;

            public ConditionalMemberInfo( MemberDefinition<TMember> def, TMember val )
            {
                _def = def;
                _val = val;
            }

            public string Name => _def.Name;
            public int Index => -1;
            public Type MemberType => _def.MemberType;
            public bool RequiresWriteBack => MemberType.IsValueType;

            public IDescriptor TypeDescriptor
            {
                get
                {
                    Type actualType = _val == null ? MemberType : _val.GetType();
                    return TypeDescriptorRegistry.GetDescriptor( actualType, _def.Context )
                        ?? TypeDescriptorRegistry.GetDescriptor( MemberType, _def.Context );
                }
            }

            public object GetValue( object target ) => _def.Getter( target );
            public void SetValue( ref object target, object value )
            {
                if( _def.RefSetter != null ) _def.RefSetter( ref target, (TMember)value );
                else if( _def.Setter != null ) _def.Setter( target, (TMember)value );
            }
        }

        private readonly struct SkippedMemberInfo : IMemberInfo
        {
            public string Name { get; }
            public readonly int Index => -1;
            public Type MemberType { get; }
            public IDescriptor TypeDescriptor => null;
            public bool RequiresWriteBack => false;

            public SkippedMemberInfo( string name, Type type )
            {
                Name = name;
                MemberType = type;
            }

            public object GetValue( object target ) => null;
            public void SetValue( ref object target, object value ) { }
        }

        private readonly struct RuntimeMemberInfo<TMember> : IMemberInfo
        {
            public string Name { get; }
            public int Index => -1;
            public Type MemberType { get; }
            public IDescriptor TypeDescriptor { get; }
            public bool RequiresWriteBack => MemberType.IsValueType;

            private readonly Func<object, TMember> _getter;
            private readonly Action<object, TMember> _setter;
            private readonly RefSetter<object, TMember> _refSetter;
            private readonly MemberInfo _nativeMember;

            public RuntimeMemberInfo( string name, IDescriptor desc, Func<object, TMember> getter, Action<object, TMember> setter, RefSetter<object, TMember> refSetter, MemberInfo nativeMember, Type memberType )
            {
                Name = name;
                TypeDescriptor = desc;
                _getter = getter;
                _setter = setter;
                _refSetter = refSetter;
                _nativeMember = nativeMember;
                MemberType = memberType;
            }

            public object GetValue( object target ) => _getter( target );

            public void SetValue( ref object target, object value )
            {
                if( _refSetter != null )
                {
                    _refSetter( ref target, (TMember)value );
                }
                else if( _setter != null )
                {
                    _setter( target, (TMember)value );
                }
            }
        }

        private class ActionMethodInfo : IMethodInfo
        {
            public string Name { get; }
            public string DisplayName { get; }
            public bool IsStatic => false;
            public bool IsGeneric => false;
            public string[] GenericTypeParameters => Array.Empty<string>();
            public IParameterInfo[] Parameters => Array.Empty<IParameterInfo>();

            private readonly Action<T> _action;

            public ActionMethodInfo( string name, string displayName, Action<T> action )
            {
                Name = name;
                DisplayName = displayName ?? name;
                _action = action;
            }

            public object Invoke( object target, object[] parameters )
            {
                _action( (T)target );
                return null;
            }
        }
    }
}
