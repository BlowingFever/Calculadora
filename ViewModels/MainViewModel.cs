using System.Windows.Input;
using Calculadora.Common;

namespace Calculadora.ViewModels
{
    // Main ViewModel responsible for handling active view navigation.
    public class MainViewModel : ViewModelBase
    {
        // Active view bound to ContentControl in MainWindow.
        // WPF resolves the matching DataTemplate automatically.
        private ViewModelBase _currentView;
        public ViewModelBase CurrentView
        {
            get => _currentView;
            private set => SetProperty(ref _currentView, value);
        }

        // Singletons reused to preserve calculation states between view transitions.
        private readonly NormalCalcViewModel _normalCalcVm = new();
        private readonly GraphCalcViewModel _graphCalcVm = new();

        // Navigation commands bound to footer buttons.
        public ICommand ShowNormalCalculatorCommand { get; }
        public ICommand ShowGraphCalculatorCommand { get; }

        public MainViewModel()
        {
            ShowNormalCalculatorCommand = new RelayCommand(() => CurrentView = _normalCalcVm);
            ShowGraphCalculatorCommand = new RelayCommand(() => CurrentView = _graphCalcVm);

            // Default startup view.
            _currentView = _normalCalcVm;
        }
    }
}