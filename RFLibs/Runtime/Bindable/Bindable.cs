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
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }

        public event Action<T>? OnValueChanged;

        public Bindable(T initialValue)
        {
            _value = initialValue;
        }
    }
}