using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Calculadora.Common
{
    /// <summary>
    /// Abstract base class for all ViewModels in the application.
    /// Centralises <see cref="INotifyPropertyChanged"/> boilerplate so derived classes
    /// only need to call <see cref="SetProperty{T}"/> inside their property setters.
    /// </summary>
    /// <remarks>
    /// Every ViewModel should inherit from this class instead of implementing
    /// <see cref="INotifyPropertyChanged"/> directly, ensuring consistent
    /// change-notification behaviour across all WPF data bindings.
    /// </remarks>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Raised when the value of a bound property changes.
        /// WPF subscribes to this event automatically to refresh bound controls.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/> for the specified property.
        /// Thanks to <see cref="CallerMemberNameAttribute"/>, the property name is
        /// inferred automatically when called from inside a property setter.
        /// </summary>
        /// <param name="propertyName">
        /// Name of the property that changed.
        /// Filled in by the compiler when omitted.
        /// </param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Assigns <paramref name="value"/> to the backing <paramref name="field"/> and raises
        /// <see cref="PropertyChanged"/> only when the value actually changes.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="field">Reference to the property's backing field.</param>
        /// <param name="value">New value to assign.</param>
        /// <param name="propertyName">
        /// Name of the property. Inferred automatically from the call site.
        /// </param>
        /// <returns>
        /// <c>true</c> if the value changed and <see cref="PropertyChanged"/> was raised;
        /// <c>false</c> if the value was equal to the previous one.
        /// </returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}