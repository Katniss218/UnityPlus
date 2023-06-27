using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.StaticEvents
{
    /// <summary>
    /// Manages static game events.
    /// </summary>
    public class StaticEventManager
    {
        class Event
        {
            public Event( string id )
            {
                this._id = id;
            }

            string _id;

            Dictionary<string, Action<object>> _listeners = new Dictionary<string, Action<object>>();
            HashSet<string> _blacklist = new HashSet<string>();

            public bool TryAddListener( OverridableEventListener<Action<object>> listener )
            {
                if( _listeners.ContainsKey( listener.id ) )
                {
                    return false;
                }

                // Allows a mod to block itself (!), but I don't think that's a problem.
                foreach( string blockedId in listener.blacklist )
                {
                    _blacklist.Add( blockedId );
                }

                // Remove listners that are on the new blacklist.
                foreach( string blockedId in listener.blacklist )
                {
                    if( _listeners.Remove( blockedId ) )
                    {
                        Debug.Log( $"StaticEventManager: `{_id}`: Listener `{blockedId}` was blocked by `{listener.id}`." );
                    }
                }

                if( _blacklist.Contains( listener.id ) )
                {
                    Debug.Log( $"StaticEventManager: `{_id}`: Listener `{listener.id}` is blocked." );
                    return false;
                }

                _listeners.Add( listener.id, listener.func );
                return true;
            }

            public bool TryInvoke( object obj )
            {
                foreach( var listener in _listeners.Values )
                {
                    try
                    {
                        listener( obj );
                    }
                    catch( Exception ex )
                    {
                        Debug.LogException( ex );
                    }
                }
                return true;
            }
        }

        // You can't remove listeners. This is by design.

        Dictionary<string, Event> _events;

        /// <summary>
        /// Tries to create an event with the given ID.
        /// </summary>
        /// <returns>True if the event was created, otherwise false (including if the event was added before).</returns>
        public bool TryCreate( string eventId )
        {
            if( eventId == null )
            {
                throw new ArgumentNullException( nameof( eventId ), $"Event ID can't be null." );
            }

            if( _events.ContainsKey( eventId ) )
            {
                return false;
            }

            _events.Add( eventId, new Event( eventId ) );
            return true;
        }

        /// <summary>
        /// Checks if the event with a given ID exists.
        /// </summary>
        public bool Exists( string eventId )
        {
            if( eventId == null )
            {
                throw new ArgumentNullException( nameof( eventId ), $"Event ID can't be null." );
            }

            return _events.ContainsKey( eventId );
        }

        /// <summary>
        /// Tries to adds the listener and returns
        /// </summary>
        /// <param name="eventId">The event ID to add the listener to.</param>
        /// <param name="listener">The listener to add.</param>
        /// <returns>False if the listener id was already added, or if the event doesn't exist. Otherwise true.</returns>
        public bool TryAddListener( string eventId, OverridableEventListener<Action<object>> listener )
        {
            if( eventId == null )
            {
                throw new ArgumentNullException( nameof( eventId ), $"Event ID can't be null." );
            }

            if( _events.TryGetValue( eventId, out Event @event ) )
            {
                return @event.TryAddListener( listener );
            }

            return false; // unknown event ID.
        }

        /// <summary>
        /// Safely invokes each non-blocked listener of a specific event one-by-one.
        /// </summary>
        /// <param name="eventId">The event ID of the event to invoke.</param>
        /// <param name="obj">The object parameter to invoke the events with.</param>
        /// <returns>False if the event doesn't exist.</returns>
        public bool TryInvoke( string eventId, object obj = null )
        {
            if( eventId == null )
            {
                throw new ArgumentNullException( nameof( eventId ), $"Event ID can't be null." );
            }

            if( _events.TryGetValue( eventId, out Event @event ) )
            {
                return @event.TryInvoke( obj );
            }

            return false;
        }
    }
}