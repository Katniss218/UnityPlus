using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.AssetManagement
{
#if UNITY_EDITOR
    [CustomPropertyDrawer( typeof( AssetRegisterer.Entry ) )]
    public class EntryDrawer : PropertyDrawer
    {
        public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
        {
            EditorGUI.BeginProperty( position, label, property );

            // Calculate the width for each field
            float labelWidth = 0.666666f;

            // Create rectangles for the two fields
            Rect assetIDRect = new Rect( position.x, position.y, (position.width * labelWidth), position.height );
            Rect assetRect = new Rect( position.x + (position.width * labelWidth), position.y, (position.width * (1 - labelWidth)), position.height );

            // Get the serialized properties for the two fields
            SerializedProperty assetIDProperty = property.FindPropertyRelative( nameof( AssetRegisterer.Entry.assetID ) );
            SerializedProperty assetProperty = property.FindPropertyRelative( nameof( AssetRegisterer.Entry.asset ) );

            // Draw the two fields side by side without labels
            EditorGUI.PropertyField( assetIDRect, assetIDProperty, GUIContent.none );
            EditorGUI.PropertyField( assetRect, assetProperty, GUIContent.none );

            EditorGUI.EndProperty();
        }
    }
#endif

    /// <summary>
    /// Registers a specific set of assets (Unity Objects) when its first initialized.
    /// </summary>
    public class AssetRegisterer : MonoBehaviour
    {
        /// <summary>
        /// Describes a specific `asset ID - asset reference` pair.
        /// </summary>
        [Serializable]
        public struct Entry
        {
            /// <summary>
            /// The asset ID to register under.
            /// </summary>
            public string assetID;

            /// <summary>
            /// The asset to register.
            /// </summary>
            public Object asset;
        }

        [SerializeField]
        private Entry[] _assetsToRegister;

        [SerializeField]
        private bool _autoUpdateFromResources; // update the value of `_assetsToRegister` based on what is currently in the Resources directory.

        void Awake()
        {
            if( _assetsToRegister == null )
            {
                return;
            }

            foreach( var entry in _assetsToRegister )
            {
                if( entry.assetID == null )
                {
                    Debug.LogWarning( $"Null asset ID present in list of assets to register." );
                    continue;
                }
                if( entry.asset == null )
                {
                    Debug.LogWarning( $"Null asset present in list of assets to register." );
                    continue;
                }
                Registry.Register( entry.assetID, entry.asset );
            }

            // Allows to garbage collect them later, if unloaded from the registry.
            _assetsToRegister = null;
        }

#if UNITY_EDITOR
        void TryUpdateEntriesResources()
        {
            const string PATH_DIR = "Assets/Resources/";

            if( !_autoUpdateFromResources )
            {
                return;
            }

            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();

            List<Entry> entries = new List<Entry>();

            foreach( var path in allAssetPaths )
            {
                if( !path.StartsWith( PATH_DIR ) )
                {
                    continue;
                }

                Object asset = AssetDatabase.LoadAssetAtPath<Object>( path );
                if( asset is DefaultAsset )
                {
                    continue;
                }

                int start = PATH_DIR.Length;
                int end = path.LastIndexOf( '.' );
                string assetID = end == -1 ? path[start..] : path[start..end];

                entries.Add( new Entry() { assetID = $"resources::{assetID}", asset = asset } );
            }

            _assetsToRegister = entries.ToArray();
        }

        private void OnValidate()
        {
            TryUpdateEntriesResources();
        }
#endif
    }
}
