using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Calculadora.Models
{
    /// <summary>
    /// Represents a single mathematical expression entry in the graphing calculator UI.
    /// Each instance maps to one row in the side-panel list and one curve on the ScottPlot canvas.
    /// </summary>
    /// <remarks>
    /// Implements <see cref="INotifyPropertyChanged"/> directly rather than inheriting
    /// <c>ViewModelBase</c>, so it can be data-bound inside an <c>ItemsControl</c>
    /// without pulling in the full ViewModel hierarchy into the model layer.
    /// </remarks>
    public class ExpressionItem : INotifyPropertyChanged
    {
        private string _text = string.Empty;
        private string _color = "#2196F3";
        private bool _visible = true;

        /// <summary>
        /// Unique identifier generated at construction time.
        /// Used to associate this entry with its corresponding curve in ScottPlot
        /// even when the text or colour changes.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// The mathematical function text entered by the user.
        /// Examples: <c>"x^2"</c>, <c>"sin(x) + cos(x)"</c>, <c>"x = y^2"</c>.
        /// </summary>
        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Hexadecimal colour string (<c>"#RRGGBB"</c>) used to render the plotted curve.
        /// Assigned automatically from the colour palette in <c>GraphCalcViewModel</c>.
        /// </summary>
        public string Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Controls whether the curve is rendered on the plot.
        /// Users can hide/show expressions without removing them.
        /// </summary>
        public bool Visible
        {
            get => _visible;
            set { _visible = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Raised when the value of any property on this instance changes,
        /// allowing bound controls to update automatically.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/> for the calling member.
        /// </summary>
        /// <param name="propertyName">
        /// Name of the property that changed.
        /// Inferred automatically by the compiler via <see cref="CallerMemberNameAttribute"/>.
        /// </param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}