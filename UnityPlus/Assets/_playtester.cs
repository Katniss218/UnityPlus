using Newtonsoft.Json;
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
        {
            ("perf_test_go", new Member<_playtester, GameObject>( ObjectContext.Ref, o => o.perfTestGo )),
            ("action", new Member<_playtester, Action<string>>( o => o.Action ))
        };
    }
    
    [MapsInheritingFrom( typeof( FPSCounterDebug ) )]
    public static SerializationMapping FPSCounterDebugMapping()
    {
        return new MemberwiseSerializationMapping<FPSCounterDebug>()
        {
            ("fps", new Member<FPSCounterDebug, float>( o => o.fps ))
        };
    }

    [MapsInheritingFrom( typeof( BaseClass ) )]
    public static SerializationMapping BaseClassMapping()
    {
        return new MemberwiseSerializationMapping<BaseClass>()
        {
            ("base_member", new Member<BaseClass, float>( o => o.baseMember ))
        };
    }

    [MapsInheritingFrom( typeof( DerivedClass ) )]
    public static SerializationMapping DerivedClassMapping()
    {
        return new MemberwiseSerializationMapping<DerivedClass>()
        {
            ("derived_member", new Member<DerivedClass, string>( o => o.derivedMember ))
        };
    }
    
    [MapsInheritingFrom( typeof( MoreDerivedClass ) )]
    public static SerializationMapping MoreDerivedClassMapping()
    {
        return new MemberwiseSerializationMapping<MoreDerivedClass>()
        {
            ("more_derived_member", new Member<MoreDerivedClass, string>( o => o.moreDerivedMember ))
        };
    }

    [MapsImplementing( typeof( IAnInterface ) )]
    public static SerializationMapping IAnInterfaceMapping()
    {
        return new MemberwiseSerializationMapping<IAnInterface>()
        {
            ("interface_member", new Member<IAnInterface, float>( o => o.interfaceMember ))
        };
    }

    [MapsInheritingFrom( typeof( ReferencingClass ) )]
    public static SerializationMapping ReferencingClassMapping()
    {
        return new MemberwiseSerializationMapping<ReferencingClass>()
        {
            ("ref_member", new Member<ReferencingClass, BaseClass>( ObjectContext.Ref, o => o.refMember ))
        };
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
        // Arrange
        IEnumerable<OverridableEventListener<string>> events = new List<OverridableEventListener<string>>()
            {
                new OverridableEventListener<string>( "A", null, new[] { "B" }, null, null ),
                new OverridableEventListener<string>( "B", null, new[] { "A" }, null, null ),
                new OverridableEventListener<string>( "C", null, null, new[] { "B" }, null ),
                new OverridableEventListener<string>( "D", null, null, new[] { "C" }, null ),
            };
        bool wasCircular = false;

        // Act
        var sortedEvents = events
            .SortDependencies( out wasCircular )
            .Select( l => l.ID );
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