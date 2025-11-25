namespace RFLibs.Bindable
{
    using System;

    public interface IReadableEndpoint<T>
    {
        T Value { get; }
        event Action<T, T> OnValueChanged;
    }

    public interface IWritableEndpoint<T>
    {
        T Value { get; set; }
    }

    public interface IBindableEndpoint<T> : IReadableEndpoint<T>, IWritableEndpoint<T>
    {
    }
}