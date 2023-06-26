using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.StaticEvents
{
    public interface IOverridable
    {
        Type Overrides { get; } // typeof(SomeEventProvider)
    }

    /// <summary>
    /// A static overrridable listener.
    /// </summary>
    public interface IOverridableEventListener : IOverridable
    {
        void Invoke();
    }

    public interface IOverridableEventListener<T> : IOverridable
    {
        void Invoke( ref T instance );
    }

    public static class IOverridableEx
    {
        public static Comparison<IOverridable> OverridableComparison = ( x, y ) =>
        {
            // <0 => x goes first
            // =0 => both equal
            // >0 => y goes first

#warning TODO - test this
            Type xType = x.GetType();
            Type yOverrides = y.Overrides;

            Type yType = y.GetType();
            Type xOverrides = x.Overrides;

            // Whatever (x or y) is overriden by the other should go last.
            // If nothing is overriden, they stay in the order they were in.

            if( xOverrides != yType || yOverrides == xType )
            {
                return 1;
            }

            if( xOverrides == yType || yOverrides != xType )
            {
                return -1;
            }

            return 0;
        };

        public static void SortOverride( this List<IOverridable> funcs )
        {
            funcs.Sort( OverridableComparison );
        }
    }
}