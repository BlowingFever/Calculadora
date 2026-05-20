using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using Calculadora.Common;
using Calculadora.Models;

namespace Calculadora.ViewModels
{
    // ViewModel managing the graphing calculator logic and expressions.
    // Exposes refresh notifications to guide view rendering on ScottPlot.
    public class GraphCalcViewModel : ViewModelBase
    {
        static GraphCalcViewModel()
        {
            org.mariuszgromada.math.mxparser.License.iConfirmNonCommercialUse("WPF Graph Calculator User");
        }

        // Palette of hexadecimal colors to assign to newly added equations sequentially.
        private static readonly string[] ColorPalette =
        {
            "#2196F3",   // Blue
            "#F44336",   // Red
            "#4CAF50",   // Green
            "#FF9800",   // Orange
            "#9C27B0",   // Purple
            "#00BCD4",   // Cyan
        };

        private int _paletteIndex = 0;

        // Observable collection bound to UI elements for listing equations.
        public ObservableCollection<ExpressionItem> Expressions { get; } = new();

        // Commands for managing expression collection entries.

        public ICommand AddExpressionCommand    { get; }
        public ICommand RemoveExpressionCommand { get; }

        // Event raised when expression edits or collection shifts require plot refresh.
        public event EventHandler? PlotRefreshRequested;

        // ── Constructor ───────────────────────────────────────────────────────

        public GraphCalcViewModel()
        {
            AddExpressionCommand    = new RelayCommand(AddExpression);
            RemoveExpressionCommand = new RelayCommand<ExpressionItem>(RemoveExpression);

            // Trigger plot refresh when items are added or removed from the collection
            Expressions.CollectionChanged += OnExpressionsCollectionChanged;

            // Populate an initial empty expression field on startup
            AddExpression();
        }

        // -- Command Handlers

        private void AddExpression()
        {
            var item = new ExpressionItem
            {
                Color = ColorPalette[_paletteIndex % ColorPalette.Length]
            };

            // Subscribe to catch subsequent property edits (e.g., text or visibility changes)
            item.PropertyChanged += OnExpressionItemPropertyChanged;

            _paletteIndex++;
            Expressions.Add(item);
        }

        private void RemoveExpression(ExpressionItem? item)
        {
            if (item == null) return;

            // Unsubscribe to avoid memory leaks
            item.PropertyChanged -= OnExpressionItemPropertyChanged;

            Expressions.Remove(item);
        }

        // -- Internal Event Handlers

        private void OnExpressionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RequestPlotRefresh();
        }

        private void OnExpressionItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Trigger refresh on property updates like user input text or visible toggles
            RequestPlotRefresh();
        }

        // Raises the plot refresh request event
        private void RequestPlotRefresh()
            => PlotRefreshRequested?.Invoke(this, EventArgs.Empty);

        // Evaluates a mathematical equation with mXparser within the bounding limits.
        // Returns coordinates arrays for plotting. Supports:
        // - Standard y = f(x)
        // - Vertical curves x = f(y)
        // - Constant vertical lines x = Constant
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

                // Parse LHS and RHS if equation contains '='
                int eqIndex = text.IndexOf('=');
                if (eqIndex > 0)
                {
                    string lhs = text.Substring(0, eqIndex).Trim().ToLower();
                    string rhs = text.Substring(eqIndex + 1).Trim();

                    if (lhs == "x")
                    {
                        expressionToEvaluate = rhs;
                        // If RHS has 'y', it represents x = f(y); otherwise, it is a vertical constant x = C
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
                    // If it doesn't have '=', but contains 'y', we assume x = f(y)
                    if (text.ToLower().Contains("y"))
                    {
                        isFunctionOfY = true;
                    }
                }

                // CASE 1: Constant vertical line (e.g., x = 5)
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

                // CASE 2: Function of Y (e.g., x = y^2 or x = 2y + 1)
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

                // CASE 3: Standard function of X (e.g., y = x^2, y = sin(x))
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
