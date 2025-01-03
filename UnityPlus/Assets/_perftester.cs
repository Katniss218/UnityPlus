using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

public class _perftester : MonoBehaviour
{
    public struct Vector32
    {
        public double x, y, z;

        public Vector32( double x, double y, double z )
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public class C1
    {
        public string name;
        public string description;
        public int v;
        public int h;
        public int w;
        public int x;
        public int y;
        public int z;
        public Vector32 w2;
        public Vector32 x2;
        public Vector32 y2;
        public Vector32 z2;
        public C2 c;
    }

    public class C2
    {
        public int[] intArray;
    }

    [MapsInheritingFrom( typeof( C1 ) )]
    public static SerializationMapping C1Mapping()
    {
        return new MemberwiseSerializationMapping<C1>()
            .WithMember( "name", o => o.name )
            .WithMember( "description", o => o.description )
            .WithMember( "v", o => o.v )
            .WithMember( "h", o => o.h )
            .WithMember( "w", o => o.w )
            .WithMember( "x", o => o.x )
            .WithMember( "y", o => o.y )
            .WithMember( "z", o => o.z )
            .WithMember( "w2", o => o.w2 )
            .WithMember( "x2", o => o.x2 )
            .WithMember( "y2", o => o.y2 )
            .WithMember( "z2", o => o.z2 )
            .WithMember( "c", o => o.c );
    }

    [MapsInheritingFrom( typeof( C2 ) )]
    public static SerializationMapping C2Mapping()
    {
        return new MemberwiseSerializationMapping<C2>()
            .WithMember( "int_array", o => o.intArray );
    }

    [MapsInheritingFrom( typeof( Vector32 ) )]
    public static SerializationMapping Vector32Mapping()
    {
        return new PrimitiveSerializationMapping<Vector32>()
        {
            OnSave = ( o, s ) => new SerializedArray( 3 ) { (SerializedPrimitive)o.x, (SerializedPrimitive)o.y, (SerializedPrimitive)o.z },
            OnLoad = ( data, l ) => new Vector32( (double)data[0], (double)data[1], (double)data[2] )
        };
    }

    C1 obj = new C1()
    {
        name = "asadafafsafdsd",
        description = "fdfsgsfusjofsafdsafdsafdsadsafdsafdsasad\nf fbiss hbfsiahfnb fidsanfsadfbsdifbhsio hfnsod hodsa fbsaod fbosf a",
        v = -43342,
        h = 324,
        w = 324,
        x = 324,
        y = 324,
        z = 324,
        w2 = new Vector32( 42.23425f, -342.2345f, 2356532f ),
        x2 = new Vector32( 42.23425f, -342.2345f, 2356532f ),
        y2 = new Vector32( 42.23425f, -342.2345f, 2356532f ),
        z2 = new Vector32( 42.23425f, -342.2345f, 2356532f ),
        c = new C2()
        {
            intArray = new[] { 0, 1, 2, 5, 6, 74, 4353, 34, 2, 3, 4, 6, 3, 3, 6, 8, 6, 3, 2, 3, 45, 7, 9, 7, 23, 43, 65, 8765, 875, 3, 2323, 45746, 765, 765, 435, 234, 65, 98, 876, 453 }
        },
    };

    void Update()
    {
        RunPerfTest();
    }

    private void RunPerfTest()
    {
        const int COUNT = 100;

        Profiler.BeginSample( "custom" );
        for( int i = 0; i < COUNT; i++ )
        {
            SerializedData data = SerializationUnit.Serialize<C1>( obj );

            StringBuilder sb = new StringBuilder();
            new JsonStringWriter( data, sb ).Write();
            string s = sb.ToString();
            data = new JsonStringReader( s ).Read();

            C1 go = SerializationUnit.Deserialize<C1>( data );
        }
        Profiler.EndSample();

        //

        Profiler.BeginSample( "newtonsoft" );
        for( int i = 0; i < COUNT; i++ )
        {
            JToken sData = JToken.FromObject( obj );

            string s2 = sData.ToString();
            sData = JToken.Parse( s2 );

            C1 go2 = sData.ToObject<C1>();
        }
        Profiler.EndSample();

        //

        Profiler.BeginSample( "newtonsoftdirect" );
        for( int i = 0; i < COUNT; i++ )
        {
            string sJson = JsonConvert.SerializeObject( obj );
            C1 go3 = JsonConvert.DeserializeObject<C1>( sJson );
        }
        Profiler.EndSample();


        Profiler.BeginSample( "custom-nostring" );
        for( int i = 0; i < COUNT; i++ )
        {
            SerializedData data = SerializationUnit.Serialize<C1>( obj );

            C1 go = SerializationUnit.Deserialize<C1>( data );
        }
        Profiler.EndSample();

        Profiler.BeginSample( "newtonsoft-nostring" );
        for( int i = 0; i < COUNT; i++ )
        {
            JToken sData = JToken.FromObject( obj );

            C1 go2 = sData.ToObject<C1>();
        }
        Profiler.EndSample();
    }
}
