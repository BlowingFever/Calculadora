using System.Windows.Controls;

namespace Calculadora.Views
{
    /// <summary>
    /// Code-behind for the standard calculator view.
    /// All interaction logic is handled by <see cref="Calculadora.ViewModels.NormalCalcViewModel"/>.
    /// </summary>
    public partial class NormalCalcView : UserControl
    {
        /// <summary>Initializes the <see cref="NormalCalcView"/> component.</summary>
        public NormalCalcView() => InitializeComponent();
    }
}