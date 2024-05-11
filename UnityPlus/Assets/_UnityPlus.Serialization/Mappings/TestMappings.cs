using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization.Mappings
{
    public static class TestMappings
    {
        [SerializationMappingProvider( typeof( float ) )]
        public static SerializationMapping FloatMapping()
        {
            return new DirectMapping<float>()
            {
                AsSerialized = ( o, s ) => (SerializedPrimitive)o,
                AsObject = ( data, l ) => (float)data
            };
        }
        [SerializationMappingProvider( typeof( string ) )]
        public static SerializationMapping StringMapping()
        {
            return new DirectMapping<string>()
            {
                AsSerialized = ( o, s ) => (SerializedPrimitive)o,
                AsObject = ( data, l ) => (string)data
            };
        }
        [SerializationMappingProvider( typeof( int ) )]
        public static SerializationMapping Int32Mapping()
        {
            return new DirectMapping<int>()
            {
                AsSerialized = ( o, s ) => (SerializedPrimitive)o,
                AsObject = ( data, l ) => (int)data
            };
        }
        [SerializationMappingProvider( typeof( bool ) )]
        public static SerializationMapping BooleanMapping()
        {
            return new DirectMapping<bool>()
            {
                AsSerialized = ( o, s ) => (SerializedPrimitive)o,
                AsObject = ( data, l ) => (bool)data
            };
        }

        [SerializationMappingProvider( typeof( Vector3 ) )]
        public static SerializationMapping Vector3Mapping()
        {
            return new DirectMapping<Vector3>()
            {
                AsSerialized = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z },
                AsObject = ( data, l ) => new Vector3( (float)data[0], (float)data[1], (float)data[2] )
            };
        }

        [SerializationMappingProvider( typeof( Quaternion ) )]
        public static SerializationMapping QuaternionMapping()
        {
            return new DirectMapping<Quaternion>()
            {
                AsSerialized = ( o, s ) => new SerializedArray() { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z, (SerializedPrimitive)o.w },
                AsObject = ( data, l ) => new Quaternion( (float)data[0], (float)data[1], (float)data[2], (float)data[3] )
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
               // ("children", new MemberData<GameObject, List<GameObject>>( o => o.transform.Children().Select(t=>t.gameObject).ToList(), null)),
              //  ("components", new MemberData<GameObject, Component[]>( o => o.GetComponents(), null))
            }.WithFactory( ( data, l ) =>
            {
                var obj = new GameObject();
                if( data.TryGetValue( KeyNames.ID, out var id ) )
                {
                    l.SetObj( id.DeserializeGuid(), obj );
                }
                return obj;
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
            };
        }

        [SerializationMappingProvider( typeof( MeshFilter ) )]
        public static SerializationMapping MeshFilterMapping()
        {
            return new CompoundMapping<MeshFilter>()
            {
                ("shared_mesh", new MemberAsset<MeshFilter, Mesh>( o => o.sharedMesh ))

#warning TODO - some way of automatically including members of the mapping of the base class (recursive) (union the list of members)?
            };
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