using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Serialization.ComponentData
{
    public static class ComponentFinder
    {
        public static Component GetComponentByIndex( Component[] components, object data )
        {
            // `object data` because it has to be added to a common list.

            int index = (int)data;
            if( index < 0 || index >= components.Length )
            {
                return null;
            }
            return components[index];
        }

        public static Component GetComponentByTypeAndIndex( Component[] components, object data )
        {
            // `object data` because it has to be added to a common list.

            (Type type, int index) = ((Type, int))data;

            int current = 0;
            foreach( var comp in components )
            {
                if( comp.GetType() == type )
                {
                    if( current == index )
                    {
                        return comp;
                    }
                    current++;
                }
            }
            return null;
        }
    }
}
