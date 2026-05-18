using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using Calculadora.Common;
using Calculadora.Models;

namespace Calculadora.ViewModels
{
    // ViewModel del módulo de calculadora gráfica.
    //
    // Responsabilidades:
    //   1. Gestionar la lista de expresiones (añadir, eliminar, colores).
    //   2. Exponer el evento PlotRefreshRequested para que la Vista sepa
    //      cuándo debe redibujar el gráfico (puente MVVM ↔ ScottPlot).
    //   3. En el futuro: evaluar expresiones con mXparser y pasar los datos
    //      a la Vista mediante GetPlotData().
    //
    // No contiene ninguna referencia a elementos de UI (control, ventana, etc.).
    public class GraphCalcViewModel : ViewModelBase
    {
        static GraphCalcViewModel()
        {
            org.mariuszgromada.math.mxparser.License.iConfirmNonCommercialUse("WPF Graph Calculator User");
        }

        // ── Paleta de colores secuencial ──────────────────────────────────────
        // Cada nueva expresión recibe automáticamente el siguiente color.

        private static readonly string[] ColorPalette =
        {
            "#2196F3",   // Azul
            "#F44336",   // Rojo
            "#4CAF50",   // Verde
            "#FF9800",   // Naranja
            "#9C27B0",   // Violeta
            "#00BCD4",   // Cian
        };

        private int _paletteIndex = 0;

        // ── Colección observable ──────────────────────────────────────────────
        // ObservableCollection notifica a la UI automáticamente cuando
        // se añade o elimina un elemento — sin código extra.

        public ObservableCollection<ExpressionItem> Expressions { get; } = new();

        // ── Comandos ──────────────────────────────────────────────────────────

        public ICommand AddExpressionCommand    { get; }
        public ICommand RemoveExpressionCommand { get; }

        // ── Evento de refresco del gráfico (puente MVVM ↔ ScottPlot) ─────────
        //
        // El ViewModel lo dispara cuando los datos cambian.
        // La Vista se suscribe y se encarga del renderizado.
        // Así 0 código de UI entra en el ViewModel.
        //
        public event EventHandler? PlotRefreshRequested;

        // ── Constructor ───────────────────────────────────────────────────────

        public GraphCalcViewModel()
        {
            AddExpressionCommand    = new RelayCommand(AddExpression);
            RemoveExpressionCommand = new RelayCommand<ExpressionItem>(RemoveExpression);

            // Seguir los cambios de la colección para disparar el refresco.
            Expressions.CollectionChanged += OnExpressionsCollectionChanged;

            // Arrancamos con un campo vacío ya visible.
            AddExpression();
        }

        // ── Handlers de comandos ──────────────────────────────────────────────

        private void AddExpression()
        {
            var item = new ExpressionItem
            {
                Color = ColorPalette[_paletteIndex % ColorPalette.Length]
            };

            // Nos suscribimos a los cambios del item para detectar ediciones
            // de texto y cambios de visibilidad, y así poder disparar el refresco.
            item.PropertyChanged += OnExpressionItemPropertyChanged;

            _paletteIndex++;
            Expressions.Add(item);
        }

        private void RemoveExpression(ExpressionItem? item)
        {
            if (item == null) return;

            // Importante: cancelar la suscripción antes de eliminar
            // para evitar fugas de memoria (memory leaks).
            item.PropertyChanged -= OnExpressionItemPropertyChanged;

            Expressions.Remove(item);
        }

        // ── Handlers de eventos internos ──────────────────────────────────────

        private void OnExpressionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RequestPlotRefresh();
        }

        private void OnExpressionItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Se dispara cuando el usuario edita el texto o activa/desactiva
            // la visibilidad de una expresión.
            RequestPlotRefresh();
        }

        // ── Método auxiliar de refresco ──────────────────────────────────────
        private void RequestPlotRefresh()
            => PlotRefreshRequested?.Invoke(this, EventArgs.Empty);

        // ── Proveedor de datos para el gráfico ───────────────────────
        //
        // Evalúa la expresión matemática del item en el rango X configurado
        // y devuelve los arrays de puntos listos para ScottPlot.
        //
        public (double[] xs, double[] ys)? GetPlotData(ExpressionItem item, double xMin, double xMax)
        {
            if (string.IsNullOrWhiteSpace(item.Text)) return null;

            const int points = 500;
            var xs = new double[points];
            var ys = new double[points];

            try
            {
                var f = new org.mariuszgromada.math.mxparser.Function($"f(x) = {item.Text}");
                if (!f.checkSyntax()) return null;

                var arg = new org.mariuszgromada.math.mxparser.Argument("x", 0);
                var expr = new org.mariuszgromada.math.mxparser.Expression("f(x)", f, arg);

                for (int i = 0; i < points; i++)
                {
                    double x = xMin + (xMax - xMin) * i / (points - 1);
                    arg.setArgumentValue(x);
                    xs[i] = x;
                    ys[i] = expr.calculate();
                }

                return (xs, ys);
            }
            catch
            {
                return null;
            }
        }
    }
}
