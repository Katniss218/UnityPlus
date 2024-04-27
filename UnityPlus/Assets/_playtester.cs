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
using UnityPlus.Serialization.ReferenceMaps;
using UnityPlus.Serialization.Strategies;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

public class _playtester : MonoBehaviour
{
    void Start()
    {
        UICanvas staticCanvas = CanvasManager.Get( "static" );
        UICanvas contextMenuCanvas = CanvasManager.Get( "contextmenus" );

        staticCanvas.AddPanel( new UILayoutInfo( UIAnchor.Center, (0, 0), (300, 300) ), null );
    }

    void Update()
    {
    }
}