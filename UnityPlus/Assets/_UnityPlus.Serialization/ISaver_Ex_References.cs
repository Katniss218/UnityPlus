﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.AssetManagement;

namespace UnityPlus.Serialization
{
    public static class ISaver_Ex_References
    {
        /// <summary>
        /// The special token name for a reference ID (part of Object).
        /// </summary>
        public const string ID = "$id";

        /// <summary>
        /// The special token name for a reference (part of Reference).
        /// </summary>
        public const string REF = "$ref";

        /// <summary>
        /// The special token name for an asset reference.
        /// </summary>
        public const string ASSETREF = "$assetref";



        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedData WriteObjectReference( this ISaver s, object value )
        {
            // A missing '$ref' node means the reference is broken.

            if( value == null )
            {
                return new SerializedObject();
            }

            Guid guid = s.GetID( value );

            return new SerializedObject()
            {
                { $"{REF}", s.WriteGuid( guid) }
            };
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedObject WriteAssetReference( this ISaver s, object assetRef )
        {
            if( assetRef == null )
            {
                return new SerializedObject();
            }

            string assetID = AssetRegistry.GetAssetID( assetRef );
            if( assetID == null )
            {
                return new SerializedObject();
            }

            return new SerializedObject()
            {
                { $"{ASSETREF}", assetID }
            };
        }

        /// <summary>
        /// Writes a delegate (reference to a method).
        /// </summary>
        /// <remarks>
        /// This is capable of fully serializing an arbitrary delegate, including multicasting, lambdas, and references to instance methods.
        /// 2. CHANGING CODE MIGHT INVALIDATE REFERENCES TO LAMBDAS.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedData WriteDelegate( this ISaver s, Delegate delegateObj )
        {
            SerializedArray invocationListJson = new SerializedArray();

            foreach( var del in delegateObj.GetInvocationList() )
            {
                SerializedData delJson = s.WriteSingleDelegate( del );
                invocationListJson.Add( delJson );
            }

            return invocationListJson;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static SerializedData WriteSingleDelegate( this ISaver s, Delegate delegateObj )
        {
            Type delegateType = delegateObj.GetType();

            MethodInfo method = delegateObj.Method;
            Type declaringType = method.DeclaringType;
            object target = delegateObj.Target;

            SerializedArray jsonParameters = new SerializedArray();
            ParameterInfo[] parameters = method.GetParameters();
            foreach( var param in parameters )
            {
                jsonParameters.Add( (SerializedPrimitive)param.ParameterType.AssemblyQualifiedName );
            }

            SerializedObject obj = new SerializedObject()
            {
                { "method", new SerializedObject() {
                    { "delegate_type", s.WriteType( delegateType ) },
                    { "identifier", method.Name },
                    { "parameters", jsonParameters },
                    { "declaring_type", s.WriteType( declaringType ) }
                } },
                { "target", s.WriteObjectReference( target ) }
            };
            return obj;
        }
    }
}