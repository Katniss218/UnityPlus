﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.UILib
{
    public class ValueBarSegment : MonoBehaviour
    {
#warning TODO - this along with the value bar can be turned to have a proper wrapper, analogous to everything else.

        [SerializeField]
        private float _left;
        [SerializeField]
        private float _width;
        [SerializeField]
        private float _widthParent;
        [SerializeField]
        private float _offset;

        internal float Left
        {
            get { return _left; }
            set
            {
                _left = value;
                OnLeftOrWidthChanged();
            }
        }

        internal float Width
        {
            get { return _width; }
            set
            {
                _width = value;
                OnLeftOrWidthChanged();
            }
        }

        internal float TotalWidth
        {
            get { return _widthParent; }
            set
            {
                _widthParent = value;
                OnLeftOrWidthChanged();
            }
        }

        internal float ForegroundOffsetX
        {
            get { return _offset; }
            set
            {
                _offset = value;
                OnForegroundOffsetChanged();
            }
        }

        [SerializeField]
        RectTransform _imageT;
        Image _image;

        RectTransform _selfMask;

        public Color Color { get => _image.color; set => _image.color = value; }
        public Sprite Sprite { get => _image.sprite; set => _image.sprite = value; }

        void Awake()
        {
            _selfMask = (RectTransform)this.transform;
        }

        public void SetImage( Image image )
        {
            _image = image;
            _imageT = (RectTransform)image.transform;
        }

#warning TODO - text.

        void OnLeftOrWidthChanged()
        {
            // mask and image are always anchored on the left, and have pivots on the left too.

            _selfMask.sizeDelta = new Vector2( Width, 0 );
            _imageT.sizeDelta = new Vector2( TotalWidth, 0 );
            _selfMask.anchoredPosition = new Vector2( Left, 0 );
            _imageT.anchoredPosition = new Vector2( -Left + ForegroundOffsetX, 0 );
        }

        void OnForegroundOffsetChanged()
        {
            _imageT.anchoredPosition = new Vector2( -Left + ForegroundOffsetX, 0 );
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if( _image != null ) // IsValidate can be called before a suitable image exists.
            {
                OnLeftOrWidthChanged();
                OnForegroundOffsetChanged();
            }
        }
#endif

        /// <remarks>
        /// Synchronize after creating a bar with this.
        /// </remarks>
        internal static ValueBarSegment Create( RectTransform parent, float totalWidth )
        {
            (GameObject maskGO, RectTransform maskT) = UIElement.CreateUIGameObject( parent, "mask", new UILayoutInfo( UIAnchor.Left, UIFill.Vertical(), 0, parent.rect.width ) );

            Image maskImage = maskGO.AddComponent<Image>();
            maskImage.raycastTarget = false;
            maskImage.sprite = null;

            Mask mask = maskGO.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            (GameObject imgGO, RectTransform imgT) = UIElement.CreateUIGameObject( maskT, "foreground", new UILayoutInfo( UIAnchor.Left, UIFill.Vertical(), 0, parent.rect.width ) );

            Image foregroundImage = imgGO.AddComponent<Image>();
            foregroundImage.raycastTarget = false;
            foregroundImage.type = Image.Type.Sliced;

            ValueBarSegment barSegment = maskGO.AddComponent<ValueBarSegment>();
            barSegment.SetImage( foregroundImage );
            barSegment.TotalWidth = totalWidth;

            return barSegment;
        }
    }
}