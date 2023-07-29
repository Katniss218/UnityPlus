using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.CSharp;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;
using UnityPlus.Serialization.Strategies;

public class _playtester : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        string code = @"
using System;
using UnityEngine;

public class CodeGenerated
{
    public static void Run()
    {
        GameObject go = new GameObject();
        object o = ""hi"";
        UnityEngine.Debug.Log(o);
        GameObject go2 = GameObject.Find(""Plane"");
        go2.transform.rotation = Quaternion.Euler( -31, -31, -31 );
    }
}";

        Assembly assembly = CSharpScript.CompileCode( code );

        if( assembly != null )
        {
            Type type = assembly.GetType( "CodeGenerated" );
            MethodInfo method = type.GetMethod( "Run" );
            method.Invoke( null, null );
        }
    }

    Saver s;
    Loader l;
    AsyncSaver _as;
    AsyncLoader _al;
    JsonPrefabAndDataStrategy strat;

    // Update is called once per frame
    void Start()
    {
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

        Action pause = () =>
        {
            Time.timeScale = 0;
        };
        Action unpause = () =>
        {
            Time.timeScale = 1;
        };

        JsonExplicitHierarchyStrategy stratExpl = new JsonExplicitHierarchyStrategy();
        stratExpl.IncludedObjectsMask = 1 << 0;

        //_as = new AsyncSaver( pause, unpause, new Func<ISaver, IEnumerator>[] { strat.SaveSceneObjects_Data }, new Func<ISaver, IEnumerator>[] { strat.SaveSceneObjects_Object } );
        _as = new AsyncSaver( pause, unpause, new Func<ISaver, IEnumerator>[] { stratExpl.SaveSceneObjects_Data }, new Func<ISaver, IEnumerator>[] { stratExpl.SaveSceneObjects_Object } );

        _al = new AsyncLoader( pause, unpause, new Func<ILoader, IEnumerator>[] { strat.LoadSceneObjects_Object }, new Func<ILoader, IEnumerator>[] { strat.LoadSceneObjects_Data } );
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