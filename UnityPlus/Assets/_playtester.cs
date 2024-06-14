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
    [SerializationMappingProvider( typeof( _playtester ) )]
    public static SerializationMapping _playtesterMapping()
    {
        return new MemberwiseSerializationMapping<_playtester>()
        {
            ("perf_test_go", new Member<_playtester, GameObject>( ObjectContext.Ref, o => o.perfTestGo )),
            ("action", new Member<_playtester, Action<string>>( o => o.Action ))
        }
        .UseBaseTypeFactory();
    }
    
    [SerializationMappingProvider( typeof( FPSCounterDebug ) )]
    public static SerializationMapping FPSCounterDebugMapping()
    {
        return new MemberwiseSerializationMapping<FPSCounterDebug>()
        {
            ("fps", new Member<FPSCounterDebug, float>( o => o.fps ))
        }
        .UseBaseTypeFactory();
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
        var mapping = SerializationMappingRegistry.GetMappingOrDefault<_playtester>( ObjectContext.Default, this );

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