using System;

namespace RFLibs.MVVM.Interfaces
{
    public interface ICommand<T>
    {
        void Execute(T parameter);
        bool CanExecute(T parameter);
        event Action CanExecuteChanged;
    }
}
