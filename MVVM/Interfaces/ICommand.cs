using System;

namespace RFLibs.MVVM.Interfaces
{
    public interface ICommand
    {
        void Execute();
        bool CanExecute { get; }
        event Action CanExecuteChanged;
    }
}
