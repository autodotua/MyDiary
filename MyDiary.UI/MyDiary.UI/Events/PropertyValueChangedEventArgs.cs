using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDiary.UI.Events
{
    public class PropertyValueChangedEventArgs<T>:EventArgs
    {
        public PropertyValueChangedEventArgs(T oldValue,T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public T OldValue { get; set; } 
        public T NewValue { get; set; }
    }
}
