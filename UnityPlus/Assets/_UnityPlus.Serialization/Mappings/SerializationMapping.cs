using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public abstract class SerializationMapping
    {
        private static readonly TypeMap<SerializationMapping> _mappings = new();

        private static bool _isInitialized = false;

        private static IEnumerable<Type> GetTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany( a => a.GetTypes() );
        }

        private static void Initialize()
        {
            foreach( var containingType in GetTypes() )
            {
                MethodInfo[] methods = containingType.GetMethods( BindingFlags.Public | BindingFlags.Static );
                foreach( var method in methods )
                {
                    SerializationMappingProviderAttribute attr = method.GetCustomAttribute<SerializationMappingProviderAttribute>();
                    if( attr == null )
                        continue;

                    if( method.ReturnParameter.ParameterType != typeof( SerializationMapping ) )
                        continue;

                    if( method.GetParameters().Length != 0 )
                        continue;

                    var targetType = attr.TargetType;
                    var mapping = (SerializationMapping)method.Invoke( null, null );

                    _mappings.Set( targetType, mapping );
                }
            }

            _isInitialized = true;
        }

        public static SerializationMapping GetMappingFor<TMember>()
        {
            if( !_isInitialized )
                Initialize();

            return _mappings.GetClosestOrDefault( typeof( TMember ) );
        }

        public static SerializationMapping GetMappingFor<TMember>( TMember memberObj )
        {
            if( !_isInitialized )
                Initialize();

            return _mappings.GetClosestOrDefault( memberObj.GetType() );
        }
    }
}