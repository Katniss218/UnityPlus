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
        SerializedObject serobject = new UnityEngine.Serialization.Json.JsonReader( System.IO.File.ReadAllText( "c:/test/testjson.json" ) ).Parse();

        using( MemoryStream s = new MemoryStream() )
        {
            serobject.WriteJson( s );
            string ssss = Encoding.UTF8.GetString( s.ToArray() );
        }

        var x = Resources.LoadAll<Object>( "" );
        foreach( var xx in x )
        {
            Debug.Log( xx.name );
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