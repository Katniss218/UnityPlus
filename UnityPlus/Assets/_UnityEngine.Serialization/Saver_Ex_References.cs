using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AssetManagement;

namespace UnityEngine.Serialization
{
    public static class Saver_Ex_References
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
        public static JToken WriteObjectReference( this Saver s, object value )
        {
            // A missing '$ref' node means the reference is broken.

            if( value == null )
            {
                return new JObject();
            }

            Guid guid = s.GetID( value );

            return new JObject()
            {
                { $"{REF}", s.WriteGuid( guid) }
            };
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static JToken WriteAssetReference( this Saver s, object assetRef )
        {
            if( assetRef == null )
            {
                return new JObject();
            }

            string assetID = AssetRegistry.GetAssetID( assetRef );
            if( assetID == null )
            {
                return new JObject();
            }

            return new JObject()
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
        public static JToken WriteDelegate( this Saver s, Delegate delegateObj )
        {
            JArray invocationListJson = new JArray();

            foreach( var del in delegateObj.GetInvocationList() )
            {
                JToken delJson = s.WriteSingleDelegate( del );
                invocationListJson.Add( delJson );
            }

            return invocationListJson;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static JToken WriteSingleDelegate( this Saver s, Delegate delegateObj )
        {
            Type delegateType = delegateObj.GetType();

            MethodInfo method = delegateObj.Method;
            Type declaringType = method.DeclaringType;
            object target = delegateObj.Target;

            JArray jsonParameters = new JArray();
            ParameterInfo[] parameters = method.GetParameters();
            foreach( var param in parameters )
            {
                jsonParameters.Add( param.ParameterType.AssemblyQualifiedName );
            }

            JObject obj = new JObject()
            {
                { "method", new JObject() {
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