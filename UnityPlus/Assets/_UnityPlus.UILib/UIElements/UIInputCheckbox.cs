using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// An input element that has a cycle of values to toggle between. The sprite changes for each value.
    /// </summary>
    /// <typeparam name="TValue">The type of the outputted value.</typeparam>
    public partial class UIInputCyclical<TValue> : UIElement, IUIInputElement<TValue>, IUIElementChild, IPointerClickHandler
    {
        protected Image imageComponent;

        public IUIElementContainer Parent { get; set; }

        protected (TValue v, Sprite sprite)[] options;

        protected int current;


        public event Action<IUIInputElement<TValue>.ValueChangedEventData> OnValueChanged;

        public bool TryGetValue( out TValue value )
        {
            value = options[current].v;
            return true;
        }

        public void CycleForwards( TValue value )
        {
            current = (current + 1) % options.Length;
            RefreshVisual();
        }
        
        public void CycleBackwards( TValue value )
        {
            current = (current - 1) % options.Length;
            RefreshVisual();
        }

        protected virtual void RefreshVisual()
        {
            imageComponent.sprite = options[current].sprite;
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