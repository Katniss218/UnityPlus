using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.AssetManagement;
using UnityEngine.Serialization;
using UnityEngine.Serialization.Json;
using UnityEngine.Serialization.Strategies;

public class _tester : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {

    }

    Saver s;
    Loader l;
    PrefabAndDataStrategy strat;

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
        SerializedObject val = (SerializedObject)sut.Parse();


        SerializedObject serobject = (SerializedObject)new UnityEngine.Serialization.Json.JsonStringReader( System.IO.File.ReadAllText( "c:/test/testjson.json" ) ).Parse();

        using( MemoryStream s = new MemoryStream() )
        {
            new JsonStreamWriter( serobject, s ).Write();
            string ssss = Encoding.UTF8.GetString( s.ToArray() );
        }

        GameObject f2 = AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/Cube2" );

        ClonedGameObject.Instantiate( f2 );

        strat = new PrefabAndDataStrategy();

        s = new Saver( "test", new System.Action<Saver>[] { strat.SaveSceneObjects_Data }, new System.Action<Saver>[] { strat.SaveSceneObjects_Object } );

        l = new Loader( "test", new System.Action<Loader>[] { strat.LoadSceneObjects_Object }, new System.Action<Loader>[] { strat.LoadSceneObjects_Data } );
    }

    void Update()
    {
        if( Input.GetKeyDown( KeyCode.S ) )
        {
            s.Save();
        }
        if( Input.GetKeyDown( KeyCode.L ) )
        {
            l.Load();
        }
    }
}