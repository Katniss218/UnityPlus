using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEditor.Progress;

namespace UnityPlus.Serialization
{
    public class SerializationMappingProviderAttribute : Attribute
    {
        public Type Type { get; set; }

        public SerializationMappingProviderAttribute( Type type )
        {
            this.Type = type;
        }
    }

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

    public abstract class SerializationMappingItem<TSource>
    {
    }

    internal interface IObjectMapping<TSource>
    {
        void GetObjectsPass( TSource mf, SerializedData data, IReverseReferenceMap s );

        void SetObjectsPass( TSource obj, SerializedData data, IForwardReferenceMap l );
    }

    internal interface IDataMapping<TSource>
    {
        void GetDataPass( TSource mf, SerializedData data, IReverseReferenceMap s );

        void SetDataPass( TSource obj, SerializedData data, IForwardReferenceMap l );
    }

    public class DataMapping<TSource, TMember> : SerializationMappingItem<TSource>, IDataMapping<TSource>
    {
        private readonly string _name;
        private readonly Func<TSource, TMember> _getter;
        private readonly Action<TSource, TMember> _setter;

        public DataMapping( string name, Expression<Func<TSource, TMember>> member )
        {
            _name = name;
            _getter = MappingUtils.CreateGetter( member );
            _setter = MappingUtils.CreateSetter( member );
        }

        public DataMapping( string name, Func<TSource, TMember> getter, Action<TSource, TMember> setter )
        {
            _name = name;
            _getter = getter;
            _setter = setter;
        }

        public void GetDataPass( TSource mf, SerializedData root, IReverseReferenceMap s )
        {
            root[_name] = _getter.Invoke( mf ).GetData( s ) ); // TODO - this basically needs to recursively call getdatapass on children.
        }

        public void SetDataPass( TSource obj, SerializedData root, IForwardReferenceMap l )
        {
            if( root.TryGetValue( _name, out var data ) )
                _setter.Invoke( obj, data.AsObject<TMember>() ); // TODO - this basically needs to convert to primitive (call AsXYZ for appropriate type).
        }
    }

    public class AssetDataMapping<TSource, TMember> : SerializationMappingItem<TSource>, IDataMapping<TSource> where TMember : class
    {
        private readonly string _name;
        private readonly Func<TSource, TMember> _getter;
        private readonly Action<TSource, TMember> _setter;

        public AssetDataMapping( string name, Expression<Func<TSource, TMember>> member )
        {
            _name = name;
            _getter = MappingUtils.CreateGetter( member );
            _setter = MappingUtils.CreateSetter( member );
        }

        public void GetDataPass( TSource mf, SerializedData root, IReverseReferenceMap s )
        {
            root[_name] = s.WriteAssetReference( _getter.Invoke( mf ) ) );
        }

        public void SetDataPass( TSource obj, SerializedData root, IForwardReferenceMap l )
        {
            if( root.TryGetValue( _name, out var data ) )
                _setter.Invoke( obj, l.ReadAssetReference<TMember>( data ) );
        }
    }

    public class ObjectMapping<TSource, TMember> : SerializationMappingItem<TSource>, IObjectMapping<TSource>
    {
        private readonly string _name;
        private readonly Func<TSource, TMember> _getter;
        private readonly Action<TSource, TMember> _setter;
        private readonly Func<TSource, TMember> _customFactory = null;

        public ObjectMapping( string name, Expression<Func<TSource, TMember>> member )
        {
            _name = name;
            _getter = MappingUtils.CreateGetter( member );
            _setter = MappingUtils.CreateSetter( member );
        }

        public ObjectMapping( string name, Expression<Func<TSource, TMember>> member, Func<TSource, TMember> customFactory )
        {
            _name = name;
            _getter = MappingUtils.CreateGetter( member );
            _setter = MappingUtils.CreateSetter( member );
            _customFactory = customFactory;
        }

        public void GetObjectsPass( TSource mf, SerializedData root, IReverseReferenceMap s )
        {
            root[_name] = _getter.Invoke( mf ).GetData( s ) ); // TODO - this needs to create `$id` and `$type` keys, and recursively call getobjects on the children.
        }

        public void SetObjectsPass( TSource obj, SerializedData root, IForwardReferenceMap l )
        {
            //if( root.TryGetValue( _name, out var data ) )
            //    _setter.Invoke( obj, data.AsObject<TMember>() ); // TODO - this basically needs to convert to non-primitive (call AsXYZ for appropriate type).

            if( root.TryGetValue( _name, out var data ) )
                _setter.Invoke( obj, SerializationMapping.Mappings.GetClosestOrDefault( typeof(TMember) ).SetObjectsPass( data.AsObject<TMember>() );
        }
    }

    public class SerializationMapping
    {
        public static TypeMap<SerializationMapping> Mappings { get; } = new();
    }

    public class FloatMapping : SerializationMapping
    {

    }

    public class ListMapping<TSource> : SerializationMapping
    {
        public SerializedObject GetObjectsPass( TSource obj, IReverseReferenceMap s )
        {
            // get data from all members

            SerializedObject root = new SerializedObject();

            return root;
        }

        public void SetObjectsPass( TSource obj, SerializedObject root, IForwardReferenceMap l )
        {
        }

        public SerializedObject GetDataPass( TSource obj, IReverseReferenceMap s )
        {
            // get data from all members

            SerializedObject root = new SerializedObject();

            return root;
        }

        public void SetDataPass( TSource obj, SerializedObject root, IForwardReferenceMap l )
        {
        }
    }

    public class CompoundMapping<TSource> : SerializationMapping, IEnumerable<SerializationMappingItem<TSource>>
    {
        private List<SerializationMappingItem<TSource>> _items = new();

        public void Add( SerializationMappingItem<TSource> item )
        {
            _items.Add( item );
        }

        public IEnumerator<SerializationMappingItem<TSource>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public SerializedObject GetObjectsPass( TSource obj, IReverseReferenceMap s )
        {
            // get data from all members

            SerializedObject root = new SerializedObject();

            foreach( var item in _items )
            {
                if( item is IObjectMapping<TSource> dm )
                {
                    dm.GetObjectsPass( obj, root, s );
                }
            }

            return root;
        }

        public void SetObjectsPass( TSource obj, SerializedObject root, IForwardReferenceMap l )
        {
            foreach( var item in _items )
            {
                if( item is IObjectMapping<TSource> dm )
                {
                    dm.SetObjectsPass( obj, root, l );
                }
            }
        }

        public SerializedObject GetDataPass( TSource obj, IReverseReferenceMap s )
        {
            // get data from all members

            SerializedObject root = new SerializedObject();

            foreach( var item in _items )
            {
                if( item is IDataMapping<TSource> dm )
                {
                    dm.GetDataPass( obj, root, s );
                }
            }

            return root;
        }

        public void SetDataPass( TSource obj, SerializedObject root, IForwardReferenceMap l )
        {
            foreach( var item in _items )
            {
                if( item is IDataMapping<TSource> dm )
                {
                    dm.SetDataPass( obj, root, l );
                }
            }
        }
    }
}