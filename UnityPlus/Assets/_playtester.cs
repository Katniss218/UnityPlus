using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using UnityPlus.AssetManagement;
using UnityPlus.CSharp;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;
using UnityPlus.Serialization.Json;
using UnityPlus.Serialization.ReferenceMaps;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

public class _playtester : MonoBehaviour
{
    public class BaseClass
    {
        public float baseMember;
    }

    public class DerivedClass : BaseClass
    {
        public string derivedMember;
    }
    
    public class MoreDerivedClass : DerivedClass
    {
        public string moreDerivedMember;
    }

    public class ReferencingClass
    {
        public BaseClass refMember;
    }

    [SerializationMappingProvider( typeof( _playtester ) )]
    public static SerializationMapping _playtesterMapping()
    {
        return new MemberwiseSerializationMapping<_playtester>()
        {
            ("perf_test_go", new Member<_playtester, GameObject>( ObjectContext.Ref, o => o.perfTestGo )),
            ("action", new Member<_playtester, Action<string>>( o => o.Action ))
        };
    }
    
    [SerializationMappingProvider( typeof( FPSCounterDebug ) )]
    public static SerializationMapping FPSCounterDebugMapping()
    {
        return new MemberwiseSerializationMapping<FPSCounterDebug>()
        {
            ("fps", new Member<FPSCounterDebug, float>( o => o.fps ))
        };
    }

    [SerializationMappingProvider( typeof( BaseClass ) )]
    public static SerializationMapping BaseClassMapping()
    {
        return new MemberwiseSerializationMapping<BaseClass>()
        {
            ("base_member", new Member<BaseClass, float>( o => o.baseMember ))
        };
    }

    [SerializationMappingProvider( typeof( DerivedClass ) )]
    public static SerializationMapping DerivedClassMapping()
    {
        return new MemberwiseSerializationMapping<DerivedClass>()
        {
            ("derived_member", new Member<DerivedClass, string>( o => o.derivedMember ))
        };
    }
    
    [SerializationMappingProvider( typeof( MoreDerivedClass ) )]
    public static SerializationMapping MoreDerivedClassMapping()
    {
        return new MemberwiseSerializationMapping<MoreDerivedClass>()
        {
            ("more_derived_member", new Member<MoreDerivedClass, string>( o => o.moreDerivedMember ))
        };
    }

    [SerializationMappingProvider( typeof( ReferencingClass ) )]
    public static SerializationMapping ReferencingClassMapping()
    {
        return new MemberwiseSerializationMapping<ReferencingClass>()
            {
                ("ref_member", new Member<ReferencingClass, BaseClass>( ObjectContext.Ref, o => o.refMember ))
            };
    }

    [SerializeField] GameObject perfTestGo;

    public Action<string> Action { get; set; }

    private void Awake()
    {
        this.Action = DoSomething;
    }

    private void DoSomething( string s )
    {
        Debug.Log( s );
    }

    void Start()
    {
        // Arrange
        var initialValue = new Vector3( 5.5f, -5f, 0f );
        var refMap = new BidirectionalReferenceStore();

        // Act
        var data = SerializationUnit.Serialize<object>( ObjectContext.Value, initialValue, refMap );

        var data2 = new SerializedObject()
                {
                    { "$type", initialValue.GetType().SerializeType() },
                    { "$id", refMap.GetID( initialValue ).SerializeGuid() },
                    { "value", new SerializedArray()
                        {
                            (SerializedPrimitive)5.5f,
                            (SerializedPrimitive)(-5f),
                            (SerializedPrimitive)0f,
                        } }
                };
    }

    void Update()
    {
        //RunPerfTest();
    }

    private void RunPerfTest()
    {
        const int COUNT = 1000;

        List<GameObject> list = new List<GameObject>( COUNT );

        for( int i = 0; i < COUNT; i++ )
        {
            Profiler.BeginSample( "t1" );
            SerializedData data = SerializationUnit.Serialize<GameObject>( perfTestGo );
            Profiler.EndSample();

            Profiler.BeginSample( "t2" );
            GameObject go = SerializationUnit.Deserialize<GameObject>( data );
            Profiler.EndSample();

            list.Add( go );
        }

        foreach( var go in list.ToArray() )
        {
            Destroy( go );
            list.Clear();
        }
    }
}