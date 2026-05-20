using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Calculadora.Common
{
    // Base class for MVVM ViewModels that centralizes property change notifications.
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // Raises the PropertyChanged event. CallerMemberName automatically infers the property name.
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Updates a field value and raises the PropertyChanged event only if the value has changed.
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}