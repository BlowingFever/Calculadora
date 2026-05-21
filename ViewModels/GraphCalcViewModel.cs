using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using Calculadora.Common;
using Calculadora.Models;

namespace Calculadora.ViewModels
{
    /// <summary>
    /// ViewModel de la calculadora gráfica.
    /// Gestiona la colección de expresiones matemáticas, evalúa cada una
    /// mediante mXparser y notifica a la vista cuándo debe redibujar el gráfico.
    /// </summary>
    /// <remarks>
    /// Flujo de uso:
    /// <list type="number">
    ///   <item><description>El usuario escribe expresiones en la lista lateral (<see cref="Expressions"/>).</description></item>
    ///   <item><description>Cualquier cambio en texto, color o visibilidad dispara <see cref="PlotRefreshRequested"/>.</description></item>
    ///   <item><description>El code-behind de la vista se suscribe al evento y llama a <see cref="GetPlotData"/>
    ///     para cada expresión visible.</description></item>
    ///   <item><description>ScottPlot renderiza los datos resultantes.</description></item>
    /// </list>
    /// </remarks>
    public class GraphCalcViewModel : ViewModelBase
    {
        // ── Inicialización estática ───────────────────────────────────────────

        /// <summary>
        /// Confirma el uso no comercial de la librería mXparser en el momento
        /// en que se carga la clase (antes de que se cree ninguna instancia).
        /// Este paso es obligatorio según la licencia de mXparser.
        /// </summary>
        static GraphCalcViewModel()
        {
            org.mariuszgromada.math.mxparser.License.iConfirmNonCommercialUse("WPF Graph Calculator User");
        }

        // ── Paleta de colores ─────────────────────────────────────────────────

        /// <summary>
        /// Paleta de colores cíclica asignada a las nuevas expresiones en orden de creación.
        /// Los colores se repiten en bucle cuando se superan las 6 expresiones.
        /// </summary>
        private static readonly string[] ColorPalette =
        {
            "#2196F3",  // Azul
            "#F44336",  // Rojo
            "#4CAF50",  // Verde
            "#FF9800",  // Naranja
            "#9C27B0",  // Morado
            "#00BCD4",  // Cian
        };

        /// <summary>Índice actual en <see cref="ColorPalette"/>, incrementado al añadir expresiones.</summary>
        private int _paletteIndex = 0;

        // ── Propiedades públicas ──────────────────────────────────────────────

        /// <summary>
        /// Colección observable de expresiones enlazada a la lista lateral de la vista.
        /// Cada elemento contiene el texto de la función, su color y su visibilidad.
        /// </summary>
        public ObservableCollection<ExpressionItem> Expressions { get; } = new();

        // ── Comandos ──────────────────────────────────────────────────────────

        /// <summary>
        /// Añade una nueva entrada vacía a <see cref="Expressions"/> con el siguiente
        /// color de la paleta.
        /// </summary>
        public ICommand AddExpressionCommand { get; }

        /// <summary>
        /// Elimina la entrada <see cref="ExpressionItem"/> especificada de <see cref="Expressions"/>
        /// y cancela su suscripción a eventos de cambio de propiedad.
        /// </summary>
        public ICommand RemoveExpressionCommand { get; }

        // ── Eventos ───────────────────────────────────────────────────────────

        /// <summary>
        /// Se dispara cuando es necesario redibujar el gráfico, es decir:
        /// <list type="bullet">
        ///   <item><description>El texto de una expresión cambia.</description></item>
        ///   <item><description>La visibilidad de una expresión cambia.</description></item>
        ///   <item><description>Se añade o elimina una expresión de la colección.</description></item>
        /// </list>
        /// </summary>
        public event EventHandler? PlotRefreshRequested;

        // ── Constructor ───────────────────────────────────────────────────────

        /// <summary>
        /// Configura los comandos, se suscribe a los cambios de la colección
        /// y añade una expresión vacía inicial para que el usuario pueda empezar
        /// a escribir de inmediato.
        /// </summary>
        public GraphCalcViewModel()
        {
            AddExpressionCommand = new RelayCommand(AddExpression);
            RemoveExpressionCommand = new RelayCommand<ExpressionItem>(RemoveExpression);

            Expressions.CollectionChanged += OnExpressionsCollectionChanged;

            AddExpression(); // Expresión inicial vacía.
        }

        // ── Manejadores de comandos ───────────────────────────────────────────

        /// <summary>
        /// Crea un nuevo <see cref="ExpressionItem"/> con el siguiente color de la paleta,
        /// se suscribe a sus cambios de propiedad y lo añade al final de <see cref="Expressions"/>.
        /// </summary>
        private void AddExpression()
        {
            var item = new ExpressionItem
            {
                Color = ColorPalette[_paletteIndex % ColorPalette.Length]
            };
            item.PropertyChanged += OnExpressionItemPropertyChanged;
            _paletteIndex++;
            Expressions.Add(item);
        }

        /// <summary>
        /// Cancela la suscripción de <paramref name="item"/> a los eventos de propiedad
        /// y lo elimina de <see cref="Expressions"/>. Es seguro llamarlo con <c>null</c>.
        /// </summary>
        /// <param name="item">Elemento a eliminar; no hace nada si es <c>null</c>.</param>
        private void RemoveExpression(ExpressionItem? item)
        {
            if (item == null) return;
            item.PropertyChanged -= OnExpressionItemPropertyChanged;
            Expressions.Remove(item);
        }

        // ── Manejadores de eventos internos ───────────────────────────────────

        /// <summary>
        /// Reacciona a cambios en la colección (añadir/eliminar elementos)
        /// solicitando un redibujado del gráfico.
        /// </summary>
        private void OnExpressionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => RequestPlotRefresh();

        /// <summary>
        /// Reacciona a cambios en las propiedades de un <see cref="ExpressionItem"/>
        /// (texto, color, visibilidad) solicitando un redibujado del gráfico.
        /// </summary>
        private void OnExpressionItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
            => RequestPlotRefresh();

        /// <summary>
        /// Invoca el evento <see cref="PlotRefreshRequested"/> para notificar a la vista
        /// que debe volver a dibujar el gráfico.
        /// </summary>
        private void RequestPlotRefresh()
            => PlotRefreshRequested?.Invoke(this, EventArgs.Empty);

        // ── Evaluación de expresiones ─────────────────────────────────────────

        /// <summary>
        /// Evalúa la expresión de <paramref name="item"/> con mXparser dentro de los
        /// límites de los ejes visibles y devuelve 500 puntos de coordenadas (x, y)
        /// listos para pasar a ScottPlot.
        /// </summary>
        /// <remarks>
        /// Soporta tres formas de expresión:
        /// <list type="table">
        ///   <listheader><term>Forma</term><description>Ejemplo</description></listheader>
        ///   <item><term>Función de X</term><description><c>sin(x)</c> o <c>y = x^2 + 1</c></description></item>
        ///   <item><term>Función de Y</term><description><c>x = y^2</c> (curva horizontal)</description></item>
        ///   <item><term>Línea vertical constante</term><description><c>x = 5</c></description></item>
        /// </list>
        /// Si la expresión tiene sintaxis inválida o lanza una excepción durante la
        /// evaluación, el método devuelve <c>null</c> silenciosamente.
        /// </remarks>
        /// <param name="item">Entrada de expresión a evaluar.</param>
        /// <param name="xMin">Límite izquierdo del eje X visible.</param>
        /// <param name="xMax">Límite derecho del eje X visible.</param>
        /// <param name="yMin">Límite inferior del eje Y visible.</param>
        /// <param name="yMax">Límite superior del eje Y visible.</param>
        /// <returns>
        /// Una tupla <c>(xs, ys)</c> con 500 elementos cada array,
        /// o <c>null</c> si la expresión está vacía, es inválida o falla en tiempo de evaluación.
        /// </returns>
        public (double[] xs, double[] ys)? GetPlotData(
            ExpressionItem item,
            double xMin, double xMax,
            double yMin, double yMax)
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

                int eqIndex = text.IndexOf('=');
                if (eqIndex > 0)
                {
                    string lhs = text[..eqIndex].Trim().ToLower();
                    string rhs = text[(eqIndex + 1)..].Trim();

                    if (lhs == "x")
                    {
                        expressionToEvaluate = rhs;
                        if (rhs.ToLower().Contains("y"))
                            isFunctionOfY = true;
                        else
                            isVerticalConstant = true;
                    }
                    else if (lhs == "y")
                    {
                        expressionToEvaluate = rhs;
                    }
                }
                else if (text.ToLower().Contains("y"))
                {
                    // Sin '=' pero con 'y': se interpreta como x = f(y).
                    isFunctionOfY = true;
                }

                // ── Caso 1: Línea vertical constante (p. ej. x = 5) ──────────
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

                // ── Caso 2: Función de Y (p. ej. x = y^2) ───────────────────
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

                // ── Caso 3: Función estándar de X (p. ej. y = sin(x)) ────────
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