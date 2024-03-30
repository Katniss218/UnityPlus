using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.ReferenceMaps;

namespace Serialization
{
    public class PersistentTrimodalTests
    {
        public class TestClass : IAutoPersistsData
        {
            [Persist( "field_1", PersistMode.Data )]
            public int field1;

            [Persist( "property_1", PersistMode.Data )]
            public string Property1 { get; set; }
        }

        /*public class TestClass2 : IAutoPersistsData
        {
            [PersistObject( "field_1", AsImmutable = true )] // PersistObject defines ownership.
            public object field1;

            [PersistData( "property_1", AsImmutable = true )]
            public string Property1 { get; set; }
        }*/

        [Test]
        public void Test()
        {
            // Arrange

            var refMap = new BidirectionalReferenceStore();
            TestClass sut = new TestClass()
            {
                field1 = 5,
                Property1 = "hello"
            };

            // Act
            var data = sut.GetData( refMap );

            // Assert
            Assert.That( data["field_1"], Is.EqualTo( (SerializedPrimitive)"hello" ) );
        }
    }
}