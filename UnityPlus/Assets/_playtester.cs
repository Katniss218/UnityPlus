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
using UnityPlus.Serialization.ReferenceMaps;
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

    public class TestClass : IAutoPersistsData
    {
        [Persist( "field_1", PersistMode.Data )]
        public int field1;

        [Persist( "property_1", PersistMode.Data )]
        public string Property1 { get; set; }
    }

    Saver s;
    Loader l;
    AsyncSaver _as;
    AsyncLoader _al;
    AssetGameObjectsStrategy strat;

    // Update is called once per frame
    void Start()
    {
        string s = "aa";
        string a = s;
        s = "aasdaa";

        var refMap = new BidirectionalReferenceStore();
        TestClass sut = new TestClass()
        {
            field1 = 5,
            Property1 = "hello"
        };

        // Act
        var data = sut.GetData( refMap );

        // Assert
        //Assert.That( data["field_1"], Is.EqualTo( (SerializedPrimitive)"hello" ) );
    }

    void Update()
    {
    }
}