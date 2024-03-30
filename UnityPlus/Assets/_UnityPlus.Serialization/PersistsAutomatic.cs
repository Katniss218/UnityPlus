using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class PersistsAutomatic
    {
        private struct PersistentField
        {
            public FieldInfo f;
            public PersistAttribute attr;

            public PersistentField( FieldInfo f, PersistAttribute attr )
            {
                this.f = f;
                this.attr = attr;
            }
        }

        private struct PersistentProperty
        {
            public PropertyInfo p;
            public PersistAttribute attr;

            public PersistentProperty( PropertyInfo p, PersistAttribute attr )
            {
                this.p = p;
                this.attr = attr;
            }
        }

        private static readonly Dictionary<Type, (PersistentField[] fields, PersistentProperty[] properties)> _referencepersistentMembers = new();
        private static readonly Dictionary<Type, (PersistentField[] fields, PersistentProperty[] properties)> _datapersistentMembers = new();

        private static ((PersistentField[] fields, PersistentProperty[] properties) refT, (PersistentField[] fields, PersistentProperty[] properties) dataT) CacheType( Type type )
        {
            List<PersistentField> finalDataFields = new();
            List<PersistentField> finalReferenceFields = new();
            List<PersistentProperty> finalDataProperties = new();
            List<PersistentProperty> finalReferenceProperties = new();

            FieldInfo[] fields = type.GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
            foreach( var field in fields )
            {
                PersistAttribute attr = field.GetCustomAttribute<PersistAttribute>();
                if( attr != null )
                {
                    if( attr.PersistsReference )
                        finalReferenceFields.Add( new PersistentField( field, attr ) );
                    if( attr.PersistsData )
                        finalDataFields.Add( new PersistentField( field, attr ) );
                }
            }

            PropertyInfo[] properties = type.GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
            foreach( var property in properties )
            {
                PersistAttribute attr = property.GetCustomAttribute<PersistAttribute>();
                if( attr != null )
                {
                    if( attr.PersistsReference )
                        finalReferenceProperties.Add( new PersistentProperty( property, attr ) );
                    if( attr.PersistsData )
                        finalDataProperties.Add( new PersistentProperty( property, attr ) );
                }
            }

            var refTuple = (finalReferenceFields.ToArray(), finalReferenceProperties.ToArray());
            var dataTuple = (finalDataFields.ToArray(), finalDataProperties.ToArray());

            _referencepersistentMembers.Add( type, refTuple );
            _datapersistentMembers.Add( type, dataTuple );
            return (refTuple, dataTuple);
        }

        public static void SetObjects( object obj, Type objType, SerializedObject data, IForwardReferenceMap l )
        {
            if( !_datapersistentMembers.TryGetValue( objType, out var array ) )
            {
                array = CacheType( objType ).refT;
            }

            foreach( var field in array.fields ) // for each field, save the field, and whatever it owns
            {
                if( data.TryGetValue<SerializedObject>( field.attr.Key, out var fieldData ) )
                {
                    object fieldValue = ObjectFactory.Create( fieldData, l );

                    fieldValue.SetObjects( fieldData, l );
                    field.f.SetValue( obj, fieldValue );
                }
            }
            foreach( var property in array.properties )
            {
                if( data.TryGetValue<SerializedObject>( property.attr.Key, out var propertyData ) )
                {
                    object fieldValue = ObjectFactory.Create( propertyData, l );

                    fieldValue.SetObjects( propertyData, l );
                    property.p.SetValue( obj, fieldValue );
                }
            }
        }

        public static SerializedObject GetObjects( object obj, Type objType, IReverseReferenceMap s )
        {
            if( !_referencepersistentMembers.TryGetValue( objType, out var array ) )
            {
                array = CacheType( objType ).refT;
            }

            SerializedObject data = new SerializedObject();

            foreach( var field in array.fields ) // for each field, save the field, and whatever it owns
            {
                object fieldValue = field.f.GetValue( obj );

                SerializedObject so = fieldValue.GetObjects( s );
                data.Add( field.attr.Key, so );
            }
            foreach( var property in array.properties )
            {
                object propertyValue = property.p.GetValue( obj );

                SerializedObject so = propertyValue.GetObjects( s );
                data.Add( property.attr.Key, so );
            }
            return data;
        }


        public static SerializedData GetData( object obj, Type objType, IReverseReferenceMap s )
        {
            if( !_datapersistentMembers.TryGetValue( objType, out var array ) )
            {
                array = CacheType( objType ).refT;
            }

            SerializedObject rootSO = new SerializedObject();

            foreach( var field in array.fields ) // for each field, save the field, and whatever it owns
            {
                object fieldValue = field.f.GetValue( obj );

                SerializedData so = fieldValue.GetData( s );
                rootSO.Add( field.attr.Key, so );
            }
            foreach( var property in array.properties )
            {
                object propertyValue = property.p.GetValue( obj );

                SerializedData so = propertyValue.GetData( s );
                rootSO.Add( property.attr.Key, so );
            }
            return rootSO;
        }


        public static void SetData( object obj, Type objType, IForwardReferenceMap l, SerializedData data )
        {
            if( !_datapersistentMembers.TryGetValue( objType, out var array ) )
            {
                array = CacheType( objType ).refT;
            }

            foreach( var field in array.fields ) // for each field, save the field, and whatever it owns
            {
                if( data.TryGetValue( field.attr.Key, out var fieldData ) )
                {
                    object fieldValue = field.f.GetValue( obj );
                    if( fieldValue != null )
                    {
                        fieldValue.SetData( l, fieldData );
                    }
                }
            }
            foreach( var property in array.properties )
            {
                if( data.TryGetValue( property.attr.Key, out var propertyData ) )
                {
                    object propertyValue = property.p.GetValue( obj );
                    if( propertyValue != null )
                    {
                        propertyValue.SetData( l, propertyData );
                    }
                }
            }
        }
    }
}