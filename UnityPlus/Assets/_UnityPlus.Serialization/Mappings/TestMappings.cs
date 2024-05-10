using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        [SerializationMappingProvider( typeof( Vector3 ) )]
        public static SerializationMapping Vector3Mapping()
        {
            return new ListMapping<Vector3>()
            {
                new MemberData<Vector3, float>( o => o.x ),
                new MemberData<Vector3, float>( o => o.y ),
                new MemberData<Vector3, float>( o => o.z )
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

        [SerializationMappingProvider( typeof( Transform ) )]
        public static SerializationMapping TransformMapping()
        {
            return new CompoundMapping<Transform>()
            {
                ("local_position", new MemberData<Transform, Vector3>( o => o.localPosition )),
                ("local_rotation", new MemberData<Transform, Quaternion>( o => o.localRotation )),
                ("local_scale", new MemberData<Transform, Vector3>( o => o.localScale ))
            };
        }

        [SerializationMappingProvider( typeof( GameObject ) )]
        public static SerializationMapping GameObjectMapping()
        {
            return new CompoundMapping<GameObject>()
            {
                ("name", new MemberData<GameObject, string>( o => o.name )),
                ("layer", new MemberData<GameObject, int>( o => o.layer )),
                ("is_active", new MemberData<GameObject, bool>( o => o.activeSelf, (o, v) => o.SetActive(v) )),
                ("is_static", new MemberData<GameObject, bool>( o => o.isStatic )),
                ("tag", new MemberData<GameObject, string>( o => o.tag )),
                ("children", new ListMapping<GameObject, GameObject>( o => o.)),
                ("components", new ListMapping<GameObject, Component>( o => o.))
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
            return new SerializationMapping()
            {
                ("")
            }
        }*/
    }
}