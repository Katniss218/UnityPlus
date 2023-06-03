using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Serialization
{
    public class CreatedByFactory : MonoBehaviour
    {
        /// <summary>
        /// the ID of the factory that created this object.
        /// </summary>
        private string _factoryID;

#warning TODO - guard against adding this component if any of the parents of this.gameObject already have it.
    }
}