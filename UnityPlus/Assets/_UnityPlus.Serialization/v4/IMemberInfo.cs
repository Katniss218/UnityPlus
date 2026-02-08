
using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Describes a specific step in the serialization process (a Field, Property, or Array Element).
    /// </summary>
    public interface IMemberInfo
    {
        /// <summary>
        /// The name used for serialization (Key in JSON object). Null for Array elements.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The index of the member, used for Arrays/Lists. -1 if not applicable (Named Member).
        /// </summary>
        int Index { get; }

        /// <summary>
        /// The C# type of this member.
        /// </summary>
        Type MemberType { get; }

        /// <summary>
        /// The descriptor that handles this member's type.
        /// </summary>
        ITypeDescriptor TypeDescriptor { get; }

        /// <summary>
        /// If true, modifying the value returned by GetValue requires a Write-Back to persist changes (e.g. Structs).
        /// </summary>
        bool IsValueType { get; }

        /// <summary>
        /// Retrieves the value from the target object.
        /// </summary>
        object GetValue( object target );

        /// <summary>
        /// Sets the value on the target object.
        /// </summary>
        /// <param name="target">The target object. Passed by ref to support replacing boxed value types.</param>
        /// <param name="value">The value to set.</param>
        void SetValue( ref object target, object value );
    }
}
