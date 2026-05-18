using System;
using System.Globalization;
using System.Windows.Input;
using Calculadora.Common;
using Calculadora.Models;

namespace Calculadora.ViewModels
{
    public class NormalCalcViewModel : ViewModelBase
    {
        // ── Dependencias ─────────────────────────────────────────────────────
        private readonly CalculatorModel _model = new();

        // ── Estado interno de la calculadora ────────────────────────────────
        private double _pendingOperand = 0;   // Primer número de la operación
        private string _pendingOperator = "";  // El operador (+, -, *, /)
        private bool _waitingForSecond = true; // ¿El próximo dígito empieza número nuevo?
        private bool _hasError = false;

        // ── Propiedades enlazadas a la UI ────────────────────────────────────

        // Línea grande: el número que se está escribiendo o el resultado.
        private string _displayValue = "0";
        public string Display
        {
            get => _displayValue;
            private set => SetProperty(ref _displayValue, value);
        }

        // Línea pequeña sobre el display: muestra la operación en curso ("12 +").
        private string _expression = "";
        public string Expression
        {
            get => _expression;
            private set => SetProperty(ref _expression, value);
        }

        // ── Comandos ─────────────────────────────────────────────────────────
        public ICommand DigitCommand { get; }
        public ICommand OperatorCommand { get; }
        public ICommand EqualsCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand DecimalCommand { get; }
        public ICommand ToggleSignCommand { get; }
        public ICommand PercentCommand { get; }

        public NormalCalcViewModel()
        {
            DigitCommand = new RelayCommand<string>(OnDigit);
            OperatorCommand = new RelayCommand<string>(OnOperator);
            EqualsCommand = new RelayCommand(OnEquals);
            ClearCommand = new RelayCommand(OnClear);
            DecimalCommand = new RelayCommand(OnDecimal);
            ToggleSignCommand = new RelayCommand(OnToggleSign);
            PercentCommand = new RelayCommand(OnPercent);
        }

        // ── Lógica de cada botón ─────────────────────────────────────────────

        private void OnDigit(string? digit)
        {
            if (_hasError || digit == null) return;

            if (_waitingForSecond)
            {
                // Empezamos un número nuevo
                Display = digit == "0" ? "0" : digit;
                _waitingForSecond = false;

                // Si no hay operador pendiente, limpiamos la expresión (inicio fresco)
                if (string.IsNullOrEmpty(_pendingOperator))
                    Expression = "";
            }
            else
            {
                // Limitamos la longitud para no desbordar el display
                if (Display.Replace("-", "").Replace(".", "").Length >= 12) return;
                Display = Display == "0" ? digit : Display + digit;
            }
        }

        private void OnOperator(string? op)
        {
            if (_hasError || op == null) return;

            // Caso: usuario cambia el operador antes de escribir el segundo número
            if (_waitingForSecond && !string.IsNullOrEmpty(_pendingOperator))
            {
                _pendingOperator = op;
                Expression = $"{FormatNumber(_pendingOperand)} {GetOperatorSymbol(op)}";
                return;
            }

            if (!ParseDisplay(out double current)) return;

            if (!string.IsNullOrEmpty(_pendingOperator) && !_waitingForSecond)
            {
                // Cálculo encadenado: ya había una operación pendiente
                double result = _model.Calculate(_pendingOperand, current, _pendingOperator);
                if (!IsValidResult(result)) return;
                Display = FormatNumber(result);
                _pendingOperand = result;
            }
            else
            {
                // Primer operador: guardamos el número actual
                _pendingOperand = current;
            }

            _pendingOperator = op;
            Expression = $"{FormatNumber(_pendingOperand)} {GetOperatorSymbol(op)}";
            _waitingForSecond = true;
        }

        private void OnEquals()
        {
            if (_hasError || string.IsNullOrEmpty(_pendingOperator)) return;
            if (!ParseDisplay(out double current)) return;

            double result = _model.Calculate(_pendingOperand, current, _pendingOperator);
            if (!IsValidResult(result)) return;

            // Mostramos la expresión completa antes del resultado
            Expression = $"{FormatNumber(_pendingOperand)} {GetOperatorSymbol(_pendingOperator)} {FormatNumber(current)} =";
            Display = FormatNumber(result);

            // El resultado queda listo como primer operando para la siguiente operación
            _pendingOperand = result;
            _pendingOperator = "";
            _waitingForSecond = true;
        }

        private void OnClear()
        {
            Display = "0";
            Expression = "";
            _pendingOperand = 0;
            _pendingOperator = "";
            _waitingForSecond = true;
            _hasError = false;
        }

        private void OnDecimal()
        {
            if (_hasError) return;

            if (_waitingForSecond)
            {
                Display = "0.";
                _waitingForSecond = false;
                return;
            }

            if (!Display.Contains('.'))
                Display += ".";
        }

        private void OnToggleSign()
        {
            if (_hasError || Display == "0") return;
            Display = Display.StartsWith("-") ? Display[1..] : "-" + Display;
        }

        private void OnPercent()
        {
            if (_hasError) return;
            if (!ParseDisplay(out double current)) return;
            Display = FormatNumber(current / 100.0);
        }

        // ── Utilidades privadas ──────────────────────────────────────────────

        // Intenta convertir el display actual a double.
        // Usamos InvariantCulture porque nosotros controlamos el punto decimal.
        private bool ParseDisplay(out double value)
            => double.TryParse(Display, NumberStyles.Any, CultureInfo.InvariantCulture, out value);

        // Valida el resultado y gestiona errores (ej: división por cero).
        private bool IsValidResult(double result)
        {
            if (!double.IsNaN(result) && !double.IsInfinity(result)) return true;

            Display = "Error";
            Expression = "";
            _hasError = true;
            return false;
        }

        // Convierte un double a string limpio, sin decimales innecesarios.
        private static string FormatNumber(double value)
        {
            if (value == Math.Floor(value) && Math.Abs(value) < 1e15)
                return ((long)value).ToString(CultureInfo.InvariantCulture);

            return value.ToString("G10", CultureInfo.InvariantCulture);
        }

        // Convierte el operador interno al símbolo visual que se muestra.
        private static string GetOperatorSymbol(string op) => op switch
        {
            "+" => "+",
            "-" => "−",
            "*" => "×",
            "/" => "÷",
            _ => op
        };
    }
}