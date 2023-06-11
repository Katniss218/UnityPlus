using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Serialization.Factories
{
    /// <summary>
    /// Marks an object as created by a factory with the specified name.
    /// </summary>
    [DisallowMultipleComponent]
    public class CreatedByFactory : MonoBehaviour
    {
        /// <summary>
        /// The asset ID of the factory that created this GameObject. Should match a registered instance of <see cref="IFactory{GameObject}"/>.
        /// </summary>
        [field: SerializeField]
        public string FactoryAssetID { get; internal set; }

        void Awake()
        {
            Transform transform = this.transform.parent;

            while( transform != null ) 
            {
                if( transform.GetComponent<CreatedByFactory>() != null )
                {
                    throw new InvalidOperationException( $"Can't add a {nameof( CreatedByFactory )} component to `{this.gameObject.name}` because its parent (`{transform.gameObject.name}`) already has one." );
                }
                transform = transform.parent;
            }
        }
    }
}