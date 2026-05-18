using System;
using System.Windows.Input;

namespace Calculadora.Common
{
    // Implementación de ICommand sin parámetro.
    // Uso: new RelayCommand(() => DoSomething());
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // WPF pregunta esto para habilitar/deshabilitar el botón.
        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        // WPF llama esto cuando el usuario pulsa el botón.
        public void Execute(object? parameter) => _execute();

        // Se engancha al sistema de WPF para re-evaluar CanExecute automáticamente.
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    // Versión genérica: el botón puede enviar un parámetro tipado.
    // Uso: new RelayCommand<string>(text => DoSomethingWithText(text));
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