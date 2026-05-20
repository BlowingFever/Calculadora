using System.Windows.Input;
using Calculadora.Common;

namespace Calculadora.ViewModels
{
    /// <summary>
    /// Root ViewModel that owns the active view and exposes navigation commands.
    /// WPF resolves the correct <c>DataTemplate</c> for <see cref="CurrentView"/> automatically
    /// via the type-keyed templates registered in <c>App.xaml</c>.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        // ── Active View ───────────────────────────────────────────────────────

        private ViewModelBase _currentView;

        /// <summary>
        /// The currently displayed ViewModel, bound to the <c>ContentControl</c> in
        /// <c>MainWindow.xaml</c>.
        /// </summary>
        public ViewModelBase CurrentView
        {
            get => _currentView;
            private set => SetProperty(ref _currentView, value);
        }

        // ── Singleton Child ViewModels ─────────────────────────────────────────

        /// <summary>
        /// Retained as singletons so that calculation state is preserved across view switches.
        /// </summary>
        private readonly NormalCalcViewModel _normalCalcVm = new();
        private readonly GraphCalcViewModel _graphCalcVm = new();

        // ── Navigation Commands ────────────────────────────────────────────────

        /// <summary>Switches <see cref="CurrentView"/> to the standard calculator.</summary>
        public ICommand ShowNormalCalculatorCommand { get; }

        /// <summary>Switches <see cref="CurrentView"/> to the graphing calculator.</summary>
        public ICommand ShowGraphCalculatorCommand { get; }

        // ── Constructor ────────────────────────────────────────────────────────

        /// <summary>
        /// Wires navigation commands and sets the default startup view to the standard calculator.
        /// </summary>
        public MainViewModel()
        {
            ShowNormalCalculatorCommand = new RelayCommand(() => CurrentView = _normalCalcVm);
            ShowGraphCalculatorCommand = new RelayCommand(() => CurrentView = _graphCalcVm);

            _currentView = _normalCalcVm;
        }
    }
}