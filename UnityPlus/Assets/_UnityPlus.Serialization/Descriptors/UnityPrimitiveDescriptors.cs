using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class UnityPrimitiveDescriptors
    {
        [MapsAnyInterface( ContextType = typeof( Ctx.Asset ) )]
        [MapsInheritingFrom( typeof( object ), ContextType = typeof( Ctx.Asset ) )]
        private static IDescriptor ProvideAsset<T>() where T : class
        {
            return new AssetDescriptor<T>();
        }
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
            ( o, wrapper, ctx ) =>
            {
                wrapper.Data = new SerializedArray( 16 )
                {
                    (SerializedPrimitive)o.m00, (SerializedPrimitive)o.m01, (SerializedPrimitive)o.m02, (SerializedPrimitive)o.m03,
                    (SerializedPrimitive)o.m10, (SerializedPrimitive)o.m11, (SerializedPrimitive)o.m12, (SerializedPrimitive)o.m13,
                    (SerializedPrimitive)o.m20, (SerializedPrimitive)o.m21, (SerializedPrimitive)o.m22, (SerializedPrimitive)o.m23,
                    (SerializedPrimitive)o.m30, (SerializedPrimitive)o.m31, (SerializedPrimitive)o.m32, (SerializedPrimitive)o.m33
                };
            },
            ( data, ctx ) => new Matrix4x4()
            {
                m00 = (float)data[0],
                m01 = (float)data[1],
                m02 = (float)data[2],
                m03 = (float)data[3],
                m10 = (float)data[4],
                m11 = (float)data[5],
                m12 = (float)data[6],
                m13 = (float)data[7],
                m20 = (float)data[8],
                m21 = (float)data[9],
                m22 = (float)data[10],
                m23 = (float)data[11],
                m30 = (float)data[12],
                m31 = (float)data[13],
                m32 = (float)data[14],
                m33 = (float)data[15]
            }
        );

        // --- COLORS ---

        [MapsInheritingFrom( typeof( Color ) )]
        public static IDescriptor ColorDescriptor() => new ClassOrStructDescriptor<Color>()
            .WithMember( "r", c => c.r, ( ref Color c, float v ) => c.r = v )
            .WithMember( "g", c => c.g, ( ref Color c, float v ) => c.g = v )
            .WithMember( "b", c => c.b, ( ref Color c, float v ) => c.b = v )
            .WithMember( "a", c => c.a, ( ref Color c, float v ) => c.a = v );

        [MapsInheritingFrom( typeof( Color32 ) )]
        public static IDescriptor Color32Descriptor() => new ClassOrStructDescriptor<Color32>()
            .WithMember( "r", c => c.r, ( ref Color32 c, byte v ) => c.r = v )
            .WithMember( "g", c => c.g, ( ref Color32 c, byte v ) => c.g = v )
            .WithMember( "b", c => c.b, ( ref Color32 c, byte v ) => c.b = v )
            .WithMember( "a", c => c.a, ( ref Color32 c, byte v ) => c.a = v );

        // --- RECT & BOUNDS ---

        [MapsInheritingFrom( typeof( Rect ) )]
        public static IDescriptor RectDescriptor() => new ClassOrStructDescriptor<Rect>()
            .WithMember( "x", r => r.x, ( ref Rect r, float v ) => r.x = v )
            .WithMember( "y", r => r.y, ( ref Rect r, float v ) => r.y = v )
            .WithMember( "width", r => r.width, ( ref Rect r, float v ) => r.width = v )
            .WithMember( "height", r => r.height, ( ref Rect r, float v ) => r.height = v );

        [MapsInheritingFrom( typeof( RectInt ) )]
        public static IDescriptor RectIntDescriptor() => new ClassOrStructDescriptor<RectInt>()
            .WithMember( "x", r => r.x, ( ref RectInt r, int v ) => r.x = v )
            .WithMember( "y", r => r.y, ( ref RectInt r, int v ) => r.y = v )
            .WithMember( "width", r => r.width, ( ref RectInt r, int v ) => r.width = v )
            .WithMember( "height", r => r.height, ( ref RectInt r, int v ) => r.height = v );

        // --- BOUNDS ---

        [MapsInheritingFrom( typeof( Bounds ) )]
        public static IDescriptor BoundsDescriptor() => new ClassOrStructDescriptor<Bounds>()
            .WithMember( "center", b => b.center, ( ref Bounds b, Vector3 v ) => b.center = v )
            .WithMember( "extents", b => b.extents, ( ref Bounds b, Vector3 v ) => b.extents = v );

        [MapsInheritingFrom( typeof( BoundsInt ) )]
        public static IDescriptor BoundsIntDescriptor() => new ClassOrStructDescriptor<BoundsInt>()
            .WithMember( "position", b => b.position, ( ref BoundsInt b, Vector3Int v ) => b.position = v )
            .WithMember( "size", b => b.size, ( ref BoundsInt b, Vector3Int v ) => b.size = v );

        // --- RAYS & PLANES ---

        [MapsInheritingFrom( typeof( Ray ) )]
        public static IDescriptor RayDescriptor() => new ClassOrStructDescriptor<Ray>()
            .WithMember( "origin", r => r.origin, ( ref Ray r, Vector3 v ) => r.origin = v )
            .WithMember( "direction", r => r.direction, ( ref Ray r, Vector3 v ) => r.direction = v );

        [MapsInheritingFrom( typeof( Ray2D ) )]
        public static IDescriptor Ray2DDescriptor() => new ClassOrStructDescriptor<Ray2D>()
            .WithMember( "origin", r => r.origin, ( ref Ray2D r, Vector2 v ) => r.origin = v )
            .WithMember( "direction", r => r.direction, ( ref Ray2D r, Vector2 v ) => r.direction = v );

        [MapsInheritingFrom( typeof( Plane ) )]
        public static IDescriptor PlaneDescriptor() => new ClassOrStructDescriptor<Plane>()
            .WithMember( "normal", p => p.normal, ( ref Plane p, Vector3 v ) => p.normal = v )
            .WithMember( "distance", p => p.distance, ( ref Plane p, float v ) => p.distance = v );

        // --- ANIMATION / GRADIENTS ---

        [MapsInheritingFrom( typeof( Keyframe ) )]
        public static IDescriptor KeyframeDescriptor() => new ClassOrStructDescriptor<Keyframe>()
            .WithMember( "time", k => k.time, ( ref Keyframe k, float v ) => k.time = v )
            .WithMember( "value", k => k.value, ( ref Keyframe k, float v ) => k.value = v )
            .WithMember( "in_tangent", k => k.inTangent, ( ref Keyframe k, float v ) => k.inTangent = v )
            .WithMember( "out_tangent", k => k.outTangent, ( ref Keyframe k, float v ) => k.outTangent = v )
            .WithMember( "in_weight", k => k.inWeight, ( ref Keyframe k, float v ) => k.inWeight = v )
            .WithMember( "out_weight", k => k.outWeight, ( ref Keyframe k, float v ) => k.outWeight = v )
            .WithMember( "weighted_mode", k => k.weightedMode, ( ref Keyframe k, WeightedMode v ) => k.weightedMode = v );

        [MapsInheritingFrom( typeof( AnimationCurve ) )]
        public static IDescriptor AnimationCurveDescriptor() => new ClassOrStructDescriptor<AnimationCurve>()
            .WithMember( "keys", c => c.keys, ( c, v ) => c.keys = v )
            .WithMember( "pre_wrap_mode", c => c.preWrapMode, ( c, v ) => c.preWrapMode = v )
            .WithMember( "post_wrap_mode", c => c.postWrapMode, ( c, v ) => c.postWrapMode = v );

        [MapsInheritingFrom( typeof( GradientColorKey ) )]
        public static IDescriptor GradientColorKeyDescriptor() => new ClassOrStructDescriptor<GradientColorKey>()
            .WithMember( "color", k => k.color, ( ref GradientColorKey k, Color v ) => k.color = v )
            .WithMember( "time", k => k.time, ( ref GradientColorKey k, float v ) => k.time = v );

        [MapsInheritingFrom( typeof( GradientAlphaKey ) )]
        public static IDescriptor GradientAlphaKeyDescriptor() => new ClassOrStructDescriptor<GradientAlphaKey>()
            .WithMember( "alpha", k => k.alpha, ( ref GradientAlphaKey k, float v ) => k.alpha = v )
            .WithMember( "time", k => k.time, ( ref GradientAlphaKey k, float v ) => k.time = v );

        [MapsInheritingFrom( typeof( Gradient ) )]
        public static IDescriptor GradientDescriptor() => new ClassOrStructDescriptor<Gradient>()
            .WithMember( "color_keys", g => g.colorKeys, ( g, v ) => g.colorKeys = v )
            .WithMember( "alpha_keys", g => g.alphaKeys, ( g, v ) => g.alphaKeys = v )
            .WithMember( "mode", g => g.mode, ( g, v ) => g.mode = v );
    }
}