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
        var vec = new Vector3( 42.23425f, -342.2345f, 2356532f );
        string sJson = JsonConvert.SerializeObject( this, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Error = ( serializer, err ) => err.ErrorContext.Handled = true
        } );


        var b = new MoreDerivedClass();
        var mapping = SerializationMappingRegistry.GetMappingOrDefault<DerivedClass>( ObjectContext.Default, b );

        var mapping2 = SerializationMappingRegistry.GetMappingOrDefault<MonoBehaviour>( ObjectContext.Default, (MonoBehaviour)null );

        mapping2 = SerializationMappingRegistry.GetMappingOrDefault<Component>( ObjectContext.Default, this );

        var data3 = SerializationUnit.Serialize( "teststr" );

        var su = SerializationUnit.FromObjects( this.gameObject );
        su.Serialize();
        SerializedData data2 = su.GetDataOfType<GameObject>().First();

        StringBuilder sb = new StringBuilder();
        new JsonStringWriter( data2, sb ).Write();
        string s = sb.ToString();
        //s = s.Replace( "true", "false" );

        var data = new JsonStringReader( s ).Read();

        GameObject obj = SerializationUnit.Deserialize<GameObject>( data );

        _playtester t = obj.GetComponent<_playtester>();
    }

    void Update()
    {
       // RunPerfTest();
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