using System;
using RFLibs.MVVM.Interfaces;

namespace RFLibs.MVVM
{
    public class Command<T> : ICommand<T>
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;
        public event Action CanExecuteChanged;


        public Command(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }


        public bool CanExecute(T parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(T parameter)
        {
            if (CanExecute(parameter)) _execute?.Invoke(parameter);
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke();
    }
}
