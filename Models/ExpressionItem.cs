using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Calculadora.Models
{
    // Represents a single mathematical expression for the graphing UI.
    // Implements INotifyPropertyChanged directly to allow data-binding within ItemsControl.
    public class ExpressionItem : INotifyPropertyChanged
    {
        private string _text    = string.Empty;
        private string _color   = "#2196F3";
        private bool   _visible = true;

        // Unique ID used to map and identify the curve on the ScottPlot canvas.
        public Guid Id { get; } = Guid.NewGuid();

        // The mathematical function text, e.g., "x^2 + sin(x)".
        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }

        // Hexadecimal color representation of the plotted line.
        public string Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(); }
        }

        // Toggles whether the curve is rendered on the plot.
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
