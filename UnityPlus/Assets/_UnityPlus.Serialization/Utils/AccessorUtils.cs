using System;
using System.Linq.Expressions;
using System.Reflection;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Contains static helper methods that create getter/setter lambdas from expressions.
    /// </summary>
    public static class AccessorUtils
    {
        /// <summary>
        /// Creates a getter method from the member access expression.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the expression is not a member access.</exception>
        public static Getter<TSource, TMember> CreateGetter<TSource, TMember>( Expression<Func<TSource, TMember>> memberExpression )
        {
            // Creates the following lambda: `(TSource instance) => instance.member;`
            if( memberExpression.Body is not MemberExpression memberExp )
            {
                throw new ArgumentException( "Expression is not a member access expression" );
            }

            ParameterExpression instance = Expression.Parameter( typeof( TSource ), "instance" );

            MemberExpression memberAccess = Expression.MakeMemberAccess( instance, memberExp.Member );

            return Expression.Lambda<Getter<TSource, TMember>>( memberAccess, instance )
                .Compile();
        }

        /// <summary>
        /// Creates a setter method from the member access expression.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the expression is not a member access.</exception>
        public static Setter<TSource, TMember> CreateSetter<TSource, TMember>( Expression<Func<TSource, TMember>> memberExpression )
        {
            // Creates the following lambda: `(TSource instance, TMember value) => instance.member = value;`
            if( memberExpression.Body is not MemberExpression memberExp )
            {
                throw new ArgumentException( "Expression is not a member access expression" );
            }

            ParameterExpression instance = Expression.Parameter( typeof( TSource ), "instance" );
            ParameterExpression value = Expression.Parameter( typeof( TMember ), "value" );

            MemberExpression memberAccess = Expression.MakeMemberAccess( instance, memberExp.Member );

            BinaryExpression assignment;
            try
            {
                assignment = Expression.Assign( memberAccess, value );
            }
            catch( ArgumentException ex )
            {
                throw new ArgumentException( $"Cannot create setter for member '{memberExp.Member.Name}' on type '{typeof( TSource ).Name}'. The member is likely read-only. Use WithReadonlyMember or WithConstructor/WithFactory for immutable types.", ex );
            }

            return Expression.Lambda<Setter<TSource, TMember>>( assignment, instance, value )
                .Compile();
        }

        /// <summary>
        /// Creates a setter method from the member access expression.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the expression is not a member access.</exception>
        public static RefSetter<TSource, TMember> CreateStructSetter<TSource, TMember>( Expression<Func<TSource, TMember>> memberExpression )
        {
            // Creates the following lambda: `(ref TSource instance, TMember value) => instance = value;`
            if( memberExpression.Body is not MemberExpression memberExp )
            {
                throw new ArgumentException( "Expression is not a member access expression" );
            }

            // Passing by ref allows member-wise loading of `struct` Source types.
            ParameterExpression instance = Expression.Parameter( typeof( TSource ).MakeByRefType(), "instance" );
            ParameterExpression value = Expression.Parameter( typeof( TMember ), "value" );

            MemberExpression memberAccess = Expression.MakeMemberAccess( instance, memberExp.Member );

            BinaryExpression assignment;
            try
            {
                assignment = Expression.Assign( memberAccess, value );
            }
            catch( ArgumentException ex )
            {
                throw new ArgumentException( $"Cannot create setter for member '{memberExp.Member.Name}' on type '{typeof( TSource ).Name}'. The member is likely read-only. Use WithReadonlyMember or WithConstructor/WithFactory for immutable types.", ex );
            }

            return Expression.Lambda<RefSetter<TSource, TMember>>( assignment, instance, value )
                .Compile();
        }

        // --- Untyped Reflection Helpers (for ReflectionFieldInfo) ---

        /// <summary>
        /// Creates an untyped getter (object -> object) for a specific field.
        /// </summary>
        public static Getter<object, object> CreateUntypedGetter( FieldInfo field )
        {
            var targetParam = Expression.Parameter( typeof( object ), "target" );
            var castTarget = Expression.Convert( targetParam, field.DeclaringType );
            var fieldAccess = Expression.Field( castTarget, field );
            var castResult = Expression.Convert( fieldAccess, typeof( object ) );

            return Expression.Lambda<Getter<object, object>>( castResult, targetParam ).Compile();
        }

        /// <summary>
        /// Creates an untyped setter (object -> object) for a specific field on a Class (Reference Type).
        /// </summary>
        public static Setter<object, object> CreateUntypedSetter( FieldInfo field )
        {
            if( field.DeclaringType.IsValueType )
                throw new ArgumentException( "Cannot create a standard setter for a struct field. Use CreateUntypedStructSetter or FieldInfo.SetValue." );

            var targetParam = Expression.Parameter( typeof( object ), "target" );
            var valueParam = Expression.Parameter( typeof( object ), "value" );

            var castTarget = Expression.Convert( targetParam, field.DeclaringType );
            var castValue = Expression.Convert( valueParam, field.FieldType );
            var fieldAccess = Expression.Field( castTarget, field );

            BinaryExpression assign;
            try
            {
                assign = Expression.Assign( fieldAccess, castValue );
            }
            catch( ArgumentException ex )
            {
                throw new ArgumentException( $"Cannot create setter for field '{field.Name}' on type '{field.DeclaringType.Name}'. The field might be init-only or constant.", ex );
            }

            return Expression.Lambda<Setter<object, object>>( assign, targetParam, valueParam ).Compile();
        }

        /// <summary>
        /// Creates an untyped setter (ref object -> object) for a specific field on a Struct (Value Type).
        /// Handles unboxing, assignment, and reboxing.
        /// </summary>
        public static RefSetter<object, object> CreateUntypedStructSetter( FieldInfo field )
        {
            if( !field.DeclaringType.IsValueType )
                throw new ArgumentException( "CreateUntypedStructSetter requires a field on a value type." );

            // (ref object target, object value)
            var targetParam = Expression.Parameter( typeof( object ).MakeByRefType(), "target" );
            var valueParam = Expression.Parameter( typeof( object ), "value" );

            // Local variable for unboxed struct: 'T typed = (T)target;'
            var typedVar = Expression.Variable( field.DeclaringType, "typed" );
            var unbox = Expression.Assign( typedVar, Expression.Convert( targetParam, field.DeclaringType ) );

            // Field assignment: 'typed.field = (FieldType)value;'
            var fieldAccess = Expression.Field( typedVar, field );
            var assign = Expression.Assign( fieldAccess, Expression.Convert( valueParam, field.FieldType ) );

            // Rebox: 'target = (object)typed;'
            var rebox = Expression.Assign( targetParam, Expression.Convert( typedVar, typeof( object ) ) );

            // Block: { unbox; assign; rebox; }
            var block = Expression.Block(
                new[] { typedVar },
                unbox,
                assign,
                rebox
            );

            return Expression.Lambda<RefSetter<object, object>>( block, targetParam, valueParam ).Compile();
        }
        /// <summary>
        /// Creates a compiled lambda to instantiate a type using its parameterless constructor.
        /// </summary>
        public static Func<T> CreateConstructor<T>()
        {
            var newExp = Expression.New( typeof( T ) );
            return Expression.Lambda<Func<T>>( newExp ).Compile();
        }
    }
}