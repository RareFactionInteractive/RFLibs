using System;

namespace RFLibs.Bindable
{
    public class Bindable<T> : IBindableEndpoint<T>
    {
        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value)) return;
                var oldValue = _value;
                _value = value;
                OnValueChanged?.Invoke(oldValue, _value);
            }
        }

        public event Action<T, T>? OnValueChanged;

        public Bindable(T initialValue)
        {
            _value = initialValue;
        }
    }
}