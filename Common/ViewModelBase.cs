using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Calculadora.Common
{
    /// <summary>
    /// Base class for all MVVM ViewModels.
    /// Centralises <see cref="INotifyPropertyChanged"/> boilerplate so derived classes
    /// only need to call <see cref="SetProperty{T}"/> from their property setters.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/> for the specified property.
        /// <see cref="System.Runtime.CompilerServices.CallerMemberNameAttribute"/> automatically
        /// infers the property name when called from inside a property setter.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Assigns <paramref name="value"/> to <paramref name="field"/> and raises
        /// <see cref="PropertyChanged"/> only when the value actually changes.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="field">Backing field reference.</param>
        /// <param name="value">New value to assign.</param>
        /// <param name="propertyName">Inferred automatically from the call site.</param>
        /// <returns><c>true</c> if the value changed; <c>false</c> otherwise.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}