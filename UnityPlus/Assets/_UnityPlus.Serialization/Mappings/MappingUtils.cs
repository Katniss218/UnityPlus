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
            if( !(memberExpression.Body is MemberExpression memberExp) )
            {
                throw new ArgumentException( "Expression is not a member access expression" );
            }

            ParameterExpression instance = Expression.Parameter( typeof( TSource ), "instance" );

            MemberExpression memberAccess = Expression.MakeMemberAccess( instance, memberExp.Member );

            UnaryExpression convert = Expression.Convert( memberAccess, typeof( TMember ) );

            return Expression.Lambda<Func<TSource, TMember>>( convert, instance )
                .Compile();
        }

        public static Action<TSource, TMember> CreateSetter<TSource, TMember>( Expression<Func<TSource, TMember>> memberExpression )
        {
            if( !(memberExpression.Body is MemberExpression memberExp) )
            {
                throw new ArgumentException( "Expression is not a member access expression" );
            }

            ParameterExpression instance = Expression.Parameter( typeof( TSource ), "instance" );
            ParameterExpression value = Expression.Parameter( typeof( TMember ), "value" );

            MemberExpression memberAccess = Expression.MakeMemberAccess( instance, memberExp.Member );

            UnaryExpression convertedValue = Expression.Convert( value, memberExp.Type );

            BinaryExpression assignment = Expression.Assign( memberAccess, convertedValue );

            return Expression.Lambda<Action<TSource, TMember>>( assignment, instance, value )
                .Compile();
        }
    }
}