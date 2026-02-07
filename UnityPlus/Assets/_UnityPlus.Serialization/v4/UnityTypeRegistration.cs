
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class UnityTypeRegistration
    {
        [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
        static void Register()
        {
            // Register Primitive Descriptors
            TypeDescriptorRegistry.Register( new Vector2Descriptor() );
            TypeDescriptorRegistry.Register( new Vector3Descriptor() );
            TypeDescriptorRegistry.Register( new Vector4Descriptor() );
            TypeDescriptorRegistry.Register( new Vector2IntDescriptor() );
            TypeDescriptorRegistry.Register( new Vector3IntDescriptor() );
            TypeDescriptorRegistry.Register( new QuaternionDescriptor() );
            TypeDescriptorRegistry.Register( new Matrix4x4Descriptor() );
            TypeDescriptorRegistry.Register( new ColorDescriptor() );
            TypeDescriptorRegistry.Register( new Color32Descriptor() );
            TypeDescriptorRegistry.Register( new RectDescriptor() );
            TypeDescriptorRegistry.Register( new RectIntDescriptor() );
            TypeDescriptorRegistry.Register( new BoundsDescriptor() );
            TypeDescriptorRegistry.Register( new BoundsIntDescriptor() );

            // Register Composite Descriptors
            TypeDescriptorRegistry.Register( new GameObjectDescriptor() );

            // Register Component Descriptors
            TypeDescriptorRegistry.Register( UnityComponentDescriptors.Transform );
            TypeDescriptorRegistry.Register( UnityComponentDescriptors.MeshFilter );
            TypeDescriptorRegistry.Register( UnityComponentDescriptors.MeshRenderer );
            TypeDescriptorRegistry.Register( UnityComponentDescriptors.BoxCollider );
            TypeDescriptorRegistry.Register( UnityComponentDescriptors.SphereCollider );
            TypeDescriptorRegistry.Register( UnityComponentDescriptors.CapsuleCollider );
            TypeDescriptorRegistry.Register( UnityComponentDescriptors.MeshCollider );
            TypeDescriptorRegistry.Register( UnityComponentDescriptors.Camera );
            TypeDescriptorRegistry.Register( UnityComponentDescriptors.Light );
            TypeDescriptorRegistry.Register( UnityComponentDescriptors.LODGroup );
            TypeDescriptorRegistry.Register( UnityComponentDescriptors.LOD );

            // Register Context Mappings
            // Asset Context -> AssetDescriptor
            TypeDescriptorRegistry.Register( new AssetDescriptor<UnityEngine.Object>(), ObjectContext.Asset );

            // Reference Context -> ReferenceDescriptor
            TypeDescriptorRegistry.Register( new ReferenceDescriptor<object>(), ObjectContext.Ref );
        }
    }
}
