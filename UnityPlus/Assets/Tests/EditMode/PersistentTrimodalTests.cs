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
            [Persist( "field_1" )]
            public object field1;

            [Persist( "property_1" )]
            public string Property1 { get; set; }
        }

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