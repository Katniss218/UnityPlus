using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.StaticEvents
{
    public static class StaticEventManager
    {
        static Dictionary<string, List<IOverridableEventListener>> _reg = new Dictionary<string, List<IOverridableEventListener>>();

        public static void AddListener( string eventID, IOverridableEventListener listener )
        {

        }
    }

    public static class StaticEventManager<T>
    {
        static Dictionary<string, List<IOverridableEventListener<T>>> _reg = new Dictionary<string, List<IOverridableEventListener<T>>>();
    }



    internal class DefaultOverridableListener : IOverridableEventListener<GameObject>
    {
        public Type Overrides => null;

        public void Invoke( ref GameObject instance )
        {
            throw new NotImplementedException();
        }
    }

    internal class BetterOverridableListener : IOverridableEventListener<GameObject>
    {
        public Type Overrides => typeof( DefaultOverridableListener );

        public void Invoke( ref GameObject instance )
        {
            throw new NotImplementedException();
        }
    }
}