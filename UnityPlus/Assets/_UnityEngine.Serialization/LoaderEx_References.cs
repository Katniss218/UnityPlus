﻿using Newtonsoft.Json.Linq;
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
    public static class LoaderEx_References
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static object ReadObjectReference( this Loader l, JToken json )
        {
            // should only be called in data actions.

            // A missing '$ref' node means the reference couldn't save properly.

            if( ((JObject)json).TryGetValue( $"{Saver_Ex_References.REF}", out JToken refJson ) )
            {
                Guid guid = l.ReadGuid( refJson );

                return l.Get( guid );
            }
            return null;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static T ReadAssetReference<T>( this Loader l, JToken json ) where T : class
        {
            if( ((JObject)json).TryGetValue( $"{Saver_Ex_References.ASSETREF}", out JToken refJson ) )
            {
                string assetID = (string)refJson;

                return Registry.Get<T>( assetID );
            }
            return null;
        }

        /// <summary>
        /// Reads a delegate (reference to a method).
        /// </summary>
        /// <remarks>
        /// This is capable of fully deserializing an arbitrary delegate, including multicasting, lambdas, and references to instance methods.
        /// 1. THE TARGET OBJECT SHOULD BE DESERIALIZED BEFOREHAND.
        /// 2. CHANGING CODE MIGHT INVALIDATE REFERENCES TO LAMBDAS.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Delegate ReadDelegate( this Loader l, JToken json )
        {
            JArray jsonA = (JArray)json;

            if( jsonA.Count == 1 )
            {
                return l.ReadSingleDelegate( jsonA[0] );
            }

            Delegate[] invocationList = new Delegate[jsonA.Count];
            for( int i = 0; i < jsonA.Count; i++ )
            {
                invocationList[i] = l.ReadSingleDelegate( jsonA[i] );
            }
            return Delegate.Combine( invocationList );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static Delegate ReadSingleDelegate( this Loader l, JToken json )
        {
            // TODO - this requires the target to be already deserialized.
            object target = l.ReadObjectReference( json["Target"] );

            Type delegateType = l.ReadType( json["method"]["delegate_type"] );
            Type declaringType = l.ReadType( json["method"]["declaring_type"] );
            List<Type> parameters = new List<Type>();
            foreach( var jsonParam in json["method"]["parameters"] )
            {
                parameters.Add( l.ReadType( jsonParam ) );
            }
            string methodName = (string)json["method"]["identifier"];

            MethodInfo method = declaringType.GetMethod( methodName, parameters.ToArray() );
            Delegate delegateObj = method.CreateDelegate( delegateType, target );

            return delegateObj;
            // returns the delegate object that is ready to be assigned to the field
        }
    }
}