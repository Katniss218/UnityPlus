using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.ObjectEditor.UI.Inspector
{
    public interface IDrawer
    {
        void Redraw();
    }

    /// <summary>
    /// this drawer should create the UIs for *any* object. All data should be in the mapping.
    /// </summary>
    public class MappedDrawer : IDrawer
    {
        ObjectAccessor _accessor;

        public void Redraw() // so this will basically do kind of the same thing that serialization will do. it needs to walk the mapping and look through the 'members'.
        {
            // accessor might be 'primitive' - in that case, just draw it.
            if( _accessor.IsPrimitive )
            {
                // draw primitive
                return;
            }

            // would need to recognize what a collection is and to add the buttons. this makes no sense for a generic drawer I think
            // 
        }
    }

    // v4 accessors

    public abstract class ObjectAccessor
    {
        // serialization will match members to serializeddata by name. mappings can't handle special stuff outside of factory/callbacks.
    }

    public class PrimitiveObjectAccessor : ObjectAccessor
    {
        // primitives would need to directly convert to serializeddata, and be drawn directly.
        // so they need separate serialization handling and separate drawers.
    }

    public class MemberwiseObjectAccessor : ObjectAccessor
    {
        MemberAccessor[] _members;
    }

    public class IndexedObjectAccessor : ObjectAccessor
    {
        // index getter/setter
        MemberAccessor[] _members; // length/count, etc. possibly readonly.

        IAction[] _actions; // append, remove, clear, etc. but it might be better to handle these with normal C# interfaces, since that's what they're for.
    }

    public class MemberAccessor
    {
        public string Name { get; }
        // getter
        // setter
        // readonly/writeonly
    }

    // some sort of automatic creation of reasonable default values for the accessors for types that don't have explicit definitions.
}