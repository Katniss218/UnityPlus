using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.Layout;

namespace UnityPlus.UILib.UIElements
{
    /// <summary>
    /// A context menu contains a list of items.
    /// </summary>
    public partial class UIContextMenu : UIElement, IUIElementContainer, IUIElementChild, IUILayoutDriven
    {
        protected RectTransformTrackRectTransform trackerComponent;
        protected RectTransformDestroyOnLeave destroyOnLeaveComponent;
        protected Image backgroundComponent;
        public virtual RectTransform contents => base.rectTransform;

        public IUIElementContainer Parent { get; set; }
        public List<IUIElementChild> Children { get; } = new List<IUIElementChild>();

        public LayoutDriver LayoutDriver { get; set; }

        public virtual Sprite Background { get => backgroundComponent.sprite; set => backgroundComponent.sprite = value; }

        public Action OnHide;

        public Vector2 Offset { get => trackerComponent.Offset; set => trackerComponent.Offset = value; }

        void OnDestroy()
        {
            OnHide?.Invoke();
        }

        protected internal static T Create<T>( RectTransform track, UICanvas contextMenuCanvas, UILayoutInfo layoutInfo, Sprite background, Action onDestroy ) where T : UIContextMenu
        {
            (GameObject rootGameObject, RectTransform rootTransform, T uiContextMenu) = UIElement.CreateUIGameObject<T>( contextMenuCanvas, $"uilib-{typeof( T ).Name}", layoutInfo );
            rootTransform.pivot = new Vector2( 0, 1 ); // top-left

            Image backgroundComponent = rootGameObject.AddComponent<Image>();
            backgroundComponent.raycastTarget = false;
            backgroundComponent.sprite = background;
            backgroundComponent.type = Image.Type.Sliced;

            if( background == null )
            {
                backgroundComponent.color = new Color( 0, 0, 0, 0 );
            }

            RectTransformTrackRectTransform trackerComponent = rootGameObject.AddComponent<RectTransformTrackRectTransform>();
            trackerComponent.Target = track;

            RectTransformDestroyOnLeave destroyOnLeaveComponent = rootGameObject.AddComponent<RectTransformDestroyOnLeave>();

            uiContextMenu.trackerComponent = trackerComponent;
            uiContextMenu.destroyOnLeaveComponent = destroyOnLeaveComponent;
            uiContextMenu.backgroundComponent = backgroundComponent;
            uiContextMenu.OnHide = onDestroy;
            return uiContextMenu;
        }
    }
}