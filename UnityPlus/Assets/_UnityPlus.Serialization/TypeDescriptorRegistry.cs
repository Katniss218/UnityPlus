
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class TypeDescriptorRegistry
    {
        // Cache: (Type, Context) -> Descriptor
        private static readonly Dictionary<(Type, int), IDescriptor> _descriptors = new Dictionary<(Type, int), IDescriptor>();

        // Maps Context ID -> Open Generic Type Definition (e.g. ReferenceDescriptor<>)
        private static readonly Dictionary<int, Type> _genericContextFactories = new Dictionary<int, Type>();

        // Provider Lookups (v3 Style)
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

            // Force initialization of context constants to ensure ContextRegistry is populated with legacy constants
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor( typeof( ObjectContext ).TypeHandle );
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor( typeof( ArrayContext ).TypeHandle );
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor( typeof( KeyValueContext ).TypeHandle );

            // Scan all assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach( var assembly in assemblies )
            {
                string name = assembly.GetName().Name;
                if( name.StartsWith( "System" ) || name.StartsWith( "mscorlib" ) || name.StartsWith( "UnityEditor" ) )
                    continue;

                // 2. Scan for Types and Methods
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

                        // Mapping Providers (V3 Style - Generalized)
                        var providerAttributes = method.GetCustomAttributes<MappingProviderAttribute>( false );
                        foreach( var attr in providerAttributes )
                        {
                            if( !typeof( IDescriptor ).IsAssignableFrom( method.ReturnType ) ) continue;

                            foreach( var ctx in attr.Contexts )
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

                    if( type.IsAbstract || type.IsInterface )
                        continue;

                    // Generic Context Factories
                    if( type.IsGenericTypeDefinition )
                    {
                        var contextAttributes = type.GetCustomAttributes<TypeDescriptorContextAttribute>( inherit: false );
                        foreach( var attr in contextAttributes )
                        {
                            if( typeof( IDescriptor ).IsAssignableFrom( type ) )
                            {
                                _genericContextFactories[attr.Context] = type;
                            }
                        }
                        continue;
                    }

                    // Concrete Descriptors (V4 Class Style)
                    if( typeof( IDescriptor ).IsAssignableFrom( type ) && !type.ContainsGenericParameters )
                    {
                        try
                        {
                            var ctor = type.GetConstructor( Type.EmptyTypes );
                            if( ctor != null )
                            {
                                var descriptor = (IDescriptor)Activator.CreateInstance( type );
                                Register( descriptor ); // This will just register it, extensions applied on retrieval
                            }
                        }
                        catch { /* Ignore instantiation failures */ }
                    }
                }
            }

            _isInitialized = true;
        }

        public static void Register( IDescriptor descriptor, int context = 0 )
        {
            if( descriptor == null ) return;
            _descriptors[(descriptor.WrappedType, context)] = descriptor;
        }

        public static IDescriptor GetDescriptor( Type type, int context = 0 )
        {
            if( type == null ) return null;

            if( !_isInitialized )
                Initialize();

            if( _descriptors.TryGetValue( (type, context), out var descriptor ) )
            {
                return descriptor;
            }

            descriptor = CreateJITDescriptor( type, context );
            if( descriptor != null )
            {
                // Apply Extensions immediately before caching
                ApplyExtensions( descriptor, type, context );
                Register( descriptor, context );
            }

            return descriptor;
        }

        /// <summary>
        /// Clears all cached descriptors and reflection data.
        /// Forces a full re-initialization (assembly scanning) on the next call to GetDescriptor.
        /// Use this in Unit Test TearDown to ensure a clean state.
        /// </summary>
        public static void Clear()
        {
            _descriptors.Clear();
            _genericContextFactories.Clear();

            _inheritingProviders.Clear();
            _implementingProviders.Clear();
            _anyClassProviders.Clear();
            _anyStructProviders.Clear();
            _anyInterfaceProviders.Clear();
            _anyProviders.Clear();

            _extensions.Clear();

            _isInitialized = false;
        }

        private static void ApplyExtensions( IDescriptor descriptor, Type type, int context )
        {
            if( _extensions.TryGetValue( (type, context), out var methods ) )
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

        private static IDescriptor CreateJITDescriptor( Type type, int context )
        {
            // --- 1. Dynamic Single Object Contexts (Asset/Reference) ---
            if( _genericContextFactories.TryGetValue( context, out Type openGenericType ) )
            {
                try
                {
                    Type closedType = openGenericType.MakeGenericType( type );
                    return (IDescriptor)Activator.CreateInstance( closedType );
                }
                catch { return null; }
            }

            // --- 2. Provider Attributes (Inheritance) ---
            Type checkType = type;
            while( checkType != null )
            {
                if( _inheritingProviders.TryGetValue( (context, checkType), out var method ) )
                    return InvokeProvider( method, type );

                if( checkType.IsGenericType && !checkType.IsGenericTypeDefinition )
                {
                    if( _inheritingProviders.TryGetValue( (context, checkType.GetGenericTypeDefinition()), out method ) )
                        return InvokeProvider( method, type );
                }

                checkType = checkType.BaseType;
            }

            // --- 3. Provider Attributes (Interfaces) ---
            foreach( var iface in type.GetInterfaces() )
            {
                if( _implementingProviders.TryGetValue( (context, iface), out var method ) )
                    return InvokeProvider( method, type );

                if( iface.IsGenericType && !iface.IsGenericTypeDefinition )
                {
                    if( _implementingProviders.TryGetValue( (context, iface.GetGenericTypeDefinition()), out method ) )
                        return InvokeProvider( method, type );
                }
            }

            // --- 4. Provider Attributes (Category) ---
            if( type.IsClass && _anyClassProviders.TryGetValue( context, out var classMethod ) )
                return InvokeProvider( classMethod, type );
            if( type.IsValueType && _anyStructProviders.TryGetValue( context, out var structMethod ) )
                return InvokeProvider( structMethod, type );
            if( type.IsInterface && _anyInterfaceProviders.TryGetValue( context, out var ifaceMethod ) )
                return InvokeProvider( ifaceMethod, type );
            if( _anyProviders.TryGetValue( context, out var anyMethod ) )
                return InvokeProvider( anyMethod, type );

            // --- 5. Built-in Generics (Fallbacks) ---
            // Arrays
            if( type.IsArray && type.GetArrayRank() == 1 )
            {
                Type elementType = type.GetElementType();
                Type descType = typeof( ArrayDescriptor<> ).MakeGenericType( elementType );
                var desc = (ICollectionDescriptorWithContext)Activator.CreateInstance( descType );
                desc.ElementContext = ContextRegistry.GetCollectionElementContext( context );
                return desc;
            }

            // Lists
            if( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( List<> ) )
            {
                Type elementType = type.GetGenericArguments()[0];
                Type descType = typeof( ListDescriptor<> ).MakeGenericType( elementType );
                var desc = (ICollectionDescriptorWithContext)Activator.CreateInstance( descType );
                desc.ElementContext = ContextRegistry.GetCollectionElementContext( context );
                return desc;
            }

            // Dictionaries
            if( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Dictionary<,> ) )
            {
                Type[] args = type.GetGenericArguments();
                Type descType = typeof( DictionaryDescriptor<,,> ).MakeGenericType( type, args[0], args[1] );
                var desc = (IDictionaryDescriptor)Activator.CreateInstance( descType );
                var (keyCtx, valCtx) = ContextRegistry.GetDictionaryElementContexts( context );
                desc.KeyContext = keyCtx;
                desc.ValueContext = valCtx;
                return (IDescriptor)desc;
            }

            // Enums
            if( type.IsEnum )
            {
                Type descType = typeof( EnumDescriptor<> ).MakeGenericType( type );
                return (IDescriptor)Activator.CreateInstance( descType );
            }

            // --- 6. Reflection Fallback ---
            // Handles Classes, Structs, and Interfaces (via polymorphism)
            if( type.IsClass || type.IsValueType || type.IsInterface )
            {
                // For interfaces, we create a descriptor based on the interface type itself.
                // It will likely have no members (unless we scan interface properties, which ReflectionClassDescriptor does),
                // but it allows the StackMachine to start and then resolve the actual type via $type.
                Type descType = typeof( ReflectionClassDescriptor<> ).MakeGenericType( type );
                return (IDescriptor)Activator.CreateInstance( descType );
            }

            return null;
        }

        private static IDescriptor InvokeProvider( MethodInfo method, Type targetType )
        {
            try
            {
                if( method.IsGenericMethodDefinition )
                {
                    Type[] genArgs;
                    if( targetType.IsArray )
                        genArgs = new[] { targetType.GetElementType() };
                    else if( targetType.IsGenericType && targetType.GetGenericArguments().Length == method.GetGenericArguments().Length )
                        genArgs = targetType.GetGenericArguments();
                    else
                        genArgs = new[] { targetType };

                    method = method.MakeGenericMethod( genArgs );
                }

                return (IDescriptor)method.Invoke( null, null );
            }
            catch( Exception ex )
            {
                Debug.LogError( $"Failed to invoke provider '{method.Name}' for type '{targetType}': {ex}" );
                return null;
            }
        }

        public interface ICollectionDescriptorWithContext : IDescriptor
        {
            int ElementContext { get; set; }
        }

        private class ReflectionClassDescriptor<T> : CompositeDescriptor
        {
            public override Type WrappedType => typeof( T );
            private readonly IMemberInfo[] _members;
            private readonly IMethodInfo[] _methods;
            private readonly Func<T> _constructor;

            // Lifecycle
            private readonly bool _implementsUnityCallback;
            private readonly Action<object> _onSerializing;
            private readonly Action<object> _onDeserialized;

            public ReflectionClassDescriptor()
            {
                // Constructor Optimization
                if( !typeof( T ).IsInterface && !typeof( ScriptableObject ).IsAssignableFrom( typeof( T ) ) && !typeof( Component ).IsAssignableFrom( typeof( T ) ) )
                {
                    try
                    {
                        var ctor = typeof( T ).GetConstructor( Type.EmptyTypes );
                        if( ctor != null || typeof( T ).IsValueType )
                        {
                            var newExp = Expression.New( typeof( T ) );
                            _constructor = Expression.Lambda<Func<T>>( newExp ).Compile();
                        }
                    }
                    catch { /* Fallback or ignore */ }
                }

                // Fields
                var fields = typeof( T ).GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
                var memberList = new List<IMemberInfo>();

                foreach( var field in fields )
                {
                    if( field.IsStatic ) continue;
                    bool isPublic = field.IsPublic;
                    bool hasSerializeField = field.GetCustomAttribute<SerializeField>() != null;
                    bool hasNonSerialized = field.GetCustomAttribute<NonSerializedAttribute>() != null;

                    if( hasNonSerialized ) continue;
                    if( !isPublic && !hasSerializeField ) continue;

                    memberList.Add( new ReflectionFieldInfo( field ) );
                }
                _members = memberList.ToArray();

                // Methods (Inspector support)
                var methods = typeof( T ).GetMethods( BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static );
                var methodList = new List<IMethodInfo>();

                foreach( var method in methods )
                {
                    // Scan for Serialization Callbacks
                    if( method.GetCustomAttribute<OnSerializingAttribute>() != null && method.GetParameters().Length == 1 )
                        _onSerializing = ( obj ) => method.Invoke( obj, new object[] { default( StreamingContext ) } );

                    if( method.GetCustomAttribute<OnDeserializedAttribute>() != null && method.GetParameters().Length == 1 )
                        _onDeserialized = ( obj ) => method.Invoke( obj, new object[] { default( StreamingContext ) } );

                    // Scan for Inspector Methods
                    if( method.IsSpecialName ) continue;
                    if( method.DeclaringType == typeof( object ) || method.DeclaringType == typeof( Component ) || method.DeclaringType == typeof( MonoBehaviour ) ) continue;

                    methodList.Add( new ReflectionMethodInfo( method ) );
                }
                _methods = methodList.ToArray();

                _implementsUnityCallback = typeof( ISerializationCallbackReceiver ).IsAssignableFrom( typeof( T ) );
            }

            public override int GetStepCount( object target ) => _members.Length;
            public override IMemberInfo GetMemberInfo( int stepIndex, object target ) => _members[stepIndex];

            public override int GetMethodCount() => _methods.Length;
            public override IMethodInfo GetMethodInfo( int methodIndex ) => _methods[methodIndex];

            public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
            {
                if( typeof( T ).IsInterface ) return null;

                if( typeof( ScriptableObject ).IsAssignableFrom( typeof( T ) ) )
                    return ScriptableObject.CreateInstance( typeof( T ) );

                if( _constructor != null )
                    return _constructor();

                try { return Activator.CreateInstance<T>(); }
                catch { return null; }
            }

            public override void OnSerializing( object target, SerializationContext context )
            {
                if( _implementsUnityCallback ) ((ISerializationCallbackReceiver)target).OnBeforeSerialize();
                _onSerializing?.Invoke( target );
            }

            public override void OnDeserialized( object target, SerializationContext context )
            {
                if( _implementsUnityCallback ) ((ISerializationCallbackReceiver)target).OnAfterDeserialize();
                _onDeserialized?.Invoke( target );
            }
        }
    }
}
