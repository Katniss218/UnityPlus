using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Specifies whether to persist either the object reference (instance), state (data), or both.
    /// </summary>
    [Flags]
	public enum PersistMode
	{
		/// <summary>
		/// No persistence.
		/// </summary>
		None = 0,

		/// <summary>
		/// Persists object reference (instance).
		/// </summary>
		Objects = 1,

		/// <summary>
		/// Persists object state (data).
		/// </summary>
		Data = 2,

        /// <summary>
        /// Persists both object reference (instance) and state (data).
        /// </summary>
        ObjectsAndData = Objects | Data
	}

	/// <summary>
	/// Used to automatically persist a field or property when calling GetData, SetData, GetObjects, and SetObjects on an object.
	/// </summary>
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false )]
	public sealed class PersistAttribute : Attribute
	{
		/// <summary>
		/// The name of the member when serialized.
		/// </summary>
		public string SerializedName { get; set; }

		/// <summary>
		/// Whether or not to persist the object instance (reference) of a member.
		/// </summary>
		public bool PersistsObjects { get; }

		/// <summary>
		/// Whether or not to persist the data of a member.
		/// </summary>
		public bool PersistsData { get; }

		public PersistAttribute( string serializedName, PersistMode mode )
		{
			this.SerializedName = serializedName;
			this.PersistsObjects = mode.HasFlag( PersistMode.Objects );
			this.PersistsData = mode.HasFlag( PersistMode.Data );
		}
	}
}