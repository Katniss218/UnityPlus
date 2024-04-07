using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// Represents a simple icon UI element.
    /// </summary>
    public class UIIcon : UIElement, IUIElementChild
    {
        protected internal Image imageComponent;

        public IUIElementContainer Parent { get; set; }

        public Sprite Sprite { get => imageComponent.sprite; set => imageComponent.sprite = value; }

        public static T Create<T>( IUIElementContainer parent, UILayoutInfo layoutInfo, Sprite icon ) where T : UIIcon
        {
            (GameObject rootGameObject, RectTransform rootTransform, T uiIcon) = UIElement.CreateUIGameObject<T>( parent, $"uilib-{nameof( T )}", layoutInfo );

            Image imageComponent = rootGameObject.AddComponent<Image>();
            imageComponent.raycastTarget = false;
            imageComponent.sprite = icon;
            imageComponent.type = Image.Type.Simple;

            uiIcon.imageComponent = imageComponent;
            return uiIcon;
        }
    }
}