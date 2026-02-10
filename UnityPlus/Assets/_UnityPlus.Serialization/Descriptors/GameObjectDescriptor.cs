
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public class GameObjectDescriptor : CompositeDescriptor
    {
        public override Type MappedType => typeof( GameObject );

        // Steps:
        // 0: Name
        // 1: Layer
        // 2: Tag
        // 3: IsStatic
        // 4: Components (Sequence)
        // 5: Children (Sequence)
        // 6: Active (Applied last to trigger Awake/OnEnable after full population)
        public override int GetStepCount( object target ) => 7;

        public override IMemberInfo GetMemberInfo( int stepIndex, object target )
        {
            switch( stepIndex )
            {
                case 0: return new PropertyMember( "name", typeof( string ), ( t ) => ((GameObject)t).name, ( ref object t, object v ) => ((GameObject)t).name = (string)v );
                case 1: return new PropertyMember( "layer", typeof( int ), ( t ) => ((GameObject)t).layer, ( ref object t, object v ) => ((GameObject)t).layer = (int)v );
                case 2: return new PropertyMember( "tag", typeof( string ), ( t ) => ((GameObject)t).tag, ( ref object t, object v ) => ((GameObject)t).tag = (string)v );
                case 3: return new PropertyMember( "isStatic", typeof( bool ), ( t ) => ((GameObject)t).isStatic, ( ref object t, object v ) => ((GameObject)t).isStatic = (bool)v );

                // Virtual Containers
                case 4: return new VirtualListMember( KeyNames.COMPONENTS, target, new ComponentSequenceDescriptor() );
                case 5: return new VirtualListMember( KeyNames.CHILDREN, target, new ChildSequenceDescriptor() );

                // Activation (Must be last)
                case 6: return new PropertyMember( "active", typeof( bool ), ( t ) => ((GameObject)t).activeSelf, ( ref object t, object v ) => ((GameObject)t).SetActive( (bool)v ) );
            }
            throw new IndexOutOfRangeException();
        }

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            // Pre-scan for RectTransform to determine creation method
            bool hasRectTransform = false;
            if( data is SerializedObject objScan && objScan.TryGetValue( KeyNames.COMPONENTS, out var compDataScan ) && compDataScan is SerializedArray compArrScan )
            {
                foreach( var cNode in compArrScan )
                {
                    if( cNode is SerializedObject cObj && cObj.TryGetValue( KeyNames.TYPE, out var typeVal ) )
                    {
                        string typeName = (string)(SerializedPrimitive)typeVal;
                        if( typeName.Contains( "RectTransform" ) ) // String check is faster/safer than loading type if assembly agnostic
                        {
                            hasRectTransform = true;
                            break;
                        }
                    }
                }
            }

            var go = hasRectTransform
                ? new GameObject( "New Game Object", typeof( RectTransform ) )
                : new GameObject();

            go.SetActive( false ); // Ensure it's inactive during population

            // Pre-instantiate components based on data types
            if( data is SerializedObject obj && obj.TryGetValue( KeyNames.COMPONENTS, out var compData ) && compData is SerializedArray compArr )
            {
                foreach( var cNode in compArr )
                {
#warning TODO - use the v3 extension method for types instead.
                    if( cNode is SerializedObject cObj && cObj.TryGetValue( KeyNames.TYPE, out var typeVal ) )
                    {
                        string typeName = (string)(SerializedPrimitive)typeVal;
                        Type type = ctx.TypeResolver.ResolveType( typeName );

                        if( type != null )
                        {
                            // Don't add duplicates of Transform/RectTransform which are auto-created
                            if( go.GetComponent( type ) == null )
                            {
                                go.AddComponent( type );
                            }
                        }
                    }
                }
            }

            return go;
        }

        // --- Helpers ---

        private struct PropertyMember : IMemberInfo
        {
            public string Name { get; }
            public readonly int Index => -1;
            public Type MemberType { get; }
            public IDescriptor TypeDescriptor { get; }
            public readonly bool RequiresWriteBack => false;

            private Func<object, object> _getter;
            private RefSetter<object, object> _setter;

            public PropertyMember( string name, Type type, Func<object, object> getter, RefSetter<object, object> setter )
            {
                Name = name;
                MemberType = type;
                TypeDescriptor = TypeDescriptorRegistry.GetDescriptor( type );
                _getter = getter;
                _setter = setter;
            }

            public object GetValue( object target ) => _getter( target );
            public void SetValue( ref object target, object value ) => _setter( ref target, value );
        }

        private struct VirtualListMember : IMemberInfo
        {
            public string Name { get; }
            public int Index => -1;
            public Type MemberType => typeof( GameObject ); // It wraps the GO
            public IDescriptor TypeDescriptor { get; }
            public bool RequiresWriteBack => false;

            private object _target;

            public VirtualListMember( string name, object target, IDescriptor descriptor )
            {
                Name = name;
                _target = target;
                TypeDescriptor = descriptor;
            }

            public object GetValue( object target ) => _target; // Pass the GameObject through to the SequenceDescriptor
            public void SetValue( ref object target, object value ) { /* No-op, list is modified in-place */ }
        }
    }

    /// <summary>
    /// Iterates over components on a GameObject.
    /// </summary>
    public class ComponentSequenceDescriptor : CompositeDescriptor
    {
        public override Type MappedType => typeof( GameObject );

        public override int GetStepCount( object target )
        {
            // During serialization, we use the actual component count.
            // During deserialization, the StackMachine uses the DataNode array length, so this value is ignored by the Driver logic 
            // (Driver uses PopulationStepCount derived from DataNode for collections usually, but since this is a Composite acting as a collection, 
            // we need to be careful).
            // Actually, for Composite types, the Driver uses GetStepCount().
            // So we must return the count of the components currently on the object (which we pre-instantiated in GameObjectDescriptor).
            return ((GameObject)target).GetComponents<Component>().Length;
        }

        public override IMemberInfo GetMemberInfo( int stepIndex, object target )
        {
            var components = ((GameObject)target).GetComponents<Component>();
            if( stepIndex < components.Length )
            {
                return new InstanceMemberInfo( stepIndex, components[stepIndex] );
            }
            return null;
        }

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            // Should not be called directly, as this is accessed via an existing GameObject instance
            return null;
        }

        private struct InstanceMemberInfo : IMemberInfo
        {
            public string Name => null; // Array element
            public int Index => _index;
            public Type MemberType { get; }
            public IDescriptor TypeDescriptor { get; }
            public bool RequiresWriteBack => false;

            private int _index;
            private object _instance;

            public InstanceMemberInfo( int index, Component instance )
            {
                _index = index;
                _instance = instance;
                MemberType = instance.GetType();
                TypeDescriptor = TypeDescriptorRegistry.GetDescriptor( MemberType );
            }

            public object GetValue( object target ) => _instance;
            public void SetValue( ref object target, object value ) { }
        }
    }

    /// <summary>
    /// Iterates over children of a GameObject.
    /// </summary>
    public class ChildSequenceDescriptor : CollectionDescriptor
    {
        public override Type MappedType => typeof( GameObject );

        // We act as a collection of GameObjects
        public override int GetStepCount( object target )
        {
            return ((GameObject)target).transform.childCount;
        }

        public override object Resize( object target, int newSize )
        {
            // We can't resize child count arbitrarily without creating objects.
            // The StackMachine will drive creation via CreateInitialTarget of the children.
            return target;
        }

        public override IMemberInfo GetMemberInfo( int stepIndex, object target )
        {
            Transform t = ((GameObject)target).transform;
            GameObject child = stepIndex < t.childCount ? t.GetChild( stepIndex ).gameObject : null;

            return new ChildMemberInfo( stepIndex, child );
        }

        public override object CreateInitialTarget( SerializedData data, SerializationContext ctx )
        {
            return null; // Should not be called
        }

        private struct ChildMemberInfo : IMemberInfo
        {
            public string Name => null;
            public int Index => _index;
            public Type MemberType => typeof( GameObject );
            public IDescriptor TypeDescriptor => TypeDescriptorRegistry.GetDescriptor( typeof( GameObject ) );
            public bool RequiresWriteBack => false;

            private int _index;
            private GameObject _child;

            public ChildMemberInfo( int index, GameObject child )
            {
                _index = index;
                _child = child;
            }

            public object GetValue( object target ) => _child;

            public void SetValue( ref object target, object value )
            {
                // When a child is fully deserialized (or created), we parent it.
                if( value is GameObject childGO && target is GameObject parentGO )
                {
                    childGO.transform.SetParent( parentGO.transform, false );
                }
            }
        }
    }
}