using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;
using UnityPlus.Serialization.Strategies;

public class _testscenereferencer : MonoBehaviour, IPersistsData
{
    public GameObject go;
    public Component c;

    public SerializedData GetData( IReverseReferenceMap s )
    {
        return new SerializedObject()
        {
            { "go", s.WriteObjectReference(go) },
            { "c", s.WriteObjectReference(c) }
        };
    }

    public void SetData( SerializedData json, IForwardReferenceMap l )
    {
        this.go = (GameObject)l.ReadObjectReference( json["go"] );
        this.c = (Component)l.ReadObjectReference( json["c"] );
    }
}