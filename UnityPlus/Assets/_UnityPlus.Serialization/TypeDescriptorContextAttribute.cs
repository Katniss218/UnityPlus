using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Registers an open generic Type Descriptor to handle a specific serialization context.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = false )]
    public sealed class TypeDescriptorContextAttribute : Attribute
    {
        public int Context { get; }

        public TypeDescriptorContextAttribute( int context )
        {
            Context = context;
        }
    }
}