using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Calculadora.Models
{
    /// <summary>
    /// Represents a single mathematical expression entry in the graphing calculator UI.
    /// </summary>
    /// <remarks>
    /// Implements <see cref="INotifyPropertyChanged"/> directly (rather than inheriting
    /// <c>ViewModelBase</c>) so it can be data-bound inside an <c>ItemsControl</c> without
    /// pulling in the full ViewModel hierarchy.
    /// </remarks>
    public class ExpressionItem : INotifyPropertyChanged
    {
        private string _text = string.Empty;
        private string _color = "#2196F3";
        private bool _visible = true;

        /// <summary>
        /// Unique identifier used to map this entry to its curve on the ScottPlot canvas.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// The mathematical function text entered by the user, e.g. <c>"x^2 + sin(x)"</c>.
        /// </summary>
        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Hexadecimal color string (<c>"#RRGGBB"</c>) used to render the plotted curve.
        /// </summary>
        public string Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Controls whether the curve is rendered on the plot.
        /// </summary>
        public bool Visible
        {
            get => _visible;
            set { _visible = value; OnPropertyChanged(); }
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/> for the calling member.
        /// </summary>
        /// <param name="propertyName">Inferred automatically from the call site.</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}