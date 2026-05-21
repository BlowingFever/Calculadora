using System;
using System.Windows.Input;

namespace Calculadora.Common
{
    /// <summary>
    /// Parameterless <see cref="ICommand"/> implementation backed by delegates.
    /// Allows commands to be defined concisely inside a ViewModel without a dedicated class.
    /// </summary>
    /// <remarks>
    /// Typical usage:
    /// <code>
    /// ClearCommand = new RelayCommand(() => OnClear());
    /// SaveCommand  = new RelayCommand(() => OnSave(), () => CanSave);
    /// </code>
    /// Command availability is re-evaluated automatically after every user interaction
    /// via the WPF <see cref="System.Windows.Input.CommandManager"/>.
    /// </remarks>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        /// <summary>
        /// Initialises a new instance of <see cref="RelayCommand"/>.
        /// </summary>
        /// <param name="execute">
        /// The action to invoke when the command executes. Must not be <c>null</c>.
        /// </param>
        /// <param name="canExecute">
        /// Optional predicate that controls whether the command is available.
        /// When omitted the command is always enabled.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="execute"/> is <c>null</c>.
        /// </exception>
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
        /// Hooks into <see cref="CommandManager.RequerySuggested"/> so the UI
        /// re-queries <see cref="CanExecute"/> after every user interaction,
        /// keeping buttons enabled/disabled reactively.
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
    /// Typical usage:
    /// <code>
    /// DigitCommand  = new RelayCommand&lt;string&gt;(digit => OnDigit(digit));
    /// RemoveCommand = new RelayCommand&lt;ExpressionItem&gt;(item => Remove(item));
    /// </code>
    /// </remarks>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        /// <summary>
        /// Initialises a new instance of <see cref="RelayCommand{T}"/>.
        /// </summary>
        /// <param name="execute">
        /// The typed action to invoke when the command executes. Must not be <c>null</c>.
        /// </param>
        /// <param name="canExecute">
        /// Optional typed predicate controlling command availability.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="execute"/> is <c>null</c>.
        /// </exception>
        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <inheritdoc/>
        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;

        /// <inheritdoc/>
        public void Execute(object? parameter) => _execute((T?)parameter);

        /// <inheritdoc cref="RelayCommand.CanExecuteChanged"/>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}