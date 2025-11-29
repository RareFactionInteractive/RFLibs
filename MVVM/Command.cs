using System;
using RFLibs.MVVM.Interfaces;

namespace RFLibs.MVVM
{
    public class Command : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;
        public event Action CanExecuteChanged;


        public Command(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }


        public bool CanExecute
        {
            get
            {
                return _canExecute?.Invoke() ?? true;
            }
        }

        public void Execute()
        {
            if (CanExecute) _execute?.Invoke();
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke();
    }
}