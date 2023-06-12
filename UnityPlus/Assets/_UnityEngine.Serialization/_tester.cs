using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AssetManagement;
using UnityEngine.Serialization;
using UnityEngine.Serialization.Factories;
using UnityEngine.Serialization.Strategies;

public class _tester : MonoBehaviour, IPersistent
{
    [SerializeField]
    PrefabAssetFactory fac;

    public JToken GetData( Saver s )
    {
        return new JObject()
        {
            { "fac", Registry.GetAssetID( fac ) }
        };
    }

    public void SetData( Loader l, JToken json )
    {
        throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    void Awake()
    {
        Registry.Register( "defaultfac", fac );
    }

    Saver s;
    Loader l;
    SceneFactoryGameObjectDataStrategy strat;

    // Update is called once per frame
    void Start()
    {
        var x = Resources.LoadAll<Object>( "" );
        foreach( var xx in x )
        {
            Debug.Log( xx.name );
        }

        GameObject f3 = Registry.Get<GameObject>( "resources::Cube" );
        IFactory<GameObject> f2 = Registry.Get<IFactory<GameObject>>( "defaultfac" );

        Debug.Log( f2 );
        Debug.Log( f3 );

        f2.Create();

        strat = new SceneFactoryGameObjectDataStrategy();

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