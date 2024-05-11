using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Extensions;

namespace UnityPlus.Serialization.Mappings
{
    public static class TestMappings
    {
        [SerializationMappingProvider( typeof( float ) )]
        public static SerializationMapping FloatMapping()
        {
            return new DirectMapping<float>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (float)data
            };
        }
        [SerializationMappingProvider( typeof( string ) )]
        public static SerializationMapping StringMapping()
        {
            return new DirectMapping<string>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (string)data
            };
        }
        [SerializationMappingProvider( typeof( int ) )]
        public static SerializationMapping Int32Mapping()
        {
            return new DirectMapping<int>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (int)data
            };
        }
        [SerializationMappingProvider( typeof( bool ) )]
        public static SerializationMapping BooleanMapping()
        {
            return new DirectMapping<bool>()
            {
                SaveFunc = ( o, s ) => (SerializedPrimitive)o,
                LoadFunc = ( data, l ) => (bool)data
            };
        }

        [SerializationMappingProvider( typeof( Vector3 ) )]
        public static SerializationMapping Vector3Mapping()
        {
            return new DirectMapping<Vector3>()
            {
                SaveFunc = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z },
                LoadFunc = ( data, l ) => new Vector3( (float)data[0], (float)data[1], (float)data[2] )
            };
        }

        [SerializationMappingProvider( typeof( Quaternion ) )]
        public static SerializationMapping QuaternionMapping()
        {
            return new DirectMapping<Quaternion>()
            {
                SaveFunc = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z, (SerializedPrimitive)o.w },
                LoadFunc = ( data, l ) => new Quaternion( (float)data[0], (float)data[1], (float)data[2], (float)data[3] )
            };
        }



        [SerializationMappingProvider( typeof( GameObject ) )]
        public static SerializationMapping GameObjectMapping()
        {
            return new CompoundMapping<GameObject>()
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

        [SerializationMappingProvider( typeof( Array ) )]
        public static SerializationMapping ArrayMapping<T>()
        {
#warning TODO - multidimensional arrays?
            return new DirectMapping<T[]>()
            {
                SaveFunc = ( o, s ) =>
                {
                    SerializedArray serializedArray = new SerializedArray();
                    for( int i = 0; i < o.Length; i++ )
                    {
                        T value = o[i];
                        var mapping = SerializationMapping.GetMappingFor( value );

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

                        var mapping = SerializationMapping.GetMappingFor<T>( elementType );

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

#warning TODO - some way of automatically including members of the mapping of the base class (recursive) (union the list of members)?
            return new CompoundMapping<Component>()
                .WithFactory( ( data, l ) =>
            {
                Guid id = data[KeyNames.ID].DeserializeGuid();

                Component c = (Component)l.GetObj( id );

                return c;
            } );
        }

        [SerializationMappingProvider( typeof( Transform ) )]
        public static SerializationMapping TransformMapping()
        {
            return new CompoundMapping<Transform>()
            {
                ("local_position", new Member<Transform, Vector3>( o => o.localPosition )),
                ("local_rotation", new Member<Transform, Quaternion>( o => o.localRotation )),
                ("local_scale", new Member<Transform, Vector3>( o => o.localScale ))
            }.WithFactory( ( data, l ) =>
            {
                Guid id = data[KeyNames.ID].DeserializeGuid();

                Transform c = (Transform)l.GetObj( id );

                return c;
            } );
        }

        [SerializationMappingProvider( typeof( MeshFilter ) )]
        public static SerializationMapping MeshFilterMapping()
        {
            return new CompoundMapping<MeshFilter>()
            {
                ("shared_mesh", new MemberAsset<MeshFilter, Mesh>( o => o.sharedMesh ))
            }.WithFactory( ( data, l ) =>
            {
                Guid id = data[KeyNames.ID].DeserializeGuid();

                MeshFilter c = (MeshFilter)l.GetObj( id );

                return c;
            } );

#warning TODO - some way of automatically including members of the mapping of the base class (recursive) (union the list of members)?
        }


        /*
        
        [SerializationMappingProvider( typeof( F2AxisActuator ) )]
        public static SerializationMapping F2AxisActuatorMapping()
        {
            return new SerializationMapping<F2AxisActuator>()
            {
                new ObjectMapping<F2AxisActuator, ControleeInput>( "set_x", act => act.SetX, act => new ControleeInput<float>( act.SetXListener ) ),
                new DataMapping<F2AxisActuator>( "set_x", act => act.SetX )
            };
        }
        
        [SerializationMappingProvider( typeof( List<> ) )]
        public static SerializationMapping List_T_Mapping()
        {
            return new DirectMapping<List<>>()
            {
                AsSerialized = ( o, s ) =>
                {
                    SerializedArray arr = new SerializedArray();
                    for( int i = 0; i < o.Count; i++ )
                    {
                        var mapping = SerializationMapping.GetMappingFor( o[i] );

                        var ser = mapping.GetDataPass( s );
                        arr.Add( ser );
                    }

                    return arr;
                },
                AsObject = ( data, l ) =>
                {
                    SerializedArray arr = new SerializedArray();
                    for( int i = 0; i < o.Count; i++ )
                    {
                        var mapping = SerializationMapping.GetMappingFor( o[i] );

                        var ser = mapping.GetDataPass( s );
                        arr.Add( ser );
                    }

                    return arr;
                }
            };
        }

        
        */
    }
}