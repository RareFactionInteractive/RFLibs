using System;

namespace RFLibs.MVVM
{
    public class Bindable<T>
    {
        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value)) return;
                _value = value;
                OnValueChanged?.Invoke(value);
            }
        }

        public event Action<T> OnValueChanged;

        public Bindable(T initialValue = default, Action<T>? onValueChanged = null)
        {
            _value = initialValue;

            if (onValueChanged != null)
            {
                OnValueChanged += onValueChanged;
            }   
        }
    }
}