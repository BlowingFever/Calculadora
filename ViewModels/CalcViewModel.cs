using System;
using System.Globalization;
using System.Windows.Input;
using Calculadora.Common;
using Calculadora.Models;

namespace Calculadora.ViewModels
{
    public class NormalCalcViewModel : ViewModelBase
    {
        // -- Dependencies
        private readonly CalculatorModel _model = new();

        // -- Calculator Internal State
        // Computation tokens: intermediate results get collapsed here for calculation
        private readonly System.Collections.Generic.List<string> _tokens = new();
        // Display tokens: full expression history, never collapsed, only for the Expression label
        private readonly System.Collections.Generic.List<string> _expressionTokens = new();

        private bool _waitingForSecond = true;
        private bool _hasError = false;
        private bool _justEvaluated = false;

        // -- UI Bindings
        private string _displayValue = "0";
        public string Display
        {
            get => _displayValue;
            private set => SetProperty(ref _displayValue, value);
        }

        private string _expression = "";
        public string Expression
        {
            get => _expression;
            private set => SetProperty(ref _expression, value);
        }

        // -- Commands
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

        // -- Button Event Handlers

        private void OnDigit(string? digit)
        {
            if (_hasError || digit == null) return;

            if (_justEvaluated)
            {
                // Start a fresh operation if a digit is typed right after Equals
                ClearTokens();
                Expression = "";
                _justEvaluated = false;
                Display = digit;
                _waitingForSecond = false;
                return;
            }

            if (_waitingForSecond)
            {
                Display = digit;
                _waitingForSecond = false;
            }
            else
            {
                // Limit input length to prevent UI overflow (ignoring formatting symbols)
                if (Display.Replace("-", "").Replace(".", "").Length >= 12) return;
                Display = Display == "0" ? digit : Display + digit;
            }
        }

        private void OnOperator(string? op)
        {
            if (_hasError || op == null) return;

            if (_justEvaluated)
            {
                // Chain operation using the last result as the starting operand
                ClearTokens();
                _tokens.Add(Display);
                _expressionTokens.Add(Display);
                _tokens.Add(op);
                _expressionTokens.Add(op);
                Expression = BuildDisplayExpression();
                _waitingForSecond = true;
                _justEvaluated = false;
                return;
            }

            if (_waitingForSecond)
            {
                // Chaining two operators consecutively triggers a syntax error
                TriggerError();
                return;
            }

            // Push current display value into both lists
            _tokens.Add(Display);
            _expressionTokens.Add(Display);

            // Collapse the computation tokens respecting operator precedence
            if (!CollapseTokens(GetPrecedence(op)))
            {
                TriggerError();
                return;
            }

            // Push the new operator into both lists
            _tokens.Add(op);
            _expressionTokens.Add(op);

            Expression = BuildDisplayExpression();
            _waitingForSecond = true;
        }

        private void OnEquals()
        {
            if (_hasError) return;
            if (_justEvaluated) return;

            if (_waitingForSecond)
            {
                // Pressing equals without a final operand is a syntax error (e.g. "5 + =")
                TriggerError();
                return;
            }

            // Push the final operand into both lists
            _tokens.Add(Display);
            _expressionTokens.Add(Display);

            // Snapshot the full expression for the history label before collapsing
            string fullExpression = BuildDisplayExpression() + " =";

            // Force-collapse everything (precedence 0 evaluates all pending operators)
            if (!CollapseTokens(0))
            {
                TriggerError();
                return;
            }

            if (_tokens.Count != 1 ||
                !double.TryParse(_tokens[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                TriggerError();
                return;
            }

            Display = FormatNumber(result);
            Expression = fullExpression;
            _justEvaluated = true;
            _waitingForSecond = true;
        }

        private void OnClear()
        {
            Display = "0";
            Expression = "";
            ClearTokens();
            _waitingForSecond = true;
            _justEvaluated = false;
            _hasError = false;
        }

        private void OnDecimal()
        {
            if (_hasError) return;

            if (_justEvaluated)
            {
                ClearTokens();
                Expression = "";
                _justEvaluated = false;
                Display = "0.";
                _waitingForSecond = false;
                return;
            }

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
            if (_hasError) return;

            if (_justEvaluated)
            {
                ClearTokens();
                _justEvaluated = false;
            }

            if (Display == "0") return;
            Display = Display.StartsWith("-") ? Display[1..] : "-" + Display;
        }

        private void OnPercent()
        {
            if (_hasError) return;

            if (_justEvaluated)
            {
                ClearTokens();
                _justEvaluated = false;
            }

            if (!ParseDisplay(out double current)) return;
            Display = FormatNumber(current / 100.0);
        }

        // -- Private Utilities

        // Collapses computation tokens whose operator precedence >= targetPrecedence.
        // _expressionTokens is intentionally NOT touched here — it always keeps the full history.
        private bool CollapseTokens(int targetPrecedence)
        {
            while (_tokens.Count >= 3)
            {
                string op = _tokens[_tokens.Count - 2];
                int prec = GetPrecedence(op);
                if (prec < targetPrecedence) break;

                string num2Str = _tokens[_tokens.Count - 1];
                string num1Str = _tokens[_tokens.Count - 3];

                if (!double.TryParse(num1Str, NumberStyles.Any, CultureInfo.InvariantCulture, out double n1) ||
                    !double.TryParse(num2Str, NumberStyles.Any, CultureInfo.InvariantCulture, out double n2))
                    return false;

                double result = _model.Calculate(n1, n2, op);
                if (!double.IsFinite(result) || double.IsNaN(result))
                    return false;

                // Replace the [Operand1, Operator, Operand2] triple with the collapsed Result
                _tokens.RemoveRange(_tokens.Count - 3, 3);
                _tokens.Add(result.ToString(CultureInfo.InvariantCulture));
                Display = FormatNumber(result);
            }
            return true;
        }

        private static int GetPrecedence(string op) => op switch
        {
            "+" or "-" => 1,
            "*" or "/" => 2,
            _ => 0
        };

        // Builds the visible expression string from the full _expressionTokens history
        private string BuildDisplayExpression()
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < _expressionTokens.Count; i++)
            {
                if (i > 0) sb.Append(" ");
                string token = _expressionTokens[i];
                if (i % 2 == 1) // Operator position
                {
                    sb.Append(GetOperatorSymbol(token));
                }
                else // Number position
                {
                    if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
                        sb.Append(FormatNumber(val));
                    else
                        sb.Append(token);
                }
            }
            return sb.ToString();
        }

        // Clears both token lists atomically
        private void ClearTokens()
        {
            _tokens.Clear();
            _expressionTokens.Clear();
        }

        private void TriggerError()
        {
            Display = "Error";
            Expression = "";
            ClearTokens();
            _hasError = true;
            _waitingForSecond = true;
            _justEvaluated = false;
        }

        private bool ParseDisplay(out double value)
            => double.TryParse(Display, NumberStyles.Any, CultureInfo.InvariantCulture, out value);

        private static string FormatNumber(double value)
        {
            if (value == Math.Floor(value) && Math.Abs(value) < 1e15)
                return ((long)value).ToString(CultureInfo.InvariantCulture);
            return value.ToString("G10", CultureInfo.InvariantCulture);
        }

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