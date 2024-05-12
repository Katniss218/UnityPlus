using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Extensions;

namespace UnityPlus.Serialization.Mappings
{
    public static class Mappings
    {
        [SerializationMappingProvider( typeof( float ) )]
        public static SerializationMapping FloatMapping()
        {
            return new DirectSerializationMapping<float>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (float)data
            };
        }

        [SerializationMappingProvider( typeof( string ) )]
        public static SerializationMapping StringMapping()
        {
            return new DirectSerializationMapping<string>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (string)data
            };
        }

        [SerializationMappingProvider( typeof( int ) )]
        public static SerializationMapping Int32Mapping()
        {
            return new DirectSerializationMapping<int>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (int)data
            };
        }

        [SerializationMappingProvider( typeof( bool ) )]
        public static SerializationMapping BooleanMapping()
        {
            return new DirectSerializationMapping<bool>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (bool)data
            };
        }

        [SerializationMappingProvider( typeof( Vector3 ) )]
        public static SerializationMapping Vector3Mapping()
        {
            return new DirectSerializationMapping<Vector3>()
            {
                SaveFunc = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z },
                LoadFunc = ( data, l ) => new Vector3( (float)data[0], (float)data[1], (float)data[2] )
            };
        }

        [SerializationMappingProvider( typeof( Quaternion ) )]
        public static SerializationMapping QuaternionMapping()
        {
            return new DirectSerializationMapping<Quaternion>()
            {
                SaveFunc = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z, (SerializedPrimitive)o.w },
                LoadFunc = ( data, l ) => new Quaternion( (float)data[0], (float)data[1], (float)data[2], (float)data[3] )
            };
        }

        [SerializationMappingProvider( typeof( Array ) )]
        public static SerializationMapping ArrayMapping<T>()
        {
#warning TODO - multidimensional arrays?
            return new DirectSerializationMapping<T[]>()
            {
                SaveFunc = ( o, s ) =>
                {
                    SerializedArray serializedArray = new SerializedArray();
                    for( int i = 0; i < o.Length; i++ )
                    {
                        T value = o[i];
                        var mapping = SerializationMappingRegistry.GetMappingOrDefault<T>( value );

                        var data = mapping.Save( value, s );

                        serializedArray.Add( data );
                    }

                    return serializedArray;
                },
                LoadFunc = ( data, l ) =>
                {
                    SerializedArray serializedArray = (SerializedArray)data;

                    T[] o = new T[serializedArray.Count];

                    for( int i = 0; i < serializedArray.Count; i++ )
                    {
                        Type elementType = typeof( T );
                        SerializedData elementData = serializedArray[i];
                        if( elementData.TryGetValue( KeyNames.TYPE, out var elementType2 ) )
                        {
                            elementType = elementType2.ToType();
                        }

                        var mapping = SerializationMappingRegistry.GetMappingOrDefault<T>( elementType );

                        var element = (T)mapping.Load( elementData, l );

                        o[i] = element;
                    }

                    return o;
                }
            };
        }



        [SerializationMappingProvider( typeof( Component ) )]
        public static SerializationMapping ComponentMapping()
        {
            return new CompoundSerializationMapping<Component>()
                .WithFactory( ( data, l ) =>
                {
                    Guid id = data[KeyNames.ID].DeserializeGuid();

                    Component c = (Component)l.GetObj( id );

                    return c;
                } );
        }

        [SerializationMappingProvider( typeof( GameObject ) )]
        public static SerializationMapping GameObjectMapping()
        {
            return new CompoundSerializationMapping<GameObject>()
            {
                ("name", new Member<GameObject, string>( o => o.name )),
                ("layer", new Member<GameObject, int>( o => o.layer )),
                ("is_active", new Member<GameObject, bool>( o => o.activeSelf, (o, value) => o.SetActive(value) )),
                ("is_static", new Member<GameObject, bool>( o => o.isStatic )),
                ("tag", new Member<GameObject, string>( o => o.tag )),
                ("children", new Member<GameObject, GameObject[]>( o =>
                {
                    return o.transform.Children().Select( child => child.gameObject ).ToArray();
                }, (o, value) =>
                {
                    foreach( var child in value )
                    {
                        child.transform.SetParent( o.transform );
                    }
                } )),
                ("components", new Member<GameObject, Component[]>( o => {
                return o.GetComponents();
                }, (o, value) =>
                {
                    // Do nothing, since the instantiated components are already part of the gameobject.
                    // This is very much a hack, but it's how Unity works :shrug:.
                } ))
            }.WithFactory( ( data, l ) =>
            {
                var obj = new GameObject();
                if( data.TryGetValue( KeyNames.ID, out var id ) )
                {
                    l.SetObj( id.DeserializeGuid(), obj );
                }
                // Instantiate components along the gameobject.
                // The component base class factory will then look up the component in the refmap ('$id'), instead of instantiating and setting it.
                if( data.TryGetValue<SerializedArray>( "components", out var components ) )
                {
                    foreach( var compData in components.OfType<SerializedObject>() )
                    {
                        try
                        {
                            Guid id2 = compData[KeyNames.ID].DeserializeGuid();
                            Type type = compData[KeyNames.TYPE].ToType();

                            Component component = obj.GetTransformOrAddComponent( type );
                            if( component is Behaviour behaviour )
                            {
                                // Disable the behaviour to prevent 'start' from firing prematurely if deserializing over multiple frames.
                                // It will be re-enabled by SetData.
                                behaviour.enabled = false;
                            }

                            l.SetObj( id2, component );
                        }
                        catch( Exception ex )
                        {
                            Debug.LogError( $"Failed to deserialize a component with ID: `{(string)compData?[KeyNames.ID] ?? "<null>"}`." );
                            Debug.LogException( ex );
                        }
                    }
                }
                return obj;
            } );
        }

        [SerializationMappingProvider( typeof( Transform ) )]
        public static SerializationMapping TransformMapping()
        {
            return new CompoundSerializationMapping<Transform>()
            {
                ("local_position", new Member<Transform, Vector3>( o => o.localPosition )),
                ("local_rotation", new Member<Transform, Quaternion>( o => o.localRotation )),
                ("local_scale", new Member<Transform, Vector3>( o => o.localScale ))
            }
            .IncludeRecursiveBaseTypeFactory();
        }

        [SerializationMappingProvider( typeof( MeshFilter ) )]
        public static SerializationMapping MeshFilterMapping()
        {
            return new CompoundSerializationMapping<MeshFilter>()
            {
                ("shared_mesh", new MemberAsset<MeshFilter, Mesh>( o => o.sharedMesh ))
            }
            .IncludeRecursiveBaseTypeFactory();
        }
    }
}