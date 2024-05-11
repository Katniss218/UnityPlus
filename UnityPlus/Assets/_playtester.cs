using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityPlus.AssetManagement;
using UnityPlus.CSharp;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;
using UnityPlus.Serialization.Mappings;
using UnityPlus.Serialization.ReferenceMaps;
using UnityPlus.Serialization.Strategies;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

public class _playtester : MonoBehaviour
{
    [SerializationMappingProvider( typeof( _playtester ) )]
    public static SerializationMapping _playtesterMapping()
    {
        return new CompoundMapping<_playtester>()
        {
        }.WithFactory( ( data, l ) =>
        {
            Guid id = data[KeyNames.ID].DeserializeGuid();

            _playtester c = (_playtester)l.GetObj( id );

            return c;
        } );
    }

    void Start()
    {
        var mapping = SerializationMapping.GetMappingFor( this.gameObject );

        SerializedData data = mapping.Save( this.gameObject, new BidirectionalReferenceStore() );



        StringBuilder sb = new StringBuilder();
        new JsonStringWriter( data, sb ).Write();
        string s = sb.ToString();
        s = s.Replace( "true", "false" );

        data = new JsonStringReader( s ).Read();

        object obj = mapping.Load( data, new BidirectionalReferenceStore() );
    }
    
    void Update()
    {
        List<GameObject> list = new List<GameObject>( 100 );
        for( int i = 0; i < 1000; i++ )
        {
            Profiler.BeginSample( "t1" );

            var mapping = SerializationMapping.GetMappingFor( this.gameObject );
            SerializedData data = mapping.Save( this.gameObject, new BidirectionalReferenceStore() );

            Profiler.EndSample();
            BidirectionalReferenceStore refStore = new BidirectionalReferenceStore();

            Profiler.BeginSample( "t2" );

            mapping = SerializationMapping.GetMappingFor( this.gameObject );
            GameObject go = (GameObject)mapping.Load( data, refStore );
            mapping.LoadReferences( go, data, refStore );

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