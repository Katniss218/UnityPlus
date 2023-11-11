using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    JsonAssetGameObjectsStrategy strat;

    // Update is called once per frame
    void Start()
    {
    }

    void Update()
    {
    }
}