using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityPlus.AssetManagement;
using UnityPlus.CSharp;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;
using UnityPlus.Serialization.Mappings;
using UnityPlus.Serialization.ReferenceMaps;
using UnityPlus.Serialization.Strategies;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

public class _playtester : MonoBehaviour
{
    void Start()
    {
        //UICanvas staticCanvas = CanvasManager.Get( "static" );
        // UICanvas contextMenuCanvas = CanvasManager.Get( "contextmenus" );

        //staticCanvas.AddPanel( new UILayoutInfo( UIAnchor.Center, (0, 0), (300, 300) ), null );

        // var mapping = (SerializationMapping<MeshFilter>)TestMappings.MeshFilterMapping();
        // var mapping = (CompoundMapping<GameObject>)TestMappings.GameObjectMapping();
        var mapping = (CompoundMapping<GameObject>)SerializationMapping.GetMappingFor<GameObject>();

        var mf = this.gameObject.AddComponent<MeshFilter>();

        SerializedObject data = mapping.GetDataPass( this.gameObject, new BidirectionalReferenceStore() );

        StringBuilder sb = new StringBuilder();
        new JsonStringWriter( data, sb ).Write();
        string s = sb.ToString();

        s = s.Replace( "true", "false" );

        data = (SerializedObject)new JsonStringReader( s ).Read();

        mapping.SetDataPass( this.gameObject, data, new BidirectionalReferenceStore() );
    }

    void Update()
    {
    }
}