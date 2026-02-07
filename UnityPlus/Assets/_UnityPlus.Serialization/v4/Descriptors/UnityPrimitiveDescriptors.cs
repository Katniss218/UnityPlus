
using System;
using UnityEngine;

namespace UnityPlus.Serialization
{
    // --- VECTORS ---

    public class Vector2Descriptor : PrimitiveDescriptor<Vector2>
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            var v = (Vector2)target;
            data = new SerializedArray { (SerializedPrimitive)v.x, (SerializedPrimitive)v.y };
        }

        public override DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = default( Vector2 );
            if( data is SerializedArray arr && arr.Count >= 2 )
            {
                result = new Vector2( (float)arr[0], (float)arr[1] );
                return DeserializeResult.Success;
            }
            return DeserializeResult.Failed;
        }
    }

    public class Vector3Descriptor : PrimitiveDescriptor<Vector3>
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            var v = (Vector3)target;
            data = new SerializedArray { (SerializedPrimitive)v.x, (SerializedPrimitive)v.y, (SerializedPrimitive)v.z };
        }

        public override DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = default( Vector3 );
            if( data is SerializedArray arr && arr.Count >= 3 )
            {
                result = new Vector3( (float)arr[0], (float)arr[1], (float)arr[2] );
                return DeserializeResult.Success;
            }
            return DeserializeResult.Failed;
        }
    }

    public class Vector4Descriptor : PrimitiveDescriptor<Vector4>
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            var v = (Vector4)target;
            data = new SerializedArray { (SerializedPrimitive)v.x, (SerializedPrimitive)v.y, (SerializedPrimitive)v.z, (SerializedPrimitive)v.w };
        }

        public override DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = default( Vector4 );
            if( data is SerializedArray arr && arr.Count >= 4 )
            {
                result = new Vector4( (float)arr[0], (float)arr[1], (float)arr[2], (float)arr[3] );
                return DeserializeResult.Success;
            }
            return DeserializeResult.Failed;
        }
    }

    public class Vector2IntDescriptor : PrimitiveDescriptor<Vector2Int>
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            var v = (Vector2Int)target;
            data = new SerializedArray { (SerializedPrimitive)v.x, (SerializedPrimitive)v.y };
        }

        public override DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = default( Vector2Int );
            if( data is SerializedArray arr && arr.Count >= 2 )
            {
                result = new Vector2Int( (int)arr[0], (int)arr[1] );
                return DeserializeResult.Success;
            }
            return DeserializeResult.Failed;
        }
    }

    public class Vector3IntDescriptor : PrimitiveDescriptor<Vector3Int>
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            var v = (Vector3Int)target;
            data = new SerializedArray { (SerializedPrimitive)v.x, (SerializedPrimitive)v.y, (SerializedPrimitive)v.z };
        }

        public override DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = default( Vector3Int );
            if( data is SerializedArray arr && arr.Count >= 3 )
            {
                result = new Vector3Int( (int)arr[0], (int)arr[1], (int)arr[2] );
                return DeserializeResult.Success;
            }
            return DeserializeResult.Failed;
        }
    }

    // --- QUATERNION ---

    public class QuaternionDescriptor : PrimitiveDescriptor<Quaternion>
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            var q = (Quaternion)target;
            data = new SerializedArray { (SerializedPrimitive)q.x, (SerializedPrimitive)q.y, (SerializedPrimitive)q.z, (SerializedPrimitive)q.w };
        }

        public override DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = default( Quaternion );
            if( data is SerializedArray arr && arr.Count >= 4 )
            {
                result = new Quaternion( (float)arr[0], (float)arr[1], (float)arr[2], (float)arr[3] );
                return DeserializeResult.Success;
            }
            return DeserializeResult.Failed;
        }
    }

    // --- MATRIX ---

    public class Matrix4x4Descriptor : PrimitiveDescriptor<Matrix4x4>
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            var m = (Matrix4x4)target;
            var arr = new SerializedArray( 16 );
            for( int i = 0; i < 16; i++ ) arr.Add( (SerializedPrimitive)m[i] );
            data = arr;
        }

        public override DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = Matrix4x4.identity;
            if( data is SerializedArray arr && arr.Count >= 16 )
            {
                var m = new Matrix4x4();
                for( int i = 0; i < 16; i++ ) m[i] = (float)arr[i];
                result = m;
                return DeserializeResult.Success;
            }
            return DeserializeResult.Failed;
        }
    }

    // --- COLORS ---

    public class ColorDescriptor : PrimitiveDescriptor<Color>
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            var c = (Color)target;
            data = new SerializedObject
            {
                ["r"] = (SerializedPrimitive)c.r,
                ["g"] = (SerializedPrimitive)c.g,
                ["b"] = (SerializedPrimitive)c.b,
                ["a"] = (SerializedPrimitive)c.a
            };
        }

        public override DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = default( Color );
            if( data is SerializedObject obj )
            {
                result = new Color(
                    obj.TryGetValue( "r", out var r ) ? (float)r : 0f,
                    obj.TryGetValue( "g", out var g ) ? (float)g : 0f,
                    obj.TryGetValue( "b", out var b ) ? (float)b : 0f,
                    obj.TryGetValue( "a", out var a ) ? (float)a : 1f
                );
                return DeserializeResult.Success;
            }
            return DeserializeResult.Failed;
        }
    }

    public class Color32Descriptor : PrimitiveDescriptor<Color32>
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            var c = (Color32)target;
            data = new SerializedObject
            {
                ["r"] = (SerializedPrimitive)c.r,
                ["g"] = (SerializedPrimitive)c.g,
                ["b"] = (SerializedPrimitive)c.b,
                ["a"] = (SerializedPrimitive)c.a
            };
        }

        public override DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = default( Color32 );
            if( data is SerializedObject obj )
            {
                result = new Color32(
                    obj.TryGetValue( "r", out var r ) ? (byte)r : (byte)0,
                    obj.TryGetValue( "g", out var g ) ? (byte)g : (byte)0,
                    obj.TryGetValue( "b", out var b ) ? (byte)b : (byte)0,
                    obj.TryGetValue( "a", out var a ) ? (byte)a : (byte)255
                );
                return DeserializeResult.Success;
            }
            return DeserializeResult.Failed;
        }
    }

    // --- RECT ---

    public class RectDescriptor : PrimitiveDescriptor<Rect>
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            var r = (Rect)target;
            data = new SerializedObject
            {
                ["x"] = (SerializedPrimitive)r.x,
                ["y"] = (SerializedPrimitive)r.y,
                ["width"] = (SerializedPrimitive)r.width,
                ["height"] = (SerializedPrimitive)r.height
            };
        }

        public override DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = default( Rect );
            if( data is SerializedObject obj )
            {
                result = new Rect(
                    obj.TryGetValue( "x", out var x ) ? (float)x : 0f,
                    obj.TryGetValue( "y", out var y ) ? (float)y : 0f,
                    obj.TryGetValue( "width", out var w ) ? (float)w : 0f,
                    obj.TryGetValue( "height", out var h ) ? (float)h : 0f
                );
                return DeserializeResult.Success;
            }
            return DeserializeResult.Failed;
        }
    }

    public class RectIntDescriptor : PrimitiveDescriptor<RectInt>
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            var r = (RectInt)target;
            data = new SerializedObject
            {
                ["x"] = (SerializedPrimitive)r.x,
                ["y"] = (SerializedPrimitive)r.y,
                ["width"] = (SerializedPrimitive)r.width,
                ["height"] = (SerializedPrimitive)r.height
            };
        }

        public override DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = default( RectInt );
            if( data is SerializedObject obj )
            {
                result = new RectInt(
                    obj.TryGetValue( "x", out var x ) ? (int)x : 0,
                    obj.TryGetValue( "y", out var y ) ? (int)y : 0,
                    obj.TryGetValue( "width", out var w ) ? (int)w : 0,
                    obj.TryGetValue( "height", out var h ) ? (int)h : 0
                );
                return DeserializeResult.Success;
            }
            return DeserializeResult.Failed;
        }
    }

    // --- BOUNDS ---

    public class BoundsDescriptor : PrimitiveDescriptor<Bounds>
    {
        private static readonly Vector3Descriptor _v3Desc = new Vector3Descriptor();

        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            var b = (Bounds)target;

            SerializedData center = null;
            _v3Desc.SerializeDirect( b.center, ref center, ctx );

            SerializedData extents = null;
            _v3Desc.SerializeDirect( b.extents, ref extents, ctx );

            data = new SerializedObject
            {
                ["center"] = center,
                ["extents"] = extents
            };
        }

        public override DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = default( Bounds );
            if( data is SerializedObject obj )
            {
                _v3Desc.DeserializeDirect( obj.TryGetValue( "center", out var cData ) ? cData : null, ctx, out var center );
                _v3Desc.DeserializeDirect( obj.TryGetValue( "extents", out var eData ) ? eData : null, ctx, out var extents );

                result = new Bounds( (Vector3)center, (Vector3)extents * 2f );
                return DeserializeResult.Success;
            }
            return DeserializeResult.Failed;
        }
    }

    public class BoundsIntDescriptor : PrimitiveDescriptor<BoundsInt>
    {
        private static readonly Vector3IntDescriptor _v3IntDesc = new Vector3IntDescriptor();

        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            var b = (BoundsInt)target;

            SerializedData position = null;
            _v3IntDesc.SerializeDirect( b.position, ref position, ctx );

            SerializedData size = null;
            _v3IntDesc.SerializeDirect( b.size, ref size, ctx );

            data = new SerializedObject
            {
                ["position"] = position,
                ["size"] = size
            };
        }

        public override DeserializeResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = default( BoundsInt );
            if( data is SerializedObject obj )
            {
                _v3IntDesc.DeserializeDirect( obj.TryGetValue( "position", out var pData ) ? pData : null, ctx, out var position );
                _v3IntDesc.DeserializeDirect( obj.TryGetValue( "size", out var sData ) ? sData : null, ctx, out var size );

                result = new BoundsInt( (Vector3Int)position, (Vector3Int)size );
                return DeserializeResult.Success;
            }
            return DeserializeResult.Failed;
        }
    }
}
