using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TValue">The type of the outputted value.</typeparam>
    public partial class UIInputRadio<TValue> : UIElement, IUIInputElement<TValue>, IUIElementChild, IPointerClickHandler
    {
        /// <summary>
        /// Stores a reference to every single radio, so they can work in tandem with each other.
        /// </summary>
        protected readonly static Dictionary<string, List<UIInputRadio<TValue>>> allRadios = new();

        /// <summary>
        /// Determines which radios this radio works with.
        /// </summary>
        protected string context;

        protected TValue value;

        private bool _isSelected;

        public event Action<IUIInputElement<TValue>.ValueChangedEventData> OnValueChanged;

        public IUIElementContainer Parent { get; set; }

        public bool TryGetValue( out TValue value )
        {
            if( allRadios.TryGetValue( context, out var list ) )
            {
                UIInputRadio<TValue> selectedRadio = list.FirstOrDefault( e => e._isSelected );
                if( selectedRadio != null )
                {
                    value = selectedRadio.value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        void OnEnable()
        {
            if( !allRadios.TryGetValue( context, out var list ) )
            {
                list = new List<UIInputRadio<TValue>>();
                allRadios.Add( context, list );
            }

            list.Add( this );
        }

        void OnDisable()
        {
            if( allRadios.TryGetValue( context, out var list ) )
            {
                list.Remove( this );
                if( list.Count == 0 )
                {
                    allRadios.Remove( context );
                }
            }
        }

        public void OnPointerClick( PointerEventData eventData )
        {
            throw new NotImplementedException();
        }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout ) where T : UIInputSlider<TValue>
        {
            throw new NotImplementedException();
        }
    }
}