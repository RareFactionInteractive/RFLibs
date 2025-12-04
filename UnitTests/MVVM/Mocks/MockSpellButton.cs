using RFLibs.MVVM;
using RFLibs.MVVM.Interfaces;
using UnitTests.MVVM.Models;

namespace UnitTests.MVVM.Mocks
{
    /// <summary>
    /// Mock button that binds to a spell command
    /// Demonstrates command binding with CanExecute
    /// </summary>
    public class MockSpellButton
    {
        private readonly ICommand? _command;
        private readonly ICommand<ITargetable>? _targetedCommand;
        private readonly ITargetable? _target;

        public bool IsEnabled { get; private set; }

        // Constructor for non-targeted commands
        public MockSpellButton(ICommand command)
        {
            _command = command;
            _command.CanExecuteChanged += UpdateEnabledState;
            UpdateEnabledState();
        }

        // Constructor for targeted commands
        public MockSpellButton(ICommand<ITargetable> targetedCommand, ITargetable target)
        {
            _targetedCommand = targetedCommand;
            _target = target;
            _targetedCommand.CanExecuteChanged += UpdateEnabledState;
            UpdateEnabledState();
        }

        public void Click()
        {
            if (_command != null && _command.CanExecute)
            {
                _command.Execute();
                UpdateEnabledState();
            }
            else if (_targetedCommand != null && _target != null && _targetedCommand.CanExecute(_target))
            {
                _targetedCommand.Execute(_target);
                UpdateEnabledState();
            }
        }

        public void UpdateEnabledState()
        {
            if (_command != null)
            {
                IsEnabled = _command.CanExecute;
            }
            else if (_targetedCommand != null && _target != null)
            {
                IsEnabled = _targetedCommand.CanExecute(_target);
            }
        }

        ~MockSpellButton()
        {
            if (_command != null)
                _command.CanExecuteChanged -= UpdateEnabledState;
            if (_targetedCommand != null)
                _targetedCommand.CanExecuteChanged -= UpdateEnabledState;
        }
    }
}
