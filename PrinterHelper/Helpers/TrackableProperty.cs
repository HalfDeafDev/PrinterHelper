using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrinterHelper.Helpers
{
    public class TrackableProperty<T> : ObservableObject, ITrackable
    {
        private T _original;
        protected T _value;

        public T Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public T SolidValue
        {
            get => _original;
        }

        public TrackableProperty(T value)
        {
            _value = value;
            _original = value;
        }

        public bool HasChanged()
        {
            if (_value is not null && !_value.Equals(_original))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Solidify()
        {
            _original = _value;

            OnPropertyChanged(nameof(SolidValue));
        }

        public void Solidify(T value)
        {
            Value = value;
            Solidify();
        }
    }
}
