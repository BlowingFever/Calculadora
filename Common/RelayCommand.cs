using System;
using System.Windows.Input;

namespace Calculadora.Common
{
    // Parameterless ICommand implementation.
    // Usage: new RelayCommand(() => DoSomething());
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Determines if the command can execute in its current state.
        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        // Executes the command logic.
        public void Execute(object? parameter) => _execute();

        // Hooks into WPF to query command status when UI interaction occurs.
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    // Generic ICommand implementation accepting a parameter of type T.
    // Usage: new RelayCommand<string>(text => DoSomethingWithText(text));
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;
        public void Execute(object? parameter) => _execute((T?)parameter);

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}