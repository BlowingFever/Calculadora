using System.Windows.Input;
using Calculadora.Common;

namespace Calculadora.ViewModels
{
    // ViewModel principal.
    // Su única responsabilidad: saber QUÉ vista está activa
    // y cambiarla cuando el usuario pulsa los botones de navegación.
    public class MainViewModel : ViewModelBase
    {
        // La vista activa. ContentControl en MainWindow se enlaza aquí.
        // Cuando cambia, WPF busca el DataTemplate que corresponde al tipo
        // del objeto y renderiza la View adecuada automáticamente.
        private ViewModelBase _currentView;
        public ViewModelBase CurrentView
        {
            get => _currentView;
            private set => SetProperty(ref _currentView, value);
        }

        // Instancias de cada ViewModel. Se crean una sola vez y se reutilizan.
        // Así la calculadora no se reinicia al navegar entre vistas.
        private readonly NormalCalcViewModel _normalCalcVm = new();
        private readonly GraphCalcViewModel _graphCalcVm = new();

        // Comandos enlazados a los botones de la barra de navegación.
        public ICommand ShowNormalCalculatorCommand { get; }
        public ICommand ShowGraphCalculatorCommand { get; }

        public MainViewModel()
        {
            ShowNormalCalculatorCommand = new RelayCommand(() => CurrentView = _normalCalcVm);
            ShowGraphCalculatorCommand = new RelayCommand(() => CurrentView = _graphCalcVm);

            // Vista por defecto al arrancar la aplicación.
            _currentView = _normalCalcVm;
        }
    }
}