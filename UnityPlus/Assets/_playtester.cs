using System;
using System.Collections.Generic;
using System.IO;
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
        return new CompoundSerializationMapping<_playtester>()
        {
            ("action", new Member<_playtester, Action<string>>(o => o.Action))
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
#warning TODO - specify what objects we want to save.
        //Saver saver = new Saver( new BidirectionalReferenceStore(), )

            #warning saver/loader work with a single serialization unit/group.

        var mapping = SerializationMappingRegistry.GetMappingOrDefault( this.gameObject );

        SerializedData data = mapping.Save( this.gameObject, new BidirectionalReferenceStore() );



        StringBuilder sb = new StringBuilder();
        new JsonStringWriter( data, sb ).Write();
        string s = sb.ToString();
        //s = s.Replace( "true", "false" );

        data = new JsonStringReader( s ).Read();

        var refstore = new BidirectionalReferenceStore();
        object obj = mapping.Load( data, refstore );
        mapping.LoadReferences( ref obj, data, refstore );

        _playtester t = ((GameObject)obj).GetComponent<_playtester>();
    }
    /*
    void Update()
    {
        List<GameObject> list = new List<GameObject>( 100 );
        for( int i = 0; i < 1000; i++ )
        {
            Profiler.BeginSample( "t1" );

            var mapping = SerializationMappingRegistry.GetMappingOrDefault( this.gameObject );
            SerializedData data = mapping.Save( this.gameObject, new BidirectionalReferenceStore() );

            Profiler.EndSample();
            BidirectionalReferenceStore refStore = new BidirectionalReferenceStore();

            Profiler.BeginSample( "t2" );

            mapping = SerializationMappingRegistry.GetMappingOrDefault( this.gameObject );
            object go = mapping.Load( data, refStore );
            mapping.LoadReferences( ref go, data, refStore );

            Profiler.EndSample();
            list.Add( (GameObject)go );
        }
        foreach( var go in list.ToArray() )
        {
            Destroy( go );
            list.Clear();
        }
    }*/
}