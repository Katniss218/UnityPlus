using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization.ComponentData;
using UnityEngine.TestTools;

public class ComponentFinderTest
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
        Component c = ComponentFinder.GetComponentByIndex( components, 1 );

        // Assert
        Assert.IsTrue( c == components[1] );
    }

    [Test]
    public void GetByTypeAndIndex___CorrectValue___ValidData()
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
        Component c = ComponentFinder.GetComponentByTypeAndIndex( components, (typeof( TestComponent ), 1) );

        // Assert
        Assert.IsTrue( c == components[3] );
    }
}