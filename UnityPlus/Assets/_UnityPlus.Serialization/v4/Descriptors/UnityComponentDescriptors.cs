using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class UnityComponentDescriptors
    {
        // --- BASE ---

        public static ClassDescriptor<Transform> Transform => new ClassDescriptor<Transform>()
            .WithMember( "localPosition", t => t.localPosition )
            .WithMember( "localRotation", t => t.localRotation )
            .WithMember( "localScale", t => t.localScale );

        // --- PHYSICS ---

        public static ClassDescriptor<BoxCollider> BoxCollider => new ClassDescriptor<BoxCollider>()
            .WithMember( "isTrigger", c => c.isTrigger )
            .WithMember( "center", c => c.center )
            .WithMember( "size", c => c.size );

        public static ClassDescriptor<SphereCollider> SphereCollider => new ClassDescriptor<SphereCollider>()
            .WithMember( "isTrigger", c => c.isTrigger )
            .WithMember( "center", c => c.center )
            .WithMember( "radius", c => c.radius );

        public static ClassDescriptor<CapsuleCollider> CapsuleCollider => new ClassDescriptor<CapsuleCollider>()
            .WithMember( "isTrigger", c => c.isTrigger )
            .WithMember( "center", c => c.center )
            .WithMember( "radius", c => c.radius )
            .WithMember( "height", c => c.height )
            .WithMember( "direction", c => c.direction );

        public static ClassDescriptor<MeshCollider> MeshCollider => new ClassDescriptor<MeshCollider>()
            .WithMember( "isTrigger", c => c.isTrigger )
            .WithMember( "convex", c => c.convex )
            .WithAsset( "sharedMesh", c => c.sharedMesh );

        // --- RENDERING ---

        public static ClassDescriptor<MeshFilter> MeshFilter => new ClassDescriptor<MeshFilter>()
            .WithAsset( "sharedMesh", m => m.sharedMesh );

        public static ClassDescriptor<MeshRenderer> MeshRenderer => new ClassDescriptor<MeshRenderer>()
            .WithMember( "enabled", r => r.enabled )
            .WithMember( "shadowCastingMode", r => r.shadowCastingMode )
            .WithMember( "receiveShadows", r => r.receiveShadows )
            .WithMember( "sharedMaterials", ObjectContext.Asset, r => r.sharedMaterials );

        public static ClassDescriptor<Camera> Camera => new ClassDescriptor<Camera>()
            .WithMember( "enabled", c => c.enabled )
            .WithMember( "clearFlags", c => c.clearFlags )
            .WithMember( "backgroundColor", c => c.backgroundColor )
            .WithMember( "cullingMask", c => c.cullingMask )
            .WithMember( "orthographic", c => c.orthographic )
            .WithMember( "orthographicSize", c => c.orthographicSize )
            .WithMember( "fieldOfView", c => c.fieldOfView )
            .WithMember( "nearClipPlane", c => c.nearClipPlane )
            .WithMember( "farClipPlane", c => c.farClipPlane )
            .WithMember( "depth", c => c.depth );

        public static ClassDescriptor<Light> Light => new ClassDescriptor<Light>()
            .WithMember( "enabled", l => l.enabled )
            .WithMember( "type", l => l.type )
            .WithMember( "color", l => l.color )
            .WithMember( "intensity", l => l.intensity )
            .WithMember( "range", l => l.range )
            .WithMember( "spotAngle", l => l.spotAngle )
            .WithMember( "shadows", l => l.shadows );

        public static ClassDescriptor<LODGroup> LODGroup => new ClassDescriptor<LODGroup>()
            .WithMember( "localReferencePoint", l => l.localReferencePoint )
            .WithMember( "size", l => l.size )
            .WithMember( "fadeMode", l => l.fadeMode )
            .WithMember( "animateCrossFading", l => l.animateCrossFading )
            .WithMember( "lods", l => l.GetLODs(), ( l, v ) => l.SetLODs( v ) );

        // Struct needed for LODGroup
        public static ClassDescriptor<LOD> LOD => new ClassDescriptor<LOD>()
            .WithMember( "screenRelativeTransitionHeight", l => l.screenRelativeTransitionHeight )
            .WithMember( "fadeTransitionWidth", l => l.fadeTransitionWidth )
            .WithMember( "renderers", ObjectContext.Ref, l => l.renderers );
    }
}
