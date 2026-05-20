using System;
using System.Windows;
using System.Windows.Controls;
using Calculadora.ViewModels;
using Calculadora.Models;

namespace Calculadora.Views
{
    public partial class GraphCalcView : UserControl
    {
        public GraphCalcView()
        {
            InitializeComponent();

            // Subscribe to global theme changes
            ThemeManager.ThemeChanged += OnThemeChanged;

            // Unsubscribe on unload to prevent memory leaks
            this.Unloaded += (s, e) =>
            {
                ThemeManager.ThemeChanged -= OnThemeChanged;
            };

            // ScottPlot initialization in Loaded event to guarantee DataContext availability
            this.Loaded += (_, _) =>
            {
                if (DataContext is not GraphCalcViewModel vm) return;

                // Synchronize initial theme
                UpdatePlotTheme();

                // Recalculate plot limits on zoom/pan interaction
                WpfPlot.Plot.RenderManager.AxisLimitsChanged += (sender, args) => RefreshPlot(vm);

                // Refresh rendering when ViewModel signals data updates
                vm.PlotRefreshRequested += (_, _) => RefreshPlot(vm);

                // Render initial draw
                RefreshPlot(vm);
            };
        }

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            UpdatePlotTheme();
        }

        // Synchronizes ScottPlot colors with the active light/dark theme
        private void UpdatePlotTheme()
        {
            if (WpfPlot?.Plot is null) return;

            // Fetch colors from Application Resources
            var canvasBg = GetScottPlotColor("GraphCanvasBackgroundColor", new ScottPlot.Color(0x09, 0x0B, 0x12));
            var axisColor = GetScottPlotColor("GraphAxisColor", new ScottPlot.Color(0x9C, 0xA3, 0xAF));
            var gridLineColor = GetScottPlotColor("GraphGridLineColor", new ScottPlot.Color(0x2A, 0x2F, 0x3A));

            // Determine slightly contrasted background shade based on theme
            ScottPlot.Color dataBg = ThemeManager.IsDark 
                ? new ScottPlot.Color(0x16, 0x1B, 0x27)   
                : new ScottPlot.Color(0xF9, 0xFA, 0xFB);  

            // Asignar colores a ScottPlot
            WpfPlot.Plot.FigureBackground.Color = canvasBg;
            WpfPlot.Plot.DataBackground.Color = dataBg;
            WpfPlot.Plot.Axes.Color(axisColor);

            // Configure grid lines color
            WpfPlot.Plot.Grid.MajorLineColor = gridLineColor;

            WpfPlot.Refresh();
        }

        // Helper to extract WPF colors from Resources, falling back to a default value if missing
        private ScottPlot.Color GetScottPlotColor(string resourceKey, ScottPlot.Color defaultColor)
        {
            try
            {
                if (Application.Current.Resources.Contains(resourceKey))
                {
                    var res = Application.Current.Resources[resourceKey];
                    if (res is System.Windows.Media.Color color)
                    {
                        return new ScottPlot.Color(color.R, color.G, color.B);
                    }
                    if (res is System.Windows.Media.SolidColorBrush brush)
                    {
                        return new ScottPlot.Color(brush.Color.R, brush.Color.G, brush.Color.B);
                    }
                }
            }
            catch
            {
                // Safe fallback in case of errors
            }
            return defaultColor;
        }

        // Refreshes the plot data and triggers redrawing on the UI control
        private void RefreshPlot(GraphCalcViewModel vm)
        {
            var limits = WpfPlot.Plot.Axes.GetLimits();
            double xMin = limits.Left;
            double xMax = limits.Right;
            double yMin = limits.Bottom;
            double yMax = limits.Top;

            // Fallback to defaults if range values are invalid
            if (double.IsNaN(xMin) || double.IsInfinity(xMin)) xMin = -10.0;
            if (double.IsNaN(xMax) || double.IsInfinity(xMax)) xMax = 10.0;
            if (double.IsNaN(yMin) || double.IsInfinity(yMin)) yMin = -10.0;
            if (double.IsNaN(yMax) || double.IsInfinity(yMax)) yMax = 10.0;

            WpfPlot.Plot.Clear();

            // Draw baseline infinite X and Y axes crossing at (0,0)
            var axisColor = GetScottPlotColor("GraphAxisColor", ThemeManager.IsDark 
                ? new ScottPlot.Color(0x9C, 0xA3, 0xAF) 
                : new ScottPlot.Color(0x9C, 0xA3, 0xAF));

            var xZeroLine = WpfPlot.Plot.Add.HorizontalLine(0);
            xZeroLine.Color = axisColor;
            xZeroLine.LineWidth = 1.5F;

            var yZeroLine = WpfPlot.Plot.Add.VerticalLine(0);
            yZeroLine.Color = axisColor;
            yZeroLine.LineWidth = 1.5F;

            foreach (var item in vm.Expressions)
            {
                if (!item.Visible || string.IsNullOrWhiteSpace(item.Text))
                    continue;

                var data = vm.GetPlotData(item, xMin, xMax, yMin, yMax);
                if (data is null) continue;

                var (xs, ys) = data.Value;
                var scatter = WpfPlot.Plot.Add.Scatter(xs, ys);
                scatter.Color     = ScottPlot.Color.FromHex(item.Color);
                scatter.LineWidth = 2;
                scatter.MarkerSize = 0;
            }

            WpfPlot.Refresh();
        }
    }
}