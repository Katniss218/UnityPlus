
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// An IMemberInfo implementation that uses compiled Expression Trees for fast field access.
    /// </summary>
    public class ReflectionFieldInfo : IMemberInfo
    {
        public string Name { get; }
        public int Index => -1; // Fields are named, not indexed
        public Type MemberType { get; }
        public bool IsValueType { get; }

        private Func<object, object> _getter;
        private Action<object, object> _setter; // For classes
        private RefSetter<object, object> _structSetter; // For structs

        private ITypeDescriptor _cachedDesc;
        public ITypeDescriptor TypeDescriptor
        {
            get
            {
                if( _cachedDesc == null )
                    _cachedDesc = TypeDescriptorRegistry.GetDescriptor( MemberType, 0 );
                return _cachedDesc;
            }
        }

        public ReflectionFieldInfo( FieldInfo field )
        {
            Name = field.Name;
            MemberType = field.FieldType;
            IsValueType = MemberType.IsValueType;

            // Optimize Getter
            var targetParam = Expression.Parameter( typeof( object ), "target" );
            var castTarget = Expression.Convert( targetParam, field.DeclaringType );
            var fieldAccess = Expression.Field( castTarget, field );
            var castResult = Expression.Convert( fieldAccess, typeof( object ) );
            _getter = Expression.Lambda<Func<object, object>>( castResult, targetParam ).Compile();

            // Optimize Setter
            var valueParam = Expression.Parameter( typeof( object ), "value" );
            var castValue = Expression.Convert( valueParam, MemberType );

            if( field.DeclaringType.IsValueType )
            {
                // Struct setter needs Ref param
                // (ref object target, object value)
                // Note: Expression trees for ref assignment on boxed structs are tricky/unsupported in some Unity versions via lambda.
                // We fallback to standard Reflection for structs to be safe, or use a specific delegate type if possible.
                // For safety and compatibility, we use FieldInfo.SetValue for structs, which handles the unboxing/boxing dance correctly.
                _structSetter = ( ref object target, object value ) => field.SetValue( target, value );
            }
            else
            {
                // Class setter
                var assign = Expression.Assign( fieldAccess, castValue );
                _setter = Expression.Lambda<Action<object, object>>( assign, targetParam, valueParam ).Compile();
            }
        }

        public object GetValue( object target ) => _getter( target );

        public void SetValue( ref object target, object value )
        {
            if( _setter != null ) _setter( target, value );
            else _structSetter( ref target, value );
        }
    }
}