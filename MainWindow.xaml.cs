using System.Windows;
using System.Windows.Input;
using Calculadora.ViewModels;

namespace Calculadora
{
    /// <summary>
    /// Code-behind for <c>MainWindow.xaml</c>.
    /// Translates raw keyboard input into calculator commands when the standard
    /// calculator view is active.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>Initializes the <see cref="MainWindow"/> component.</summary>
        public MainWindow() => InitializeComponent();

        /// <summary>
        /// Intercepts key-down events from the focused <c>ContentControl</c> and forwards
        /// them to <see cref="NormalCalcViewModel"/> commands.
        /// Key events are suppressed (<c>e.Handled = true</c>) to prevent duplicate input.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Key event data.</param>
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not MainViewModel main) return;
            if (main.CurrentView is not NormalCalcViewModel vm) return;

            switch (e.Key)
            {
                case Key.D0:
                case Key.NumPad0:
                    vm.DigitCommand?.Execute("0"); e.Handled = true; break;
                case Key.D1:
                case Key.NumPad1:
                    vm.DigitCommand?.Execute("1"); e.Handled = true; break;
                case Key.D2:
                case Key.NumPad2:
                    vm.DigitCommand?.Execute("2"); e.Handled = true; break;
                case Key.D3:
                case Key.NumPad3:
                    vm.DigitCommand?.Execute("3"); e.Handled = true; break;
                case Key.D4:
                case Key.NumPad4:
                    vm.DigitCommand?.Execute("4"); e.Handled = true; break;
                case Key.D5:
                case Key.NumPad5:
                    vm.DigitCommand?.Execute("5"); e.Handled = true; break;
                case Key.D6:
                case Key.NumPad6:
                    vm.DigitCommand?.Execute("6"); e.Handled = true; break;

                // Shift+7 on Spanish/international keyboards maps to '/'
                case Key.D7 when (Keyboard.Modifiers & ModifierKeys.Shift) != 0:
                    vm.OperatorCommand?.Execute("/"); e.Handled = true; break;
                case Key.D7:
                case Key.NumPad7:
                    vm.DigitCommand?.Execute("7"); e.Handled = true; break;

                case Key.D8:
                case Key.NumPad8:
                    vm.DigitCommand?.Execute("8"); e.Handled = true; break;
                case Key.D9:
                case Key.NumPad9:
                    vm.DigitCommand?.Execute("9"); e.Handled = true; break;

                case Key.Add:
                    vm.OperatorCommand?.Execute("+"); e.Handled = true; break;
                case Key.Subtract:
                    vm.OperatorCommand?.Execute("-"); e.Handled = true; break;
                case Key.Multiply:
                    vm.OperatorCommand?.Execute("*"); e.Handled = true; break;
                case Key.Divide:
                    vm.OperatorCommand?.Execute("/"); e.Handled = true; break;

                // Shift+OemPlus ('*' on some layouts)
                case Key.OemPlus when (Keyboard.Modifiers & ModifierKeys.Shift) != 0:
                    vm.OperatorCommand?.Execute("*"); e.Handled = true; break;

                case Key.OemMinus:
                    vm.OperatorCommand?.Execute("-"); e.Handled = true; break;
                case Key.OemPlus:
                    vm.OperatorCommand?.Execute("+"); e.Handled = true; break;

                case Key.OemComma:
                case Key.OemPeriod:
                case Key.Decimal:
                    vm.DecimalCommand?.Execute(null); e.Handled = true; break;

                case Key.Enter:
                    vm.EqualsCommand?.Execute(null); e.Handled = true; break;
                case Key.Escape:
                    vm.ClearCommand?.Execute(null); e.Handled = true; break;
            }
        }
    }
}