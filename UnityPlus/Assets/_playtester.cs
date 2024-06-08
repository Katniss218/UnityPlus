using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using UnityPlus.AssetManagement;
using UnityPlus.CSharp;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;
using UnityPlus.Serialization.Json;
using UnityPlus.Serialization.ReferenceMaps;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

public class _playtester : MonoBehaviour
{

    public class BaseClass
    {
        public float baseMember;
    }

    public class DerivedClass : BaseClass
    {
        public string derivedMember;
    }

    public class GenericClass<T>
    {
        public T member;
    }

    public class MultiGenericClass<T1, T2, T3>
    {
        public T1 member1;
        public T2 member2;
        public T3 member3;
    }

    [SerializationMappingProvider( typeof( BaseClass ) )]
    public static SerializationMapping BaseClassMapping()
    {
        return new MemberwiseSerializationMapping<BaseClass>()
            {
                ("base_member", new Member<BaseClass, float>( o => o.baseMember ))
            };
    }

    [SerializationMappingProvider( typeof( DerivedClass ) )]
    public static SerializationMapping DerivedClassMapping()
    {
        return new MemberwiseSerializationMapping<DerivedClass>()
            {
                ("derived_member", new Member<DerivedClass, string>( o => o.derivedMember ))
            }
        .IncludeMembers<BaseClass>();
    }


    [SerializationMappingProvider( typeof( _playtester ) )]
    public static SerializationMapping _playtesterMapping()
    {
        return new MemberwiseSerializationMapping<_playtester>()
        {
           // ("action", new Member<_playtester, Action<string>>(o => o.Action))
        }
        //.IncludeMembers<Behaviour>()
        .UseBaseTypeFactory();
    }

    public Action<string> Action { get; set; }

    private void Awake()
    {
        this.Action = DoSomething;
    }

    private void DoSomething( string s )
    {
        Debug.Log( s );
    }

    void Start()
    {
        // Arrange
        var initialValue = new KeyValuePair<BaseClass, BaseClass>( new BaseClass() { baseMember = 2 }, new DerivedClass() { baseMember = 5, derivedMember = "42" } );

        // Act
        var data = SerializationUnit.Serialize( initialValue );
        var finalValue = SerializationUnit.Deserialize<KeyValuePair<BaseClass, BaseClass>>( data );

        var su = SerializationUnit.FromObjects( this.gameObject );
        su.Serialize();
        SerializedData data2 = su.GetDataOfType<GameObject>().First();

        StringBuilder sb = new StringBuilder();
        new JsonStringWriter( data2, sb ).Write();
        string s = sb.ToString();
        //s = s.Replace( "true", "false" );

        data = new JsonStringReader( s ).Read();

        GameObject obj = SerializationUnit.Deserialize<GameObject>( data );

        _playtester t = obj.GetComponent<_playtester>();
    }
    
    void Update()
    {
        RunPerfTest();
    }

    private void RunPerfTest()
    {
        List<GameObject> list = new List<GameObject>( 100 );

        for( int i = 0; i < 1000; i++ )
        {
            Profiler.BeginSample( "t1" );
            SerializedData data = SerializationUnit.Serialize<GameObject>( this.gameObject );
            Profiler.EndSample();

            Profiler.BeginSample( "t2" );
            GameObject go = SerializationUnit.Deserialize<GameObject>( data );
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