
using System;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class UnityPrimitiveDescriptors
    {
        // --- VECTORS ---

        [MapsInheritingFrom( typeof( Vector2 ) )]
        public static IDescriptor Vector2Descriptor() => new PrimitiveConfigurableDescriptor<Vector2>(
            ( target, wrapper, ctx ) =>
                wrapper.Data = new SerializedArray { (SerializedPrimitive)target.x, (SerializedPrimitive)target.y },
            ( data, ctx ) =>
                (data is SerializedArray arr && arr.Count >= 2) ? new Vector2( (float)arr[0], (float)arr[1] ) : default
        );

        [MapsInheritingFrom( typeof( Vector3 ) )]
        public static IDescriptor Vector3Descriptor() => new PrimitiveConfigurableDescriptor<Vector3>(
            ( target, wrapper, ctx ) =>
                wrapper.Data = new SerializedArray { (SerializedPrimitive)target.x, (SerializedPrimitive)target.y, (SerializedPrimitive)target.z },
            ( data, ctx ) =>
                (data is SerializedArray arr && arr.Count >= 3) ? new Vector3( (float)arr[0], (float)arr[1], (float)arr[2] ) : default
        );

        [MapsInheritingFrom( typeof( Vector4 ) )]
        public static IDescriptor Vector4Descriptor() => new PrimitiveConfigurableDescriptor<Vector4>(
            ( target, wrapper, ctx ) =>
                wrapper.Data = new SerializedArray { (SerializedPrimitive)target.x, (SerializedPrimitive)target.y, (SerializedPrimitive)target.z, (SerializedPrimitive)target.w },
            ( data, ctx ) =>
                (data is SerializedArray arr && arr.Count >= 4) ? new Vector4( (float)arr[0], (float)arr[1], (float)arr[2], (float)arr[3] ) : default
        );

        [MapsInheritingFrom( typeof( Vector2Int ) )]
        public static IDescriptor Vector2IntDescriptor() => new PrimitiveConfigurableDescriptor<Vector2Int>(
            ( target, wrapper, ctx ) =>
                wrapper.Data = new SerializedArray { (SerializedPrimitive)target.x, (SerializedPrimitive)target.y },
            ( data, ctx ) =>
                (data is SerializedArray arr && arr.Count >= 2) ? new Vector2Int( (int)arr[0], (int)arr[1] ) : default
        );

        [MapsInheritingFrom( typeof( Vector3Int ) )]
        public static IDescriptor Vector3IntDescriptor() => new PrimitiveConfigurableDescriptor<Vector3Int>(
            ( target, wrapper, ctx ) =>
                wrapper.Data = new SerializedArray { (SerializedPrimitive)target.x, (SerializedPrimitive)target.y, (SerializedPrimitive)target.z },
            ( data, ctx ) =>
                (data is SerializedArray arr && arr.Count >= 3) ? new Vector3Int( (int)arr[0], (int)arr[1], (int)arr[2] ) : default
        );

        // --- QUATERNION ---

        [MapsInheritingFrom( typeof( Quaternion ) )]
        public static IDescriptor QuaternionDescriptor() => new PrimitiveConfigurableDescriptor<Quaternion>(
            ( target, wrapper, ctx ) =>
                wrapper.Data = new SerializedArray { (SerializedPrimitive)target.x, (SerializedPrimitive)target.y, (SerializedPrimitive)target.z, (SerializedPrimitive)target.w },
            ( data, ctx ) =>
                (data is SerializedArray arr && arr.Count >= 4) ? new Quaternion( (float)arr[0], (float)arr[1], (float)arr[2], (float)arr[3] ) : default
        );

        // --- MATRIX ---

        [MapsInheritingFrom( typeof( Matrix4x4 ) )]
        public static IDescriptor Matrix4x4Descriptor() => new PrimitiveConfigurableDescriptor<Matrix4x4>(
            ( target, wrapper, ctx ) => {
                var arr = new SerializedArray( 16 );
                for( int i = 0; i < 16; i++ ) arr.Add( (SerializedPrimitive)target[i] );
                wrapper.Data = arr;
            },
            ( data, ctx ) => {
                if( data is SerializedArray arr && arr.Count >= 16 )
                {
                    var m = new Matrix4x4();
                    for( int i = 0; i < 16; i++ ) m[i] = (float)arr[i];
                    return m;
                }
                return Matrix4x4.identity;
            }
        );

        // --- COLORS ---

        [MapsInheritingFrom( typeof( Color ) )]
        public static IDescriptor ColorDescriptor() => new PrimitiveConfigurableDescriptor<Color>(
            ( c, wrapper, ctx ) =>
                wrapper.Data = new SerializedObject
                {
                    ["r"] = (SerializedPrimitive)c.r,
                    ["g"] = (SerializedPrimitive)c.g,
                    ["b"] = (SerializedPrimitive)c.b,
                    ["a"] = (SerializedPrimitive)c.a
                },
            ( data, ctx ) =>
                (data is SerializedObject obj) ? new Color(
                    obj.TryGetValue( "r", out var r ) ? (float)r : 0f,
                    obj.TryGetValue( "g", out var g ) ? (float)g : 0f,
                    obj.TryGetValue( "b", out var b ) ? (float)b : 0f,
                    obj.TryGetValue( "a", out var a ) ? (float)a : 1f
                ) : default
        );

        [MapsInheritingFrom( typeof( Color32 ) )]
        public static IDescriptor Color32Descriptor() => new PrimitiveConfigurableDescriptor<Color32>(
            ( c, wrapper, ctx ) =>
                wrapper.Data = new SerializedObject
                {
                    ["r"] = (SerializedPrimitive)c.r,
                    ["g"] = (SerializedPrimitive)c.g,
                    ["b"] = (SerializedPrimitive)c.b,
                    ["a"] = (SerializedPrimitive)c.a
                },
            ( data, ctx ) =>
                (data is SerializedObject obj) ? new Color32(
                    obj.TryGetValue( "r", out var r ) ? (byte)r : (byte)0,
                    obj.TryGetValue( "g", out var g ) ? (byte)g : (byte)0,
                    obj.TryGetValue( "b", out var b ) ? (byte)b : (byte)0,
                    obj.TryGetValue( "a", out var a ) ? (byte)a : (byte)255
                ) : default
        );

        // --- RECT & BOUNDS ---

        [MapsInheritingFrom( typeof( Rect ) )]
        public static IDescriptor RectDescriptor() => new PrimitiveConfigurableDescriptor<Rect>(
            ( r, wrapper, ctx ) =>
                wrapper.Data = new SerializedObject
                {
                    ["x"] = (SerializedPrimitive)r.x,
                    ["y"] = (SerializedPrimitive)r.y,
                    ["width"] = (SerializedPrimitive)r.width,
                    ["height"] = (SerializedPrimitive)r.height
                },
            ( data, ctx ) =>
                (data is SerializedObject obj) ? new Rect(
                    obj.TryGetValue( "x", out var x ) ? (float)x : 0f,
                    obj.TryGetValue( "y", out var y ) ? (float)y : 0f,
                    obj.TryGetValue( "width", out var w ) ? (float)w : 0f,
                    obj.TryGetValue( "height", out var h ) ? (float)h : 0f
                ) : default
        );

        [MapsInheritingFrom( typeof( RectInt ) )]
        public static IDescriptor RectIntDescriptor() => new PrimitiveConfigurableDescriptor<RectInt>(
            ( r, wrapper, ctx ) =>
                wrapper.Data = new SerializedObject
                {
                    ["x"] = (SerializedPrimitive)r.x,
                    ["y"] = (SerializedPrimitive)r.y,
                    ["width"] = (SerializedPrimitive)r.width,
                    ["height"] = (SerializedPrimitive)r.height
                },
            ( data, ctx ) =>
                (data is SerializedObject obj) ? new RectInt(
                    obj.TryGetValue( "x", out var x ) ? (int)x : 0,
                    obj.TryGetValue( "y", out var y ) ? (int)y : 0,
                    obj.TryGetValue( "width", out var w ) ? (int)w : 0,
                    obj.TryGetValue( "height", out var h ) ? (int)h : 0
                ) : default
        );

        // --- BOUNDS ---

        [MapsInheritingFrom( typeof( Bounds ) )]
        public static IDescriptor BoundsDescriptor() => new PrimitiveConfigurableDescriptor<Bounds>(
            ( b, wrapper, ctx ) => {
                var centerDesc = TypeDescriptorRegistry.GetDescriptor( typeof( Vector3 ) ) as IPrimitiveDescriptor;
                var extentsDesc = TypeDescriptorRegistry.GetDescriptor( typeof( Vector3 ) ) as IPrimitiveDescriptor;
                SerializedData c = null, e = null;
                centerDesc?.SerializeDirect( b.center, ref c, ctx );
                extentsDesc?.SerializeDirect( b.extents, ref e, ctx );
                wrapper.Data = new SerializedObject { ["center"] = c, ["extents"] = e };
            },
            ( data, ctx ) => {
                if( data is SerializedObject obj )
                {
                    var v3Desc = TypeDescriptorRegistry.GetDescriptor( typeof( Vector3 ) ) as IPrimitiveDescriptor;
                    object c = default( Vector3 ), e = default( Vector3 );
                    if( obj.TryGetValue( "center", out var cData ) ) v3Desc?.DeserializeDirect( cData, ctx, out c );
                    if( obj.TryGetValue( "extents", out var eData ) ) v3Desc?.DeserializeDirect( eData, ctx, out e );
                    return new Bounds( (Vector3)c, (Vector3)e * 2f );
                }
                return default;
            }
        );

        [MapsInheritingFrom( typeof( BoundsInt ) )]
        public static IDescriptor BoundsIntDescriptor() => new PrimitiveConfigurableDescriptor<BoundsInt>(
            ( b, wrapper, ctx ) => {
                var v3Desc = TypeDescriptorRegistry.GetDescriptor( typeof( Vector3Int ) ) as IPrimitiveDescriptor;
                SerializedData p = null, s = null;
                v3Desc?.SerializeDirect( b.position, ref p, ctx );
                v3Desc?.SerializeDirect( b.size, ref s, ctx );
                wrapper.Data = new SerializedObject { ["position"] = p, ["size"] = s };
            },
            ( data, ctx ) => {
                if( data is SerializedObject obj )
                {
                    var v3Desc = TypeDescriptorRegistry.GetDescriptor( typeof( Vector3Int ) ) as IPrimitiveDescriptor;
                    object p = default( Vector3Int ), s = default( Vector3Int );
                    if( obj.TryGetValue( "position", out var pData ) ) v3Desc?.DeserializeDirect( pData, ctx, out p );
                    if( obj.TryGetValue( "size", out var sData ) ) v3Desc?.DeserializeDirect( sData, ctx, out s );
                    return new BoundsInt( (Vector3Int)p, (Vector3Int)s );
                }
                return default;
            }
        );
    }
}