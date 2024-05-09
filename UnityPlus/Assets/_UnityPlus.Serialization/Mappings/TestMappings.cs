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
            return new FloatMapping();
        }
        
        [SerializationMappingProvider( typeof( Vector3 ) )]
        public static SerializationMapping Vector3Mapping()
        {
            return new ListMapping<Vector3>()
            {
                new FloatMapping( o => o.x ),
                new FloatMapping( o => o.y ),
                new FloatMapping( o => o.z )
            };
        }

        [SerializationMappingProvider( typeof( MeshFilter ) )]
        public static SerializationMapping MeshFilterMapping()
        {
            return new CompoundMapping<MeshFilter>()
            {
                new AssetDataMapping<MeshFilter, Mesh>( "shared_mesh", o => o.sharedMesh )
            };
        }

        [SerializationMappingProvider( typeof( GameObject ) )]
        public static SerializationMapping GameObjectMapping()
        {
            return new CompoundMapping<GameObject>()
            {
                new DataMapping<GameObject, string>( "name", o => o.name ),
                new DataMapping<GameObject, int>( "layer", o => o.layer ),
                new DataMapping<GameObject, bool>( "is_active", o => o.activeSelf, (o, v) => o.SetActive(v) ),
                new DataMapping<GameObject, bool>( "is_static", o => o.isStatic ),
                new DataMapping<GameObject, string>( "tag", o => o.tag )
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