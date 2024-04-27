using System;
using UnityEngine;

namespace UnityPlus.UILib
{
    public class RectTransformTrackRectTransform : MonoBehaviour
    {
        /// <summary>
        /// The RectTransform to follow the center of.
        /// </summary>
        public RectTransform Target { get; set; }

        public Vector2 Offset { get; set; }

        void LateUpdate()
        {
            Vector2 pos = Target.TransformPoint( Target.rect.center );
            this.transform.position = pos + Offset;
        }
    }
}