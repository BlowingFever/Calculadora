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

            // Suscribirse al evento global de cambio de tema
            ThemeManager.ThemeChanged += OnThemeChanged;

            // Desuscribirse al descargar el control para evitar fugas de memoria
            this.Unloaded += (s, e) =>
            {
                ThemeManager.ThemeChanged -= OnThemeChanged;
            };

            // ── ScottPlot: inicialización ──────────────────────────────
            //
            // Se ejecuta en Loaded para garantizar que el DataContext ya está asignado.
            //
            this.Loaded += (_, _) =>
            {
                if (DataContext is not GraphCalcViewModel vm) return;

                // Aplicar el tema inicial del gráfico
                UpdatePlotTheme();

                // Suscribirse al evento de cambio de límites (zoom/pan) para recalcular dinámicamente
                WpfPlot.Plot.RenderManager.AxisLimitsChanged += (sender, args) => RefreshPlot(vm);

                // Suscribirse al evento del ViewModel para redibujar cuando cambien los datos.
                vm.PlotRefreshRequested += (_, _) => RefreshPlot(vm);

                // Dibujo inicial.
                RefreshPlot(vm);
            };
        }

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            UpdatePlotTheme();
        }

        // ── Método de tematización del gráfico ──────────────────────────
        //
        // Sincroniza dinámicamente el aspecto de ScottPlot con el tema actual (claro/oscuro).
        //
        private void UpdatePlotTheme()
        {
            if (WpfPlot?.Plot is null) return;

            // Obtener colores dinámicos desde los recursos de la aplicación (definidos en Themes/Colors.xaml)
            var canvasBg = GetScottPlotColor("GraphCanvasBackgroundColor", new ScottPlot.Color(0x09, 0x0B, 0x12));
            var axisColor = GetScottPlotColor("GraphAxisColor", new ScottPlot.Color(0x9C, 0xA3, 0xAF));
            var gridLineColor = GetScottPlotColor("GraphGridLineColor", new ScottPlot.Color(0x2A, 0x2F, 0x3A));

            // Para el fondo de datos, usamos un tono con ligero contraste y estética acorde al tema actual
            ScottPlot.Color dataBg = ThemeManager.IsDark 
                ? new ScottPlot.Color(0x16, 0x1B, 0x27)   // Fondo ligeramente más claro en tema oscuro
                : new ScottPlot.Color(0xF9, 0xFA, 0xFB);  // Fondo blanco/gris muy suave en tema claro

            // Asignar colores a ScottPlot
            WpfPlot.Plot.FigureBackground.Color = canvasBg;
            WpfPlot.Plot.DataBackground.Color = dataBg;
            WpfPlot.Plot.Axes.Color(axisColor);

            // Ajustar el color de las líneas principales de la cuadrícula
            WpfPlot.Plot.Grid.MajorLineColor = gridLineColor;

            WpfPlot.Refresh();
        }

        // ── Helper para obtener colores de recursos WPF con fallback ────
        //
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
                // Fallback seguro en caso de error
            }
            return defaultColor;
        }

        // ── Método de renderizado ──────────────────────────────────────
        //
        // Esta es la ÚNICA lógica permitida en el code-behind:
        // recibe datos ya calculados del ViewModel y los pasa al control de UI.
        //
        private void RefreshPlot(GraphCalcViewModel vm)
        {
            var limits = WpfPlot.Plot.Axes.GetLimits();
            double xMin = limits.Left;
            double xMax = limits.Right;
            double yMin = limits.Bottom;
            double yMax = limits.Top;

            // Si por alguna razón el rango no es válido, usamos valores por defecto
            if (double.IsNaN(xMin) || double.IsInfinity(xMin)) xMin = -10.0;
            if (double.IsNaN(xMax) || double.IsInfinity(xMax)) xMax = 10.0;
            if (double.IsNaN(yMin) || double.IsInfinity(yMin)) yMin = -10.0;
            if (double.IsNaN(yMax) || double.IsInfinity(yMax)) yMax = 10.0;

            WpfPlot.Plot.Clear();

            // Dibujar los ejes cartesianos infinitos X e Y cruzándose en (0,0) para guiar al usuario
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