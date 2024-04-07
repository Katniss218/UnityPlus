using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.UILib.UIElements
{
    public class UIInputToggle : UIElement, IUIElementChild
    {
        // Toggle has 2 sprites, one for inactive, and other for active.

        public IUIElementContainer Parent { get; set; }

        protected internal static T Create<T>() where T : UIInputToggle
        {
            throw new NotImplementedException();
        }
    }
}