﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib
{
    /// <summary>
    /// Performs UI layout updates.
    /// </summary>
    public sealed class UILayoutManager : SingletonMonoBehaviour<UILayoutManager>
    {
        private UILayoutManager GetInstanceWithErrorMessage()
        {
            // An instance is *REQUIRED* to exist.
            try
            {
                return instance;
            }
            catch( InvalidOperationException ex )
            {
                throw new InvalidOperationException( $"A {nameof( UILayoutManager )} is required for UILib layout updates. Make sure that exactly 1 {nameof( UILayoutManager )} exists in the scene." );
            }
            catch( Exception ex )
            {
                throw ex;
            }
        }

        static bool _isLayoutStale = false;

        void LateUpdate()
        {
            if( _isLayoutStale )
            {
                // TODO - rebuild.
                // Cascade the rebuild from all UICanvases.

                _isLayoutStale = false;
            }
        }

        private static void BroadcastLayoutUpdateRecursive( HashSet<IUIElement> alreadyUpdated, IUIElement current )
        {
            // TODO - this can be optimized:
            // - Mark as stale when something changes and redraw only once in lateupdate.
            // a set of objects on which broadcastlayoutupdate was called. then call once per object.
            // this needs to be turned into a monobehaviour for that to work as well.

            // ----

            // Update the children first (because the other dimensions of the changed UI element might depend on them).
            // - E.g. if the width of a vertical layout elem is changed, the text in children might get taller or shorter to fit the new width.
            if( current is IUIElementContainer elemContainer )
            {
                foreach( var child in elemContainer.Children )
                {
                    if( !alreadyUpdated.Contains( child ) )
                    {
                        alreadyUpdated.Add( child );
                        BroadcastLayoutUpdateRecursive( alreadyUpdated, child );
                    }
                }
            }

            // Update our element (self).
            if( current is IUILayoutDriven ld )
            {
                ld.LayoutDriver?.DoLayout( ld );
            }
            else if( current is IUILayoutSelf ls )
            {
                ls.DoLayout();
            }

            // Notify the parent, so it can re-layout itself if the dimensions of our element have changed.
            if( current is IUIElementChild elemChild )
            {
                if( !alreadyUpdated.Contains( elemChild.Parent ) )
                {
                    alreadyUpdated.Add( elemChild.Parent );
                    BroadcastLayoutUpdateRecursive( alreadyUpdated, elemChild.Parent );
                }
            }
        }

        /// <summary>
        /// Forces an immediate layout update starting with the specified element.
        /// </summary>
        /// <param name="startingElement">The element that'll update first. The update will cascade from there.</param>
        public static void ForceLayoutUpdate( IUIElement startingElement )
        {
            BroadcastLayoutUpdateRecursive( new HashSet<IUIElement>(), startingElement );
        }
    }
}