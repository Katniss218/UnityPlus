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

    // Update is called once per frame
    void Start()
    {
        GameObject f3 = Registry.Get<GameObject>( "resources::Cube" );
        IFactory<GameObject> f2 = Registry.Get<IFactory<GameObject>>( "defaultfac" );

        Debug.Log( f2 );
        Debug.Log( f3 );

        f2.Create();

        SceneFactoryGameObjectDataStrategy strat = new SceneFactoryGameObjectDataStrategy();

        Saver s = new Saver( "test", new System.Action<Saver>[] { strat.SaveSceneObjects_Data }, new System.Action<Saver>[] { strat.SaveSceneObjects_Object } );

        s.Save();
    }
}