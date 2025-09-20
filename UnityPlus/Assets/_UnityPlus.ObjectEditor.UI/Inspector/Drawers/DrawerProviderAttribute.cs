using System;

namespace UnityPlus.ObjectEditor.UI.Inspector.Drawers
{
    [AttributeUsage( AttributeTargets.Class )]
    public abstract class DrawerProviderAttribute : Attribute
    {
        public Type DrawnType { get; set; }

        // context is given by the mapping itself.
    }
}