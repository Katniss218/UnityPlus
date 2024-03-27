using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public class PersistentModeExtension
    {
        private static readonly Dictionary<Type, MethodInfo> _extensionGetDatas = new(); // TODO - replace with Func<...> and make a lambda to bridge.
        private static readonly Dictionary<Type, MethodInfo> _extensionSetDatas = new();


        public static void CacheMethods()
        {
            _extensionGetDatas.Clear();
            _extensionSetDatas.Clear();

            List<Type> availableContainingClasses = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany( a => a.GetTypes() )
                .Where( dt => !dt.IsSealed && !dt.IsGenericType )
                .ToList();

            foreach( var cls in availableContainingClasses )
            {
                MethodInfo[] methods = cls.GetMethods( BindingFlags.Public | BindingFlags.Static );

                foreach( var method in methods )
                {
                    if( method.Name == nameof( IPersistsData.GetData ) )
                    {
                        ParameterInfo retParam = method.ReturnParameter;
                        ParameterInfo[] methodParams = method.GetParameters();

                        if( retParam.ParameterType == typeof( SerializedData )
                         && methodParams.Length == 2
                         && methodParams[1].ParameterType == typeof( IReverseReferenceMap ) )
                        {
                            _extensionGetDatas.Add( methodParams[0].ParameterType, method );
                        }
                    }
                    if( method.Name == nameof( IPersistsData.SetData ) )
                    {
                        ParameterInfo retParam = method.ReturnParameter;
                        ParameterInfo[] methodParams = method.GetParameters();

                        if( retParam.ParameterType == typeof( void )
                         && methodParams.Length == 3
                         && methodParams[1].ParameterType == typeof( IReverseReferenceMap )
                         && methodParams[2].ParameterType == typeof( SerializedData ) )
                        {
                            _extensionSetDatas.Add( methodParams[0].ParameterType, method );
                        }
                    }
                }
            }
        }
    }
}