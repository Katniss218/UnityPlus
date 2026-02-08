
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class TypeDescriptorRegistry
    {
        // Key is (Type, Context)
        private static readonly Dictionary<(Type, int), ITypeDescriptor> _descriptors = new Dictionary<(Type, int), ITypeDescriptor>();

        // Maps Context ID -> Open Generic Type Definition (e.g. ReferenceDescriptor<>)
        private static readonly Dictionary<int, Type> _genericContextFactories = new Dictionary<int, Type>();

        // Context Mappings
        private static readonly Dictionary<int, int> _collectionContextMap = new Dictionary<int, int>();
        private static readonly Dictionary<int, (int keyCtx, int valCtx)> _dictionaryContextMap = new Dictionary<int, (int, int)>();

        // Extensions: (TargetType, Context) -> List of Extension Methods
        private static readonly Dictionary<(Type, int), List<MethodInfo>> _extensions = new Dictionary<(Type, int), List<MethodInfo>>();

        private static bool _isInitialized = false;

        private static void Initialize()
        {
            if( _isInitialized ) return;

            // Force initialization of context constants to ensure ContextRegistry is populated
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

                // 1. Scan for Context Mapping Attributes
                foreach( var attr in assembly.GetCustomAttributes<MapCollectionContextAttribute>() )
                {
                    _collectionContextMap[attr.ContainerContext] = attr.ElementContext;
                }
                foreach( var attr in assembly.GetCustomAttributes<MapDictionaryContextAttribute>() )
                {
                    _dictionaryContextMap[attr.ContainerContext] = (attr.KeyContext, attr.ValueContext);
                }

                // 2. Scan for Types
                foreach( var type in assembly.GetTypes() )
                {
                    // Scan methods for ExtendsSerializationOfAttribute
                    var methods = type.GetMethods( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );
                    foreach( var method in methods )
                    {
                        var attributes = method.GetCustomAttributes<ExtendsSerializationOfAttribute>( false );
                        foreach( var attr in attributes )
                        {
                            var key = (attr.TargetType, attr.Context);
                            if( !_extensions.ContainsKey( key ) )
                                _extensions[key] = new List<MethodInfo>();

                            _extensions[key].Add( method );
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
                            if( typeof( ITypeDescriptor ).IsAssignableFrom( type ) )
                            {
                                _genericContextFactories[attr.Context] = type;
                            }
                        }
                        continue;
                    }

                    // Concrete Descriptors
                    if( typeof( ITypeDescriptor ).IsAssignableFrom( type ) && !type.ContainsGenericParameters )
                    {
                        try
                        {
                            var ctor = type.GetConstructor( Type.EmptyTypes );
                            if( ctor != null )
                            {
                                var descriptor = (ITypeDescriptor)Activator.CreateInstance( type );
                                Register( descriptor ); // This will just register it, extensions applied on retrieval
                            }
                        }
                        catch( Exception ex )
                        {
                            Debug.LogWarning( $"Failed to instantiate discovered descriptor {type.Name}: {ex.Message}" );
                        }
                    }
                }
            }

            _isInitialized = true;
        }

        public static void Register( ITypeDescriptor descriptor, int context = 0 )
        {
            if( descriptor == null ) return;
            _descriptors[(descriptor.WrappedType, context)] = descriptor;
        }

        // --- Context Mapping Registration API ---

        public static void RegisterCollectionContext( int containerContext, int elementContext )
        {
            _collectionContextMap[containerContext] = elementContext;
        }

        public static void RegisterDictionaryContext( int containerContext, int keyContext, int valueContext )
        {
            _dictionaryContextMap[containerContext] = (keyContext, valueContext);
        }

        public static int GetCollectionElementContext( int containerContext )
        {
            if( !_isInitialized ) Initialize();
            return _collectionContextMap.TryGetValue( containerContext, out int ctx ) ? ctx : containerContext;
        }

        public static (int keyCtx, int valCtx) GetDictionaryElementContexts( int containerContext )
        {
            if( !_isInitialized ) Initialize();
            return _dictionaryContextMap.TryGetValue( containerContext, out var ctx ) ? ctx : (ObjectContext.Default, ObjectContext.Default);
        }

        // ----------------------------------------

        public static ITypeDescriptor GetDescriptor( Type type, int context = 0 )
        {
            if( type == null ) return null;

            if( !_isInitialized )
                Initialize();

            if( _descriptors.TryGetValue( (type, context), out var descriptor ) )
            {
                // We assume extensions are applied upon initial creation/registration.
                // If it's in the cache, it's ready.
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

        private static void ApplyExtensions( ITypeDescriptor descriptor, Type type, int context )
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

        private static ITypeDescriptor CreateJITDescriptor( Type type, int context )
        {
            // --- 1. Dynamic Single Object Contexts ---
            if( _genericContextFactories.TryGetValue( context, out Type openGenericType ) )
            {
                try
                {
                    Type closedType = openGenericType.MakeGenericType( type );
                    return (ITypeDescriptor)Activator.CreateInstance( closedType );
                }
                catch( ArgumentException )
                {
                    return null;
                }
            }

            // --- 2. Arrays ---
            if( type.IsArray && type.GetArrayRank() == 1 )
            {
                Type elementType = type.GetElementType();
                Type descType = typeof( ArrayDescriptor<> ).MakeGenericType( elementType );
                var desc = (ICollectionDescriptorWithContext)Activator.CreateInstance( descType );

                desc.ElementContext = GetCollectionElementContext( context );

                return desc;
            }

            // --- 3. Lists ---
            if( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( List<> ) )
            {
                Type elementType = type.GetGenericArguments()[0];
                Type descType = typeof( ListDescriptor<> ).MakeGenericType( elementType );
                var desc = (ICollectionDescriptorWithContext)Activator.CreateInstance( descType );

                desc.ElementContext = GetCollectionElementContext( context );

                return desc;
            }

            // --- 4. Dictionaries ---
            if( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Dictionary<,> ) )
            {
                Type[] args = type.GetGenericArguments();
                Type keyType = args[0];
                Type valueType = args[1];

                // Create DictionaryDescriptor<TDict, TKey, TValue>
                Type descType = typeof( DictionaryDescriptor<,,> ).MakeGenericType( type, keyType, valueType );
                var desc = (IDictionaryDescriptor)Activator.CreateInstance( descType );

                var (keyCtx, valCtx) = GetDictionaryElementContexts( context );
                desc.KeyContext = keyCtx;
                desc.ValueContext = valCtx;

                return (ITypeDescriptor)desc;
            }

            // --- 5. Classes / Structs ---
            if( type.IsClass || type.IsValueType )
            {
                Type descType = typeof( ReflectionClassDescriptor<> ).MakeGenericType( type );
                return (ITypeDescriptor)Activator.CreateInstance( descType );
            }

            return null;
        }

        public interface ICollectionDescriptorWithContext : ITypeDescriptor
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
                if( !typeof( ScriptableObject ).IsAssignableFrom( typeof( T ) ) && !typeof( Component ).IsAssignableFrom( typeof( T ) ) )
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
                        _onSerializing = ( obj ) => method.Invoke( obj, new object[] { default( StreamingContext ) } ); // Not optimized but functional

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