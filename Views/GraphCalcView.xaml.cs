using System.Windows.Controls;
using Calculadora.ViewModels;

namespace Calculadora.Views
{
    public partial class GraphCalcView : UserControl
    {
        public GraphCalcView()
        {
            InitializeComponent();

            // ── ScottPlot: inicialización ──────────────────────────────
            //
            // Se ejecuta en Loaded para garantizar que el DataContext ya está asignado.
            //
            this.Loaded += (_, _) =>
            {
                if (DataContext is not GraphCalcViewModel vm) return;

                // Aplicar tema oscuro al fondo del gráfico.
                WpfPlot.Plot.FigureBackground.Color = new ScottPlot.Color(0x09, 0x0B, 0x12);
                WpfPlot.Plot.DataBackground.Color   = new ScottPlot.Color(0x16, 0x1B, 0x27);
                WpfPlot.Plot.Axes.Color(new ScottPlot.Color(0x8B, 0x98, 0xA9));

                // Suscribirse al evento de cambio de límites (zoom/pan) para recalcular dinámicamente
                WpfPlot.Plot.RenderManager.AxisLimitsChanged += (sender, args) => RefreshPlot(vm);

                // Suscribirse al evento del ViewModel para redibujar cuando cambien los datos.
                vm.PlotRefreshRequested += (_, _) => RefreshPlot(vm);

                // Dibujo inicial.
                RefreshPlot(vm);
            };
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

            // Si por alguna razón el rango no es válido, usamos valores por defecto
            if (double.IsNaN(xMin) || double.IsInfinity(xMin)) xMin = -10.0;
            if (double.IsNaN(xMax) || double.IsInfinity(xMax)) xMax = 10.0;

            WpfPlot.Plot.Clear();

            foreach (var item in vm.Expressions)
            {
                if (!item.Visible || string.IsNullOrWhiteSpace(item.Text))
                    continue;

                var data = vm.GetPlotData(item, xMin, xMax);
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