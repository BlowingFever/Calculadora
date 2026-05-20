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
        // Evalúa la expresión matemática del item en el rango X e Y configurado
        // y devuelve los arrays de puntos listos para ScottPlot.
        // Soporta expresiones del tipo:
        //   - y = f(x) o expresiones estándar en términos de 'x'.
        //   - x = f(y) o expresiones en términos de 'y' (curvas verticales o parábolas respecto a Y).
        //   - x = C (líneas verticales constantes, ej: x = 5).
        //
        public (double[] xs, double[] ys)? GetPlotData(ExpressionItem item, double xMin, double xMax, double yMin, double yMax)
        {
            if (string.IsNullOrWhiteSpace(item.Text)) return null;

            const int points = 500;
            var xs = new double[points];
            var ys = new double[points];

            try
            {
                string text = item.Text.Trim();
                bool isFunctionOfY = false;
                bool isVerticalConstant = false;
                string expressionToEvaluate = text;

                // Detectar si tiene '=' para separar lado izquierdo y derecho
                int eqIndex = text.IndexOf('=');
                if (eqIndex > 0)
                {
                    string lhs = text.Substring(0, eqIndex).Trim().ToLower();
                    string rhs = text.Substring(eqIndex + 1).Trim();

                    if (lhs == "x")
                    {
                        expressionToEvaluate = rhs;
                        // Si contiene 'y' es x = f(y), de lo contrario es una vertical constante x = C
                        if (rhs.ToLower().Contains("y"))
                        {
                            isFunctionOfY = true;
                        }
                        else
                        {
                            isVerticalConstant = true;
                        }
                    }
                    else if (lhs == "y")
                    {
                        expressionToEvaluate = rhs;
                        isFunctionOfY = false;
                    }
                }
                else
                {
                    // Si no tiene '=', pero contiene 'y', asumimos x = f(y)
                    if (text.ToLower().Contains("y"))
                    {
                        isFunctionOfY = true;
                    }
                }

                // CASO 1: Línea vertical constante (ej. x = 5)
                if (isVerticalConstant)
                {
                    var expr = new org.mariuszgromada.math.mxparser.Expression(expressionToEvaluate);
                    double constantX = expr.calculate();
                    if (double.IsNaN(constantX)) return null;

                    for (int i = 0; i < points; i++)
                    {
                        xs[i] = constantX;
                        ys[i] = yMin + (yMax - yMin) * i / (points - 1);
                    }
                    return (xs, ys);
                }

                // CASO 2: Función de Y (ej. x = y^2 o x = 2y + 1)
                if (isFunctionOfY)
                {
                    var f = new org.mariuszgromada.math.mxparser.Function($"f(y) = {expressionToEvaluate}");
                    if (!f.checkSyntax()) return null;

                    var arg = new org.mariuszgromada.math.mxparser.Argument("y", 0);
                    var expr = new org.mariuszgromada.math.mxparser.Expression("f(y)", f, arg);

                    for (int i = 0; i < points; i++)
                    {
                        double y = yMin + (yMax - yMin) * i / (points - 1);
                        arg.setArgumentValue(y);
                        ys[i] = y;
                        xs[i] = expr.calculate();
                    }
                    return (xs, ys);
                }

                // CASO 3: Función estándar de X (ej. y = x^2 o y = sin(x) o x^2)
                {
                    var f = new org.mariuszgromada.math.mxparser.Function($"f(x) = {expressionToEvaluate}");
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
            }
            catch
            {
                return null;
            }
        }
    }
}
