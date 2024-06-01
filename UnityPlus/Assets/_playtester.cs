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
           // ("action", new Member<_playtester, Action<string>>(o => o.Action))
        }
        //.IncludeMembers<Behaviour>()
        .UseBaseTypeFactory();
    }

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
        //JsonSerializedDataHandler handler = new JsonSerializedDataHandler();

       // SerializedData data = SerializationUnit.Serialize<GameObject>( this.gameObject );

       // GameObject obj = SerializationUnit.Deserialize<GameObject>( data );

       //SerializationUnit su = SerializationUnit.FromObjects( this.gameObject, this.transform );
        SerializationUnit su = SerializationUnit.FromObjects( this.gameObject );
        su.Serialize();
        SerializedData data = su.GetDataOfType<GameObject>().First();

        //var mapping = SerializationMappingRegistry.GetMappingOrDefault( this.gameObject );

        //SerializedData data = mapping.Save( this.gameObject, new BidirectionalReferenceStore() );



        StringBuilder sb = new StringBuilder();
        new JsonStringWriter( data, sb ).Write();
        string s = sb.ToString();
        //s = s.Replace( "true", "false" );

        data = new JsonStringReader( s ).Read();

        GameObject obj = SerializationUnit.Deserialize<GameObject>( data );

        _playtester t = obj.GetComponent<_playtester>();
    }
    /*
    void Update()
    {
        List<GameObject> list = new List<GameObject>( 100 );

        for( int i = 0; i < 1000; i++ )
        {
            Profiler.BeginSample( "t1" );
            SerializedData data = SerializationUnit.Serialize<GameObject>( this.gameObject );
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
    }*/
}