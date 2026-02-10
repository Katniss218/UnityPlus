
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class UnityTypeRegistration
    {
        [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
        static void Register()
        {
            // Note: Primitives and Components are now discovered via reflection attributes.
            // This method now only handles manual registrations if necessary.

            // Register Composite Descriptors (Still explicitly registered for now)
            TypeDescriptorRegistry.Register( new GameObjectDescriptor() );

            // Register Context Mappings
            // Asset Context -> AssetDescriptor
            TypeDescriptorRegistry.Register( new AssetDescriptor<UnityEngine.Object>(), ObjectContext.Asset );

            // Reference Context -> ReferenceDescriptor
            TypeDescriptorRegistry.Register( new ReferenceDescriptor<object>(), ObjectContext.Ref );
        }
    }
}