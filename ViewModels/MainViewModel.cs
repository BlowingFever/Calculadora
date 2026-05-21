using System.Windows.Input;
using Calculadora.Common;

namespace Calculadora.ViewModels
{
    /// <summary>
    /// Root ViewModel that owns the active view and exposes navigation commands.
    /// WPF resolves the correct <c>DataTemplate</c> for <see cref="CurrentView"/>
    /// automatically via type-keyed templates registered in <c>App.xaml</c>.
    /// </summary>
    /// <remarks>
    /// Child ViewModels are created as singletons so that their state
    /// (calculation history, graph expressions, etc.) is preserved when the user
    /// switches between views and returns.
    /// </remarks>
    public class MainViewModel : ViewModelBase
    {
        // ── Active view ───────────────────────────────────────────────────────

        private ViewModelBase _currentView;

        /// <summary>
        /// Gets the ViewModel that is currently displayed.
        /// Bound to the <c>ContentControl</c> in <c>MainWindow.xaml</c>.
        /// Changing this value causes WPF to swap the visual content automatically.
        /// </summary>
        public ViewModelBase CurrentView
        {
            get => _currentView;
            private set => SetProperty(ref _currentView, value);
        }

        // ── Singleton child ViewModels ─────────────────────────────────────────

        /// <summary>
        /// Single instance of the standard calculator ViewModel.
        /// Retained so that calculation state survives view switches.
        /// </summary>
        private readonly NormalCalcViewModel _normalCalcVm = new();

        /// <summary>
        /// Single instance of the graphing calculator ViewModel.
        /// Retained so that entered expressions survive view switches.
        /// </summary>
        private readonly GraphCalcViewModel _graphCalcVm = new();

        // ── Navigation commands ───────────────────────────────────────────────

        /// <summary>
        /// Switches <see cref="CurrentView"/> to the standard calculator.
        /// Bound to navigation buttons/tabs in the view.
        /// </summary>
        public ICommand ShowNormalCalculatorCommand { get; }

        /// <summary>
        /// Switches <see cref="CurrentView"/> to the graphing calculator.
        /// Bound to navigation buttons/tabs in the view.
        /// </summary>
        public ICommand ShowGraphCalculatorCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────

        /// <summary>
        /// Wires the navigation commands and sets the default startup view
        /// to the standard calculator.
        /// </summary>
        public MainViewModel()
        {
            ShowNormalCalculatorCommand = new RelayCommand(() => CurrentView = _normalCalcVm);
            ShowGraphCalculatorCommand = new RelayCommand(() => CurrentView = _graphCalcVm);

            _currentView = _normalCalcVm; // default startup view
        }
    }
}