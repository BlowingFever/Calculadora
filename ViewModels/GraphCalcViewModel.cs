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
    /// ViewModel for the graphing calculator view.
    /// Manages the collection of <see cref="ExpressionItem"/> entries and provides
    /// plot data evaluated via mXparser, notifying the view when a redraw is needed.
    /// </summary>
    public class GraphCalcViewModel : ViewModelBase
    {
        /// <summary>
        /// Confirms non-commercial use of the mXparser library at class load time.
        /// </summary>
        static GraphCalcViewModel()
        {
            org.mariuszgromada.math.mxparser.License.iConfirmNonCommercialUse("WPF Graph Calculator User");
        }

        /// <summary>
        /// Sequential colour palette assigned to new expressions in round-robin order.
        /// </summary>
        private static readonly string[] ColorPalette =
        {
            "#2196F3",  // Blue
            "#F44336",  // Red
            "#4CAF50",  // Green
            "#FF9800",  // Orange
            "#9C27B0",  // Purple
            "#00BCD4",  // Cyan
        };

        private int _paletteIndex = 0;

        /// <summary>
        /// Observable collection of expressions bound to the left-panel list.
        /// </summary>
        public ObservableCollection<ExpressionItem> Expressions { get; } = new();

        /// <summary>Adds a new empty expression entry to <see cref="Expressions"/>.</summary>
        public ICommand AddExpressionCommand { get; }

        /// <summary>Removes a specific <see cref="ExpressionItem"/> from <see cref="Expressions"/>.</summary>
        public ICommand RemoveExpressionCommand { get; }

        /// <summary>
        /// Raised whenever the plot must be redrawn (expression text changed, visibility toggled,
        /// or collection modified).
        /// </summary>
        public event EventHandler? PlotRefreshRequested;

        // ── Constructor ───────────────────────────────────────────────────────

        /// <summary>
        /// Initialises commands, subscribes to collection change events,
        /// and populates one empty expression on startup.
        /// </summary>
        public GraphCalcViewModel()
        {
            AddExpressionCommand = new RelayCommand(AddExpression);
            RemoveExpressionCommand = new RelayCommand<ExpressionItem>(RemoveExpression);

            Expressions.CollectionChanged += OnExpressionsCollectionChanged;

            AddExpression();
        }

        // ── Command Handlers ──────────────────────────────────────────────────

        /// <summary>
        /// Creates a new <see cref="ExpressionItem"/> with the next palette colour,
        /// subscribes to its property changes, and appends it to <see cref="Expressions"/>.
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
        /// Unsubscribes from property change events and removes <paramref name="item"/>
        /// from <see cref="Expressions"/>.
        /// </summary>
        /// <param name="item">Item to remove; no-op when <c>null</c>.</param>
        private void RemoveExpression(ExpressionItem? item)
        {
            if (item == null) return;
            item.PropertyChanged -= OnExpressionItemPropertyChanged;
            Expressions.Remove(item);
        }

        // ── Internal Event Handlers ───────────────────────────────────────────

        private void OnExpressionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => RequestPlotRefresh();

        private void OnExpressionItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
            => RequestPlotRefresh();

        /// <summary>Fires <see cref="PlotRefreshRequested"/>.</summary>
        private void RequestPlotRefresh()
            => PlotRefreshRequested?.Invoke(this, EventArgs.Empty);

        // ── Plot Data ─────────────────────────────────────────────────────────

        /// <summary>
        /// Evaluates <paramref name="item"/>'s expression with mXparser over the given axis bounds
        /// and returns coordinate arrays ready for ScottPlot.
        /// </summary>
        /// <remarks>
        /// Supports three expression forms:
        /// <list type="bullet">
        ///   <item><description>Standard function of X: <c>y = f(x)</c> or plain <c>f(x)</c>.</description></item>
        ///   <item><description>Function of Y: <c>x = f(y)</c>.</description></item>
        ///   <item><description>Vertical constant line: <c>x = C</c>.</description></item>
        /// </list>
        /// </remarks>
        /// <param name="item">The expression entry to evaluate.</param>
        /// <param name="xMin">Left bound of the visible x-axis range.</param>
        /// <param name="xMax">Right bound of the visible x-axis range.</param>
        /// <param name="yMin">Bottom bound of the visible y-axis range.</param>
        /// <param name="yMax">Top bound of the visible y-axis range.</param>
        /// <returns>
        /// A tuple of parallel x and y arrays (500 points each), or <c>null</c> if the
        /// expression is empty, has invalid syntax, or throws during evaluation.
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
                    // No '=' but contains 'y' — treat as x = f(y).
                    isFunctionOfY = true;
                }

                // ── Case 1: Constant vertical line (e.g. x = 5) ──────────────
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

                // ── Case 2: Function of Y (e.g. x = y^2) ────────────────────
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

                // ── Case 3: Standard function of X (e.g. y = sin(x)) ─────────
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