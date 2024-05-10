using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    public static class MappingUtils
    {
        public static Func<TSource, TMember> CreateGetter<TSource, TMember>( Expression<Func<TSource, TMember>> memberExpression )
        {
            // Ensure the expression is a member access expression
            if( !(memberExpression.Body is MemberExpression memberExp) )
            {
                throw new ArgumentException( "Expression is not a member access expression" );
            }

            // Create parameter for the getter lambda expression
            ParameterExpression instance = Expression.Parameter( typeof( TSource ), "instance" );

            // Create the member access expression
            MemberExpression memberAccess = Expression.MakeMemberAccess( instance, memberExp.Member );

            // Convert the result to object type
            UnaryExpression convert = Expression.Convert( memberAccess, typeof( TMember ) );

            // Compile the lambda expression into a delegate
            return Expression.Lambda<Func<TSource, TMember>>( convert, instance ).Compile();
        }

        public static Action<TSource, TMember> CreateSetter<TSource, TMember>( Expression<Func<TSource, TMember>> memberExpression )
        {
            // Ensure the expression is a member access expression
            if( !(memberExpression.Body is MemberExpression memberExp) )
            {
                throw new ArgumentException( "Expression is not a member access expression" );
            }

            // Create parameters for the setter lambda expression
            ParameterExpression instance = Expression.Parameter( typeof( TSource ), "instance" );
            ParameterExpression value = Expression.Parameter( typeof( TMember ), "value" );

            // Create member access expression with instance parameter
            MemberExpression memberAccess = Expression.MakeMemberAccess( instance, memberExp.Member );

            // Convert the value to the property or field type
            UnaryExpression convertedValue = Expression.Convert( value, memberExp.Type );

            // Create the assignment expression
            BinaryExpression assignment = Expression.Assign( memberAccess, convertedValue );

            // Compile the lambda expression into a delegate
            return Expression.Lambda<Action<TSource, TMember>>( assignment, instance, value ).Compile();
        }
    }
}