using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class TypeDescriptorRegistry
    {
        // Cache: (Type, Context) -> Descriptor
        private static readonly Dictionary<(Type, int), IDescriptor> _descriptors = new Dictionary<(Type, int), IDescriptor>();

        // Provider Lookups (V3 Style - Generalized)
        private static readonly Dictionary<(int, Type), MethodInfo> _inheritingProviders = new Dictionary<(int, Type), MethodInfo>();
        private static readonly Dictionary<(int, Type), MethodInfo> _implementingProviders = new Dictionary<(int, Type), MethodInfo>();
        private static readonly Dictionary<int, MethodInfo> _anyClassProviders = new Dictionary<int, MethodInfo>();
        private static readonly Dictionary<int, MethodInfo> _anyStructProviders = new Dictionary<int, MethodInfo>();
        private static readonly Dictionary<int, MethodInfo> _anyInterfaceProviders = new Dictionary<int, MethodInfo>();
        private static readonly Dictionary<int, MethodInfo> _anyProviders = new Dictionary<int, MethodInfo>();

        // Extensions: (TargetType, Context) -> List of Extension Methods
        private static readonly Dictionary<(Type, int), List<MethodInfo>> _extensions = new Dictionary<(Type, int), List<MethodInfo>>();

        private static bool _isInitialized = false;

        private static void Initialize()
        {
            if( _isInitialized ) return;

            // Force initialization of compatibility context constants before reflecting on them
#pragma warning disable CS0618 // Type or member is obsolete
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor( typeof( ObjectContext ).TypeHandle );
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor( typeof( ArrayContext ).TypeHandle );
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor( typeof( KeyValueContext ).TypeHandle );
#pragma warning restore CS0618

            // Scan all assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach( var assembly in assemblies )
            {
                string name = assembly.GetName().Name;
                if( name.StartsWith( "System" ) || name.StartsWith( "mscorlib" ) || name.StartsWith( "UnityEditor" ) )
                    continue;

                foreach( var type in assembly.GetTypes() )
                {
                    // Scan methods
                    var methods = type.GetMethods( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );
                    foreach( var method in methods )
                    {
                        // Extensions
                        var extAttributes = method.GetCustomAttributes<ExtendsMappingOfAttribute>( false );
                        foreach( var attr in extAttributes )
                        {
                            var key = (attr.TargetType, attr.Context);
                            if( !_extensions.ContainsKey( key ) )
                                _extensions[key] = new List<MethodInfo>();

                            _extensions[key].Add( method );
                        }

                        // Mapping Providers
                        var providerAttributes = method.GetCustomAttributes<MappingProviderAttribute>( false );
                        foreach( var attr in providerAttributes )
                        {
                            if( !typeof( IDescriptor ).IsAssignableFrom( method.ReturnType ) ) continue;

                            IEnumerable<int> targetContexts;
                            if( attr.ContextType != null )
                            {
                                int id = ContextRegistry.GetID( attr.ContextType ).ID;
                                targetContexts = new int[] { id };
                            }
                            else
                            {
                                targetContexts = attr.Contexts;
                            }

                            foreach( var ctx in targetContexts )
                            {
                                if( attr is MapsInheritingFromAttribute inh ) _inheritingProviders[(ctx, inh.MappedType)] = method;
                                else if( attr is MapsImplementingAttribute imp ) _implementingProviders[(ctx, imp.MappedType)] = method;
                                else if( attr is MapsAnyClassAttribute ) _anyClassProviders[ctx] = method;
                                else if( attr is MapsAnyStructAttribute ) _anyStructProviders[ctx] = method;
                                else if( attr is MapsAnyInterfaceAttribute ) _anyInterfaceProviders[ctx] = method;
                                else if( attr is MapsAnyAttribute ) _anyProviders[ctx] = method;
                            }
                        }
                    }

                }
            }

            _isInitialized = true;
        }

        public static void Register( IDescriptor descriptor, ContextKey context = default )
        {
            if( descriptor == null ) return;
            _descriptors[(descriptor.MappedType, context.ID)] = descriptor;
        }

        public static IDescriptor GetDescriptor( Type type, ContextKey context = default )
        {
            if( type == null ) return null;

            if( !_isInitialized )
                Initialize();

            if( _descriptors.TryGetValue( (type, context.ID), out var descriptor ) )
            {
                return descriptor;
            }

            descriptor = CreateDescriptor( type, context );
            if( descriptor != null )
            {
                ApplyExtensions( descriptor, type, context );
                Register( descriptor, context );
            }

            return descriptor;
        }

        public static void Clear()
        {
            _descriptors.Clear();

            _inheritingProviders.Clear();
            _implementingProviders.Clear();
            _anyClassProviders.Clear();
            _anyStructProviders.Clear();
            _anyInterfaceProviders.Clear();
            _anyProviders.Clear();

            _extensions.Clear();

            _isInitialized = false;
        }

        private static void ApplyExtensions( IDescriptor descriptor, Type type, ContextKey context )
        {
            if( _extensions.TryGetValue( (type, context.ID), out var methods ) )
            {
                object[] args = new object[] { descriptor };
                foreach( var method in methods )
                {
                    try
                    {
                        method.Invoke( null, args );
                    }
                    catch( Exception ex )
                    {
                        Debug.LogError( $"Error applying extension '{method.Name}' to descriptor for {type.Name}: {ex}" );
                    }
                }
            }
        }

        /// <summary>
        /// Creates a descriptor instance for the specified type and context using resolution order.
        /// </summary>
        private static IDescriptor CreateDescriptor( Type type, ContextKey context )
        {
            int contextId = context.ID;

            // Providers:
            // --- Provider Attributes (Inheritance) ---
            Type checkType = type;
            while( checkType != null )
            {
                if( _inheritingProviders.TryGetValue( (contextId, checkType), out var method ) )
                    return InvokeProvider( method, type, context );

                if( checkType.IsGenericType && !checkType.IsGenericTypeDefinition )
                {
                    if( _inheritingProviders.TryGetValue( (contextId, checkType.GetGenericTypeDefinition()), out method ) )
                        return InvokeProvider( method, type, context );
                }

                checkType = checkType.BaseType;
            }

            // --- Provider Attributes (Interfaces) ---
            foreach( var iface in type.GetInterfaces() )
            {
                if( _implementingProviders.TryGetValue( (contextId, iface), out var method ) )
                    return InvokeProvider( method, type, context );

                if( iface.IsGenericType && !iface.IsGenericTypeDefinition )
                {
                    if( _implementingProviders.TryGetValue( (contextId, iface.GetGenericTypeDefinition()), out method ) )
                        return InvokeProvider( method, type, context );
                }
            }

            // --- Provider Attributes (Category) ---
            if( type.IsClass && _anyClassProviders.TryGetValue( contextId, out var classMethod ) )
                return InvokeProvider( classMethod, type, context );
            if( type.IsValueType && _anyStructProviders.TryGetValue( contextId, out var structMethod ) )
                return InvokeProvider( structMethod, type, context );
            if( type.IsInterface && _anyInterfaceProviders.TryGetValue( contextId, out var ifaceMethod ) )
                return InvokeProvider( ifaceMethod, type, context );
            if( _anyProviders.TryGetValue( contextId, out var anyMethod ) )
                return InvokeProvider( anyMethod, type, context );

            // --- Reflection Fallback ---
            if( type.IsClass || type.IsValueType || type.IsInterface )
            {
                Type descType = typeof( ReflectionClassDescriptor<> ).MakeGenericType( type );
                return (IDescriptor)Activator.CreateInstance( descType );
            }

            return null;
        }

        private static IDescriptor InvokeProvider( MethodInfo method, Type targetType, ContextKey context )
        {
            try
            {
                // Determine Generic Arguments based on the Target Type
                Type[] genericArgs;
                if( targetType.IsArray )
                {
                    genericArgs = new Type[] { targetType.GetElementType() };
                }
                else if( targetType.IsGenericType )
                {
                    genericArgs = targetType.GetGenericArguments();
                }
                else if( targetType.IsEnum )
                {
                    genericArgs = new Type[] { targetType };
                }
                else
                {
                    /*
                    if( method.GetGenericArguments().Length != objType.GetGenericArguments().Length )
                    {
                        throw new InvalidOperationException( $"Couldn't initialize mapping from method `{method}` (mapped type: `{objType}`). Number of generic parameters on the method doesn't match the number of generic parameters on the mapped type." );
                    }*/
                    genericArgs = Type.EmptyTypes;
                }

                // CASE 1: The Provider is inside a Generic Class (e.g. class Provider<T> { static Method() } )
                if( method.DeclaringType.IsGenericTypeDefinition )
                {
                    // We must close the declaring type with the generic args
                    Type closedProviderType = method.DeclaringType.MakeGenericType( genericArgs );

                    // We must find the matching method on the closed type. 
                    // MethodBase.GetMethodFromHandle is the most robust way to map Open Method -> Closed Method
                    method = (MethodInfo)MethodBase.GetMethodFromHandle( method.MethodHandle, closedProviderType.TypeHandle );
                }
                // CASE 2: The Provider Method itself is Generic (e.g. static Method<T>() )
                else if( method.IsGenericMethodDefinition )
                {
                    // Safety check: ensure generic args match method definition count
                    if( method.GetGenericArguments().Length == genericArgs.Length )
                    {
                        method = method.MakeGenericMethod( genericArgs );
                    }
                    else if( method.GetGenericArguments().Length == 1 && genericArgs.Length == 0 )
                    {
                        // Special case: Method<T> called for non-generic type (e.g. MapsAnyClass -> T is the type itself)
                        method = method.MakeGenericMethod( targetType );
                    }
                }

                // Inject Context if requested
                ParameterInfo[] paramsInfo = method.GetParameters();
                object[] args = null;

                if( paramsInfo.Length == 1 && paramsInfo[0].ParameterType == typeof( ContextKey ) )
                {
                    args = new object[] { context };
                }

                return (IDescriptor)method.Invoke( null, args );
            }
            catch( Exception ex )
            {
                Debug.LogError( $"Failed to invoke provider '{method.Name}' for type '{targetType}': {ex}" );
                return null;
            }
        }
    }
}