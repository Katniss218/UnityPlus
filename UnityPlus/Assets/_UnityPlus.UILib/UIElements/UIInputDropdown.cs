using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TValue">The type of the outputted value.</typeparam>
    public partial class UIInputDropdown<TValue> : UIElement, IUIInputElement<TValue>, IUIElementChild, IPointerClickHandler
    {
        protected TMPro.TextMeshProUGUI textComponent;
        protected TMPro.TextMeshProUGUI placeholderComponent;
        protected Image backgroundComponent;

        public IUIElementContainer Parent { get; set; }

        public virtual string Placeholder { get => placeholderComponent.text; set => placeholderComponent.text = value; }

        public virtual Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        protected (string s, TValue v)[] options;
        protected int? selectedValue;

        protected UIContextMenu contextMenu;

        public event Action<IUIInputElement<TValue>.ValueChangedEventData> OnValueChanged;

        public void OnPointerClick( PointerEventData eventData )
        {
            // open a context menu with a scrollable list of options.
        }

        public bool TryGetValue( out TValue value )
        {
            if( selectedValue.HasValue )
            {
                value = options[selectedValue.Value].v;
                return true;
            }

            value = default;
            return false;
        }

        public void TrySelect( int index )
        {
            if( index < 0 || index >= options.Length )
                return;

            selectedValue = index;
            OnValueChanged?.Invoke( options[selectedValue.Value].v );
            RefreshVisual();
        }

        public void ClearValue()
        {
            selectedValue = null;
            OnValueChanged?.Invoke( default );
            RefreshVisual();
        }

        protected virtual void RefreshVisual()
        {
            textComponent.enabled = selectedValue.HasValue;
            if( selectedValue.HasValue )
            {
                textComponent.text = options[selectedValue.Value].s;
            }

            placeholderComponent.enabled = !selectedValue.HasValue;
        }

        protected internal static T Create<T>( IUIElementContainer parent, UICanvas contextMenuCanvas, UILayoutInfo layout, Sprite background ) where T : UIInputDropdown<TValue>
        {
            (GameObject rootGameObject, RectTransform rootTransform, T uiButton) = UIElement.CreateUIGameObject<T>( parent, $"uilib-{typeof( T ).Name}", layout );

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = true;
            backgroundComponent.sprite = background;
            backgroundComponent.type = Image.Type.Sliced;

            if( background == null )
            {
                backgroundComponent.color = new Color( 0, 0, 0, 0 );
            }

            Button buttonComponent = rootGameObject.AddComponent<Button>();
            buttonComponent.targetGraphic = backgroundComponent;
            buttonComponent.transition = Selectable.Transition.ColorTint;
            buttonComponent.colors = new ColorBlock()
            {
                normalColor = Color.white,
                selectedColor = Color.white,
                colorMultiplier = 1.0f,
                highlightedColor = Color.white,
                pressedColor = Color.white,
                disabledColor = Color.gray
            };
            buttonComponent.targetGraphic = backgroundComponent;

            uiButton.buttonComponent = buttonComponent;
            uiButton.backgroundComponent = backgroundComponent;
            uiButton.onClick = onClick;
            return uiButton;
        }
    }
}