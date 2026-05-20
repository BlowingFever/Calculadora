using System;
using System.Windows;
using System.Windows.Controls;
using Calculadora.ViewModels;
using Calculadora.Models;

namespace Calculadora.Views
{
    /// <summary>
    /// Code-behind for the graphing calculator view.
    /// Manages ScottPlot initialisation, theme synchronisation, and plot refresh cycles
    /// driven by <see cref="GraphCalcViewModel.PlotRefreshRequested"/>.
    /// </summary>
    public partial class GraphCalcView : UserControl
    {
        /// <summary>
        /// Initialises the control, subscribes to theme changes, and defers ScottPlot
        /// setup to the <c>Loaded</c> event so the <c>DataContext</c> is guaranteed available.
        /// </summary>
        public GraphCalcView()
        {
            InitializeComponent();

            ThemeManager.ThemeChanged += OnThemeChanged;
            Unloaded += (_, _) => ThemeManager.ThemeChanged -= OnThemeChanged;

            Loaded += (_, _) =>
            {
                if (DataContext is not GraphCalcViewModel vm) return;

                UpdatePlotTheme();

                WpfPlot.Plot.RenderManager.AxisLimitsChanged += (_, _) => RefreshPlot(vm);
                vm.PlotRefreshRequested += (_, _) => RefreshPlot(vm);

                RefreshPlot(vm);
            };
        }

        /// <summary>
        /// Handles global theme-change notifications and refreshes the plot colours.
        /// </summary>
        private void OnThemeChanged(object? sender, EventArgs e) => UpdatePlotTheme();

        /// <summary>
        /// Synchronises ScottPlot background, axis, and grid colours with the currently
        /// active light or dark theme by reading values from WPF Application resources.
        /// </summary>
        private void UpdatePlotTheme()
        {
            if (WpfPlot?.Plot is null) return;

            var canvasBg = GetScottPlotColor("GraphCanvasBackgroundColor", new ScottPlot.Color(0x09, 0x0B, 0x12));
            var axisColor = GetScottPlotColor("GraphAxisColor", new ScottPlot.Color(0x9C, 0xA3, 0xAF));
            var gridColor = GetScottPlotColor("GraphGridLineColor", new ScottPlot.Color(0x2A, 0x2F, 0x3A));

            ScottPlot.Color dataBg = ThemeManager.IsDark
                ? new ScottPlot.Color(0x16, 0x1B, 0x27)
                : new ScottPlot.Color(0xF9, 0xFA, 0xFB);

            WpfPlot.Plot.FigureBackground.Color = canvasBg;
            WpfPlot.Plot.DataBackground.Color = dataBg;
            WpfPlot.Plot.Axes.Color(axisColor);
            WpfPlot.Plot.Grid.MajorLineColor = gridColor;

            WpfPlot.Refresh();
        }

        /// <summary>
        /// Resolves a WPF resource key to a <see cref="ScottPlot.Color"/>, accepting both
        /// <see cref="System.Windows.Media.Color"/> and <see cref="System.Windows.Media.SolidColorBrush"/>
        /// resource types. Returns <paramref name="defaultColor"/> when the key is absent or
        /// resolution fails.
        /// </summary>
        /// <param name="resourceKey">Application resource key to look up.</param>
        /// <param name="defaultColor">Fallback colour.</param>
        /// <returns>Resolved <see cref="ScottPlot.Color"/>.</returns>
        private ScottPlot.Color GetScottPlotColor(string resourceKey, ScottPlot.Color defaultColor)
        {
            try
            {
                if (Application.Current.Resources.Contains(resourceKey))
                {
                    var res = Application.Current.Resources[resourceKey];

                    if (res is System.Windows.Media.Color color)
                        return new ScottPlot.Color(color.R, color.G, color.B);

                    if (res is System.Windows.Media.SolidColorBrush brush)
                        return new ScottPlot.Color(brush.Color.R, brush.Color.G, brush.Color.B);
                }
            }
            catch { /* Safe fallback. */ }

            return defaultColor;
        }

        /// <summary>
        /// Clears all plot series, redraws the origin axes, evaluates every visible
        /// expression, and requests a ScottPlot refresh.
        /// </summary>
        /// <param name="vm">The graphing ViewModel providing expression data.</param>
        private void RefreshPlot(GraphCalcViewModel vm)
        {
            var limits = WpfPlot.Plot.Axes.GetLimits();
            double xMin = double.IsNaN(limits.Left) || double.IsInfinity(limits.Left) ? -10.0 : limits.Left;
            double xMax = double.IsNaN(limits.Right) || double.IsInfinity(limits.Right) ? 10.0 : limits.Right;
            double yMin = double.IsNaN(limits.Bottom) || double.IsInfinity(limits.Bottom) ? -10.0 : limits.Bottom;
            double yMax = double.IsNaN(limits.Top) || double.IsInfinity(limits.Top) ? 10.0 : limits.Top;

            WpfPlot.Plot.Clear();

            // Draw the X = 0 and Y = 0 reference axes.
            var axisColor = GetScottPlotColor("GraphAxisColor",
                ThemeManager.IsDark
                    ? new ScottPlot.Color(0x9C, 0xA3, 0xAF)
                    : new ScottPlot.Color(0x9C, 0xA3, 0xAF));

            var xAxis = WpfPlot.Plot.Add.HorizontalLine(0);
            xAxis.Color = axisColor;
            xAxis.LineWidth = 1.5F;

            var yAxis = WpfPlot.Plot.Add.VerticalLine(0);
            yAxis.Color = axisColor;
            yAxis.LineWidth = 1.5F;

            foreach (var item in vm.Expressions)
            {
                if (!item.Visible || string.IsNullOrWhiteSpace(item.Text)) continue;

                var data = vm.GetPlotData(item, xMin, xMax, yMin, yMax);
                if (data is null) continue;

                var (xs, ys) = data.Value;
                var scatter = WpfPlot.Plot.Add.Scatter(xs, ys);
                scatter.Color = ScottPlot.Color.FromHex(item.Color);
                scatter.LineWidth = 2;
                scatter.MarkerSize = 0;
            }

            WpfPlot.Refresh();
        }
    }
}