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
    [CustomEditor( typeof( ConstrainedRegUpdater ) )]
    public class ConstrainedRegUpdaterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ConstrainedRegUpdater updater = (ConstrainedRegUpdater)target;

            // Display the default inspector GUI elements
            DrawDefaultInspector();

            // Add a button to call a method
            if( GUILayout.Button( "Refresh Assets" ) )
            {
                updater.TryUpdateEntries();
            }
        }
    }
#endif

    public class ConstrainedRegUpdater : MonoBehaviour
    {
        [SerializeField]
        AssetRegisterer _registerer;

        [SerializeField]
        bool _autoUpdate = false;

        [SerializeField]
        string _constrainPath = "Assets/Resources/";

        [SerializeField]
        string _assetPrefix = "builtin::";

#if UNITY_EDITOR
        void AddEntry( ref List<AssetRegisterer.Entry> entries, string assetID, Object asset )
        {
            if( asset is DefaultAsset // folders (maybe more)
             || asset is MonoScript ) // C# files, but not scriptable object instances.
            {
                return;
            }

            entries.Add( new AssetRegisterer.Entry() { assetID = assetID, asset = asset } );
        }

        internal void TryUpdateEntries()
        {
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();

            List<AssetRegisterer.Entry> entries = new List<AssetRegisterer.Entry>();

            var x = AssetDatabase.LoadAllAssetsAtPath( "Library/unity default resources" );
            foreach( var asset in x )
            {
                if( asset.name.StartsWith( "Hidden/" ) )
                {
                    continue;
                }

                int end = asset.name.LastIndexOf( '.' );
                string assetID = end == -1 ? asset.name : asset.name[..end];
                assetID = $"{_assetPrefix}{assetID}";

                AddEntry( ref entries, assetID, asset );
            }

            var y = AssetDatabase.LoadAllAssetsAtPath( "Resources/unity_builtin_extra" );
            foreach( var asset in y )
            {
                if( asset.name.StartsWith( "Hidden/" ) )
                {
                    continue;
                }

                int end = asset.name.LastIndexOf( '.' );
                string assetID = end == -1 ? asset.name : asset.name[..end];
                assetID = $"{_assetPrefix}{assetID}";

                AddEntry( ref entries, assetID, asset );
            }

            foreach( var path in allAssetPaths )
            {
                if( !path.StartsWith( _constrainPath ) )
                {
                    continue;
                }

                Object asset = AssetDatabase.LoadAssetAtPath<Object>( path );

                int start = _constrainPath.Length;
                int end = path.LastIndexOf( '.' );
                string assetID = end == -1 ? path[start..] : path[start..end];
                assetID = $"{_assetPrefix}{assetID}";

                AddEntry( ref entries, assetID, asset );
            }

            _registerer.TrySetAssetsToRegister( entries.ToArray() );
        }

        private void OnValidate()
        {
            if( _registerer != null && _autoUpdate )
            {
                TryUpdateEntries();
            }
        }
#endif
    }
}
