using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
	[Flags]
	public enum PersistMode
	{
		None = 0,
		Reference = 1,
		Data = 2,
		ReferenceAndData = Reference | Data
	}

	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false )]
	public sealed class PersistAttribute : Attribute
	{
		public string Key { get; set; }
		public bool PersistsReference { get; }
		public bool PersistsData { get; }

		public PersistAttribute( string key, PersistMode mode )
		{
			this.Key = key;
			this.PersistsReference = mode.HasFlag( PersistMode.Reference );
			this.PersistsData = mode.HasFlag( PersistMode.Data );
		}
	}
}