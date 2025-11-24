namespace RFLibs.Bindable
{
    using System;

    public interface IReadableEndpoint<T>
    {
        T Value { get; }
        event Action<T> OnValueChanged; // fires when Value *logically* changes
    }

    public interface IWritableEndpoint<T>
    {
        T Value { get; set; }
    }

// Readable + Writable = Two-way capable
    public interface IBindableEndpoint<T> : IReadableEndpoint<T>, IWritableEndpoint<T>
    {
    }
}