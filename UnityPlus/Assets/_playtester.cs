using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;
using UnityPlus.Serialization.Patching;
using UnityPlus.Serialization.Patching.DSL;
using UnityPlus.Serialization.Patching.DSL.SyntaxTree;
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
    [SerializeField] GameObject startTestGo;

    public Action<string> Action { get; set; }

    private void Awake()
    {
        this.Action = DoSomething;
    }

    private void DoSomething( string s )
    {
        Debug.Log( s );
    }

    private void TestDataFixer()
    {
        /*
        
        (FOR any WHERE $"$type" == "some literal 123")
        {
            this = null;
        }

        */

        var mm = new DataFixerScript()
        {
            Statements = new IStatement[]
            {
                new Transformation()
                {
                    Headers = new ITransformationHeader[]
                    {
                        new ForTransformationHeader()
                        {
                            Target = new SerializedDataPath(SerializedDataPathSegment.Any())
                        },
                        new WhereTransformationHeader()
                        {
                            Filter = new BinaryExpression()
                            {
                                Left = new ValueOfExpression()
                                {
                                    Target = new SerializedDataPath(SerializedDataPathSegment.Named("$type"))
                                },
                                Right = new LiteralExpression()
                                {
                                    Value = (SerializedData)"some literal 123"
                                },
                                Op = new EqualOp()
                            }
                        }
                    },
                    Body = new IStatement[]
                    {
                        new AssignmentStatement()
                        {
                            Left = new SerializedDataPath(SerializedDataPathSegment.This()),
                            Right = new LiteralExpression()
                            {
                                Value = (SerializedData)null
                            }
                        }
                    }
                }
            }
        };

        var data = new SerializedArray()
        {
            new SerializedObject()
            {
                { "$type", "some literal 123" },
            },
            new SerializedObject()
            {
                { "$type", "othervalue" },
            },
        };

        mm.InvokeOn( data );
    }

    void Start()
    {
        TestDataFixer();


        //SerializedData data = SerializationUnit.Serialize<(int, string)>( (218, "stringval") );
        var su = SerializationUnit.FromObjectsAsync<GameObject>( startTestGo );
        do
        { // INFO - the time includes things like the JIT, so first serialization will take more steps, but that doesn't affect anything.
            su.Serialize();
        } while( !su.Result.HasFlag( SerializationResult.Finished ) );
        var data = su.GetData().First();

        // data["components"]["value"][1]["$type"] = "UnityEngine.MeshFilter1234, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        var sb = new StringBuilder();
        new JsonStringWriter( data, sb ).Write();
        Debug.Log( sb.ToString() );



        var su2 = SerializationUnit.FromDataAsync<GameObject>( data );
        do
        { // INFO - the time includes things like the JIT, so first serialization will take more steps, but that doesn't affect anything.
            su2.Deserialize();

        } while( !su2.Result.HasFlag( SerializationResult.Finished ) );
        GameObject obj = su2.GetObjects().First();



        su = SerializationUnit.FromObjectsAsync<GameObject>( startTestGo );
        do
        { // INFO - the time includes things like the JIT, so first serialization will take more steps, but that doesn't affect anything.
            su.Serialize();

        } while( !su.Result.HasFlag( SerializationResult.Finished ) );
        data = su.GetData().First();

        sb = new StringBuilder();
        new JsonStringWriter( data, sb ).Write();
        Debug.Log( sb.ToString() );
    }

    void Update()
    {
        // RunPerfTest();
        // RunPerfTestAsync_AsSync();
    }

    const int COUNT = 1000;

    private void RunPerfTest()
    {
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

    private void RunPerfTestAsync_AsSync()
    {
        List<GameObject> list = new List<GameObject>( COUNT );

        for( int i = 0; i < COUNT; i++ )
        {
            //int sCount = 0;

            Profiler.BeginSample( "t1" );
            var su = SerializationUnit.FromObjectsAsync<GameObject>( perfTestGo );
            do
            { // INFO - the time includes things like the JIT, so first serialization will take more steps, but that doesn't affect anything.
                su.Serialize();
                //sCount++;

            } while( !su.Result.HasFlag( SerializationResult.Finished ) );
            SerializedData data = su.GetData().First();

            //Debug.Log( sCount );

            Profiler.EndSample();

            //int lCount = 0;

            Profiler.BeginSample( "t2" );
            var su2 = SerializationUnit.FromDataAsync<GameObject>( data );
            do
            { // INFO - the time includes things like the JIT, so first serialization will take more steps, but that doesn't affect anything.
                su2.Deserialize();
                //lCount++;

            } while( !su2.Result.HasFlag( SerializationResult.Finished ) );
            GameObject go = su2.GetObjects().First();
            Profiler.EndSample();

            //Debug.Log( lCount );

            list.Add( go );
        }

        foreach( var go in list.ToArray() )
        {
            Destroy( go );
            list.Clear();
        }
    }
}