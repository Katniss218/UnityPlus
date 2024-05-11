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
        var mapping = SerializationMapping.GetMappingFor( this.gameObject );

        SerializedData data = mapping.Save( this.gameObject, new BidirectionalReferenceStore() );

        StringBuilder sb = new StringBuilder();
        new JsonStringWriter( data, sb ).Write();
        string s = sb.ToString();
        s = s.Replace( "true", "false" );

        data = new JsonStringReader( s ).Read();

        object obj = mapping.Load( data, new BidirectionalReferenceStore() );
    }

    void Update()
    {
    }
}