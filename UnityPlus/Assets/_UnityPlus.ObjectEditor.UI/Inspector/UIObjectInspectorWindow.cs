using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.UILib.UIElements;

namespace UnityPlus.ObjectEditor.UI.Inspector
{
    /// <summary>
    /// Inspects the specified objects.
    /// </summary>
    internal class UIObjectInspectorWindow : UIWindow
    {
        private object _inspectedObject;

        public void Redraw()
        {
            IDrawer drawer = null;// get drawer for _inspectedObject
            drawer.Redraw();
        }
    }
}