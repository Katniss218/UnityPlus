using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;
using UnityPlus.Serialization.Strategies;

public class _playtester : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {

    }

    Saver s;
    Loader l;
    AsyncSaver _as;
    AsyncLoader _al;
    JsonPrefabAndDataStrategy strat;

    // Update is called once per frame
    void Start()
    {
        string json = @"{
    ""Image"": 
    {
        ""Width"": 800,
        ""Height"": 600,
        ""Title"": ""View from 15th Floor"",
        ""Thumbnail"":
        {
            ""Url"": ""http://www.example.com/image/481989943"",
            ""Height"": 125,
            ""Width"": 100
        },
        ""Animated"" : false,
        ""IDs"": [116, 943, 234, 38793]
    }
}";
        JsonStringReader sut = new JsonStringReader( json );

        // Act
        SerializedObject val = (SerializedObject)sut.Read();

        SerializedObject serobject = (SerializedObject)new JsonStringReader( File.ReadAllText( "c:/test/testjson.json" ) ).Read();

        using( MemoryStream s = new MemoryStream() )
        {
            new JsonStreamWriter( serobject, s ).Write();
            string ssss = Encoding.UTF8.GetString( s.ToArray() );
        }

        GameObject f2 = AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/Cube2" );

        ClonedGameObject.Instantiate( f2 );

        strat = new JsonPrefabAndDataStrategy();

        // s = new Saver( "test", new Action<ISaver>[] { strat.SaveSceneObjects_Data }, new Action<ISaver>[] { strat.SaveSceneObjects_Object } );

        // l = new Loader( "test", new Action<ILoader>[] { strat.LoadSceneObjects_Object }, new Action<ILoader>[] { strat.LoadSceneObjects_Data } );

        _as = new AsyncSaver( "test", new Func<ISaver, IEnumerator>[] { strat.SaveSceneObjects_Data }, new Func<ISaver, IEnumerator>[] { strat.SaveSceneObjects_Object } );

        _al = new AsyncLoader( "test", new Func<ILoader, IEnumerator>[] { strat.LoadSceneObjects_Object }, new Func<ILoader, IEnumerator>[] { strat.LoadSceneObjects_Data } );
    }

    void Update()
    {
        if( Input.GetKeyDown( KeyCode.S ) )
        {
            _as.SaveAsync( this );
        }
        if( Input.GetKeyDown( KeyCode.L ) )
        {
            _al.LoadAsync( this );
        }
    }
}