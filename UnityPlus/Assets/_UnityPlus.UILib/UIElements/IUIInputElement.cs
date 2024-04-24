using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.UILib.UIElements
{
    public interface IUIInputElement<TValue>
    {
        public class ValueChangedEventData
        {
            public bool HasValue { get; set; }
            public TValue NewValue { get; set; }
        }

        event Action<ValueChangedEventData> OnValueChanged;

        bool TryGetValue( out TValue value );
    }
}