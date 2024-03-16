using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityPlus.Serialization;
using UnityEngine.TestTools;
using UnityPlus.Serialization.Strategies;

public class ComponentFinderTests
{
    public class TestComponent : MonoBehaviour { }

    [Test]
    public void GetComponents_Component____CorrectOrder()
    {
        // Arrange
        Component[] components = new Component[5];
        GameObject go = new GameObject();
        components[0] = go.GetComponent<Transform>();
        components[1] = go.AddComponent<MeshRenderer>();
        components[2] = go.AddComponent<TestComponent>();
        components[3] = go.AddComponent<TestComponent>();
        components[4] = go.AddComponent<MeshFilter>();

        // Act
        Component[] componentsReturned = go.GetComponents<Component>();

        // Assert
        Assert.IsTrue( components.SequenceEqual( componentsReturned ) );
    }

    [Test]
    public void GetByIndex___CorrectValue___ValidData()
    {
        // Arrange
        Component[] components = new Component[5];
        GameObject go = new GameObject();
        components[0] = go.GetComponent<Transform>();
        components[1] = go.AddComponent<MeshRenderer>();
        components[2] = go.AddComponent<TestComponent>();
        components[3] = go.AddComponent<TestComponent>();
        components[4] = go.AddComponent<MeshFilter>();

        // Act
        Component c0 = (Component)StratUtils.GetComponentOrGameObject( go, "*0" );
        Component c1 = (Component)StratUtils.GetComponentOrGameObject( go, "*1" );
        Component c2 = (Component)StratUtils.GetComponentOrGameObject( go, "*2" );
        Component c3 = (Component)StratUtils.GetComponentOrGameObject( go, "*3" );
        Component c4 = (Component)StratUtils.GetComponentOrGameObject( go, "*4" );

        // Assert
        Assert.IsTrue( c0 == components[0] );
        Assert.IsTrue( c1 == components[1] );
        Assert.IsTrue( c2 == components[2] );
        Assert.IsTrue( c3 == components[3] );
        Assert.IsTrue( c4 == components[4] );
    }
}