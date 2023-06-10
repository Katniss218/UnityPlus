using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AssetManagement;
using UnityEngine.Serialization.Factories;

public class _tester : MonoBehaviour
{
    [SerializeField]
    PrefabFactory fac;

    // Start is called before the first frame update
    void Start()
    {
        Registry.Register( "defaultfac", fac );

        GameObject f3 = Registry.Get<GameObject>( "resources::Cube" );
        IFactory f = Registry.Get<IFactory>( "defaultfac" );
        IFactory f2 = Registry.Get<IFactory<GameObject>>( "defaultfac" );

        Debug.Log( f );
        Debug.Log( f2 );
        Debug.Log( f3 );

        Instantiate( f3 );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
