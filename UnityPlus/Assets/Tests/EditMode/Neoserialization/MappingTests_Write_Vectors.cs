//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityPlus.Serialization;
//using UnityPlus.Serialization.ReferenceMaps;

//namespace Neoserialization
//{
//    public class MappingTests_Write_Vectors
//    {
//        [Test]
//        public void Writing___Vector3___CorrectLayout()
//        {
//            // Arrange
//            var initialValue = new Vector3( 5.5f, -5f, 0f );

//            // Act
//            var data = SerializationUnit.Serialize<Vector3>( ObjectContext.Value, initialValue );

//            // Assert
//            Assert.That( data, Is.EqualTo(
//                new SerializedArray()
//                {
//                    (SerializedPrimitive)5.5f,
//                    (SerializedPrimitive)(-5f),
//                    (SerializedPrimitive)0f,
//                }
//            ) );
//        }

//        [Test]
//        public void Writing___Vector3_Boxed___CorrectLayout()
//        {
//            // Arrange
//            var initialValue = new Vector3( 5.5f, -5f, 0f );
//            var refMap = new BidirectionalReferenceStore();

//            // Act
//            var data = SerializationUnit.Serialize<object>( ObjectContext.Value, initialValue, refMap );

//            // Assert
//            Assert.That( data, Is.EqualTo(
//                new SerializedObject()
//                {
//                    { "$type", initialValue.GetType().SerializeType() },
//                    { "$id", refMap.GetID( initialValue ).SerializeGuid() },
//                    { "value", new SerializedArray()
//                        {
//                            (SerializedPrimitive)5.5f,
//                            (SerializedPrimitive)(-5f),
//                            (SerializedPrimitive)0f,
//                        } }
//                }
//            ) );
//        }
//    }
//}