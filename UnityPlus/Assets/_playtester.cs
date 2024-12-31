using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using UnityPlus;
using UnityPlus.AssetManagement;
using UnityPlus.CSharp;
using UnityPlus.OverridableEvents;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;
using UnityPlus.Serialization.Json;
using UnityPlus.Serialization.ReferenceMaps;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

public class _playtester : MonoBehaviour
{
    public interface IAnInterface
    {
        float interfaceMember { get; set; }
    }

    public class InterfaceClass : IAnInterface
    {
        public float interfaceMember { get; set; }
    }

    public class BaseClass
    {
        public float baseMember;
    }

    public class DerivedClass : BaseClass
    {
        public string derivedMember;
    }

    public class MoreDerivedClass : DerivedClass
    {
        public string moreDerivedMember;
    }

    public class ReferencingClass
    {
        public BaseClass refMember;
        public IAnInterface interfaceRefMember;
    }

    [MapsInheritingFrom( typeof( _playtester ) )]
    public static SerializationMapping _playtesterMapping()
    {
        return new MemberwiseSerializationMapping<_playtester>()
            .WithMember( "perf_test_go", ObjectContext.Ref, o => o.perfTestGo )
            .WithMember( "action", o => o.Action );
    }

    [MapsInheritingFrom( typeof( FPSCounterDebug ) )]
    public static SerializationMapping FPSCounterDebugMapping()
    {
        return new MemberwiseSerializationMapping<FPSCounterDebug>()
            .WithMember( "fps", o => o.fps );
    }

    [MapsInheritingFrom( typeof( BaseClass ) )]
    public static SerializationMapping BaseClassMapping()
    {
        return new MemberwiseSerializationMapping<BaseClass>()
            .WithMember( "base_member", o => o.baseMember );
    }

    [MapsInheritingFrom( typeof( DerivedClass ) )]
    public static SerializationMapping DerivedClassMapping()
    {
        return new MemberwiseSerializationMapping<DerivedClass>()
            .WithMember( "derived_member", o => o.derivedMember );
    }

    [MapsInheritingFrom( typeof( MoreDerivedClass ) )]
    public static SerializationMapping MoreDerivedClassMapping()
    {
        return new MemberwiseSerializationMapping<MoreDerivedClass>()
            .WithMember( "more_derived_member", o => o.moreDerivedMember );
    }

    [MapsImplementing( typeof( IAnInterface ) )]
    public static SerializationMapping IAnInterfaceMapping()
    {
        return new MemberwiseSerializationMapping<IAnInterface>()
            .WithMember( "interface_member", o => o.interfaceMember );
    }

    [MapsInheritingFrom( typeof( ReferencingClass ) )]
    public static SerializationMapping ReferencingClassMapping()
    {
        return new MemberwiseSerializationMapping<ReferencingClass>()
            .WithMember( "ref_member", ObjectContext.Ref, o => o.refMember );
    }

    [SerializeField] GameObject perfTestGo;

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
        //SerializedData data = SerializationUnit.Serialize<(int, string)>( (218, "stringval") );
        var su = SerializationUnit.FromObjectsAsync<GameObject>( perfTestGo );
        do
        { // INFO - the time includes things like the JIT, so first serialization will take more steps, but that doesn't affect anything.
            su.Serialize();

        } while( su.Result == MappingResult.Progressed );
        var data = su.GetData().First();
        //SerializedData data = SerializationUnit.Serialize<GameObject>( perfTestGo );
        //SerializedData data = SerializationUnit.Serialize<int[]>( new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 } );

        var sb = new StringBuilder();
        new JsonStringWriter( data, sb ).Write();
        Debug.Log( sb.ToString() );

        //(int, string) var = SerializationUnit.Deserialize<(int, string)>( data );
        //GameObject var = SerializationUnit.Deserialize<GameObject>( data );
        var su2 = SerializationUnit.FromDataAsync<GameObject>( data );
        do
        { // INFO - the time includes things like the JIT, so first serialization will take more steps, but that doesn't affect anything.
            su2.Deserialize();

        } while( su2.Result == MappingResult.Progressed );
        GameObject var = su2.GetObjects<GameObject>().First();
        //int[] var = SerializationUnit.Deserialize<int[]>( data );

        //data = SerializationUnit.Serialize<(int, string)>( var );
        data = SerializationUnit.Serialize<GameObject>( var );
        //data = SerializationUnit.Serialize<int[]>( var );

        sb = new StringBuilder();
        new JsonStringWriter( data, sb ).Write();
        Debug.Log( sb.ToString() );

    }

    void Update()
    {
        RunPerfTest();
    }

    private void RunPerfTest()
    {
        const int COUNT = 1000;

        List<GameObject> list = new List<GameObject>( COUNT );

        for( int i = 0; i < COUNT; i++ )
        {
            Profiler.BeginSample( "t1" );
            SerializedData data = SerializationUnit.Serialize<GameObject>( perfTestGo );
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