using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Calculadora.Common
{
    // Clase base que deben heredar TODOS los ViewModels.
    // Centraliza INotifyPropertyChanged para no repetirlo en cada VM.
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // Notifica a la UI que una propiedad ha cambiado.
        // [CallerMemberName] rellena automáticamente el nombre de la propiedad
        // que llama a este método, sin que tengamos que escribirlo a mano.
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Versión inteligente: solo notifica si el valor realmente cambió.
        // Uso: SetProperty(ref _field, newValue);
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}