using System;
using System.Windows.Input;

namespace Calculadora.Common
{
    /// <summary>
    /// Parameterless <see cref="ICommand"/> implementation backed by delegates.
    /// </summary>
    /// <remarks>
    /// Usage: <c>new RelayCommand(() => DoSomething());</c>
    /// </remarks>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        /// <summary>
        /// Initializes a new instance of <see cref="RelayCommand"/>.
        /// </summary>
        /// <param name="execute">The action to invoke when the command executes.</param>
        /// <param name="canExecute">Optional predicate controlling command availability.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="execute"/> is <c>null</c>.</exception>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <inheritdoc/>
        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        /// <inheritdoc/>
        public void Execute(object? parameter) => _execute();

        /// <summary>
        /// Hooks into the WPF command manager so the UI re-queries command availability on
        /// every user interaction.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }

    /// <summary>
    /// Generic <see cref="ICommand"/> implementation that accepts a strongly-typed parameter.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    /// <remarks>
    /// Usage: <c>new RelayCommand&lt;string&gt;(text => DoSomethingWith(text));</c>
    /// </remarks>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        /// <summary>
        /// Initializes a new instance of <see cref="RelayCommand{T}"/>.
        /// </summary>
        /// <param name="execute">The action to invoke when the command executes.</param>
        /// <param name="canExecute">Optional predicate controlling command availability.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="execute"/> is <c>null</c>.</exception>
        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <inheritdoc/>
        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;

        /// <inheritdoc/>
        public void Execute(object? parameter) => _execute((T?)parameter);

        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}