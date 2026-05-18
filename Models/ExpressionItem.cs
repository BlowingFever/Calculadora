using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Calculadora.Models
{
    // Representa una ecuación individual en la lista del panel gráfico.
    //
    // Es un Model, no un ViewModel, por eso implementa INotifyPropertyChanged
    // directamente en lugar de heredar de ViewModelBase.
    // La UI (ItemsControl) reacciona automáticamente a sus cambios gracias a esto.
    public class ExpressionItem : INotifyPropertyChanged
    {
        private string _text    = string.Empty;
        private string _color   = "#2196F3";
        private bool   _visible = true;

        // ID único. Se usará para identificar cada curva en ScottPlot
        // cuando se integre la librería.
        public Guid Id { get; } = Guid.NewGuid();

        // La expresión matemática que escribe el usuario, p.ej. "x^2 + sin(x)"
        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }

        // Color de la curva en formato hex, p.ej. "#F44336"
        // Asignado automáticamente de la paleta al crear el item.
        public string Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(); }
        }

        // Si es true, la curva se dibuja. Si es false, se oculta en el gráfico.
        public bool Visible
        {
            get => _visible;
            set { _visible = value; OnPropertyChanged(); }
        }

        // ── INotifyPropertyChanged ────────────────────────────────────────────

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
