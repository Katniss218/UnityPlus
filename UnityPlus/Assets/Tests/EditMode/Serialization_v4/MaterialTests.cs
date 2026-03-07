using NUnit.Framework;
using UnityEngine;
using UnityPlus.AssetManagement;

namespace UnityPlus.Serialization.Tests
{
    [TestFixture]
    public class MaterialTests
    {
        [SetUp]
        public void Setup()
        {
            AssetRegistry.Clear();
        }

        [TearDown]
        public void Teardown()
        {
            AssetRegistry.Clear();
        }

        [Test]
        public void Material_RoundTrips_AllPropertyTypes_And_Keywords()
        {
            // 1. Setup Shader as Asset
            // "Standard" shader is the best candidate as it has Color, Float, Texture, and Keywords.
            Shader shader = Shader.Find( "Standard" );
            if( shader == null )
            {
                // Fallback to a shader that definitely exists if Standard is missing (e.g. URP/HDRP projects)
                // But URP/HDRP have their own standard shaders.
                // Let's try "Hidden/InternalErrorShader" as a fallback? No, it has no properties.
                // "Sprites/Default" has Color and Texture.
                shader = Shader.Find( "Sprites/Default" );
            }

            Assert.That( shader, Is.Not.Null, "Could not find a suitable shader ('Standard' or 'Sprites/Default') for testing." );

            string shaderAssetId = "shader::test_shader";
            AssetRegistry.Register( shaderAssetId, shader );

            // 2. Create Material
            Material original = new Material( shader );
            original.name = "TestMaterial";

            // 3. Set Properties

            // -- Color --
            // Standard and Sprites/Default both have _Color
            Color testColor = new Color( 0.1f, 0.2f, 0.3f, 0.4f );
            if( original.HasProperty( "_Color" ) )
            {
                original.SetColor( "_Color", testColor );
            }

            // -- Texture --
            // Standard and Sprites/Default both have _MainTex
            Texture2D texture = new Texture2D( 4, 4 );
            texture.name = "TestTexture";
            string textureAssetId = "tex::test_texture";
            AssetRegistry.Register( textureAssetId, texture );

            Vector2 texOffset = new Vector2( 0.1f, 0.2f );
            Vector2 texScale = new Vector2( 0.5f, 0.5f );

            if( original.HasProperty( "_MainTex" ) )
            {
                original.SetTexture( "_MainTex", texture );
                original.SetTextureOffset( "_MainTex", texOffset );
                original.SetTextureScale( "_MainTex", texScale );
            }

            // -- Float --
            // Standard has _Cutoff, _Glossiness, etc.
            float testFloat = 0.75f;
            if( original.HasProperty( "_Cutoff" ) )
            {
                original.SetFloat( "_Cutoff", testFloat );
            }

            // -- Keywords --
            string testKeyword = "_EMISSION"; // keyword needs to be a supported keyword for this to work, but both Standard and Sprites/Default support _EMISSION
            original.EnableKeyword( testKeyword );

            // 4. Serialize
            SerializedData data = SerializationUnit.Serialize( original );
            Debug.Log( data.DumpToString() );

            // 5. Deserialize
            Material deserialized = SerializationUnit.Deserialize<Material>( data );

            // 6. Assert
            Assert.That( deserialized, Is.Not.Null );
            Assert.That( deserialized.shader, Is.EqualTo( shader ) );

            // Verify Color
            if( original.HasProperty( "_Color" ) )
            {
                Assert.That( deserialized.GetColor( "_Color" ), Is.EqualTo( testColor ) );
            }

            // Verify Texture
            if( original.HasProperty( "_MainTex" ) )
            {
                Assert.That( deserialized.GetTexture( "_MainTex" ), Is.EqualTo( texture ) );
                Assert.That( deserialized.GetTextureOffset( "_MainTex" ), Is.EqualTo( texOffset ) );
                Assert.That( deserialized.GetTextureScale( "_MainTex" ), Is.EqualTo( texScale ) );
            }

            // Verify Float
            if( original.HasProperty( "_Cutoff" ) )
            {
                Assert.That( deserialized.GetFloat( "_Cutoff" ), Is.EqualTo( testFloat ) );
            }

            // Verify Keywords
            Assert.That( deserialized.IsKeywordEnabled( testKeyword ), Is.True );

            // Cleanup
            Object.DestroyImmediate( original );
            Object.DestroyImmediate( deserialized );
            Object.DestroyImmediate( texture );
        }
    }
}