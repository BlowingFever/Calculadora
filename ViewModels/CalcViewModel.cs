using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Input;
using Calculadora.Common;
using Calculadora.Models;

namespace Calculadora.ViewModels
{
    /// <summary>
    /// ViewModel for the standard (non-graphing) calculator view.
    /// Implements a precedence-aware infix evaluator using two token lists:
    /// one for live computation and one for the full expression history shown on the display.
    /// </summary>
    public class NormalCalcViewModel : ViewModelBase
    {
        // ── Dependencies ──────────────────────────────────────────────────────

        private readonly CalculatorModel _model = new();

        // ── Internal State ────────────────────────────────────────────────────

        /// <summary>
        /// Working tokens used for computation; intermediate results are collapsed here.
        /// </summary>
        private readonly List<string> _tokens = new();

        /// <summary>
        /// Full expression history tokens; never collapsed, used only for the expression label.
        /// </summary>
        private readonly List<string> _expressionTokens = new();

        private bool _waitingForSecond = true;
        private bool _hasError = false;
        private bool _justEvaluated = false;

        // ── UI-Bound Properties ───────────────────────────────────────────────

        private string _displayValue = "0";

        /// <summary>Gets the main numeric display string.</summary>
        public string Display
        {
            get => _displayValue;
            private set => SetProperty(ref _displayValue, value);
        }

        private string _expression = string.Empty;

        /// <summary>Gets the expression history string shown above the main display.</summary>
        public string Expression
        {
            get => _expression;
            private set => SetProperty(ref _expression, value);
        }

        // ── Commands ──────────────────────────────────────────────────────────

        /// <summary>Appends a digit character to the current operand.</summary>
        public ICommand DigitCommand { get; }

        /// <summary>Pushes the current display value and the selected operator onto the token stack.</summary>
        public ICommand OperatorCommand { get; }

        /// <summary>Evaluates the full expression and shows the result.</summary>
        public ICommand EqualsCommand { get; }

        /// <summary>Resets the calculator to its initial state.</summary>
        public ICommand ClearCommand { get; }

        /// <summary>Appends a decimal separator to the current operand.</summary>
        public ICommand DecimalCommand { get; }

        /// <summary>Negates the current operand.</summary>
        public ICommand ToggleSignCommand { get; }

        /// <summary>Divides the current operand by 100.</summary>
        public ICommand PercentCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────

        /// <summary>Initializes all commands.</summary>
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

        // ── Command Handlers ──────────────────────────────────────────────────

        /// <summary>
        /// Appends <paramref name="digit"/> to the display, or starts a fresh entry
        /// if the previous action was <c>=</c>.
        /// </summary>
        /// <param name="digit">Single digit character string (<c>"0"</c>–<c>"9"</c>).</param>
        private void OnDigit(string? digit)
        {
            if (_hasError || digit == null) return;

            if (_justEvaluated)
            {
                // Start a fresh operation after Equals.
                ClearTokens();
                Expression = string.Empty;
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
                // Limit input length to prevent UI overflow (formatting symbols excluded).
                if (Display.Replace("-", "").Replace(".", "").Length >= 12) return;
                Display = Display == "0" ? digit : Display + digit;
            }
        }

        /// <summary>
        /// Pushes the current display value and <paramref name="op"/> onto the token stacks,
        /// collapsing any pending higher-precedence operations first.
        /// </summary>
        /// <param name="op">Operator string: <c>"+"</c>, <c>"-"</c>, <c>"*"</c>, or <c>"/"</c>.</param>
        private void OnOperator(string? op)
        {
            if (_hasError || op == null) return;

            if (_justEvaluated)
            {
                // Chain a new operation from the last result.
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
                // Two consecutive operators without an operand is a syntax error.
                TriggerError();
                return;
            }

            _tokens.Add(Display);
            _expressionTokens.Add(Display);

            if (!CollapseTokens(GetPrecedence(op)))
            {
                TriggerError();
                return;
            }

            _tokens.Add(op);
            _expressionTokens.Add(op);
            Expression = BuildDisplayExpression();
            _waitingForSecond = true;
        }

        /// <summary>
        /// Finalises the expression, evaluates all remaining operators, and shows the result.
        /// Sets <see cref="_justEvaluated"/> so the next digit starts a fresh calculation.
        /// </summary>
        private void OnEquals()
        {
            if (_hasError || _justEvaluated) return;

            if (_waitingForSecond)
            {
                // Equals without a final operand (e.g. "5 + =") is a syntax error.
                TriggerError();
                return;
            }

            _tokens.Add(Display);
            _expressionTokens.Add(Display);

            string fullExpression = BuildDisplayExpression() + " =";

            // Precedence 0 forces evaluation of all pending operators.
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

        /// <summary>Resets the calculator to its default initial state.</summary>
        private void OnClear()
        {
            Display = "0";
            Expression = string.Empty;
            ClearTokens();
            _waitingForSecond = true;
            _justEvaluated = false;
            _hasError = false;
        }

        /// <summary>
        /// Appends a decimal separator to the current operand, or starts a new <c>"0."</c>
        /// entry if waiting for the next operand.
        /// </summary>
        private void OnDecimal()
        {
            if (_hasError) return;

            if (_justEvaluated)
            {
                ClearTokens();
                Expression = string.Empty;
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

        /// <summary>Flips the sign of the current display value.</summary>
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

        /// <summary>Divides the current display value by 100.</summary>
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

        // ── Private Utilities ─────────────────────────────────────────────────

        /// <summary>
        /// Reduces the <see cref="_tokens"/> list by evaluating all pending operators whose
        /// precedence is greater than or equal to <paramref name="targetPrecedence"/>.
        /// <see cref="_expressionTokens"/> is intentionally left untouched.
        /// </summary>
        /// <param name="targetPrecedence">Minimum operator precedence to collapse.</param>
        /// <returns><c>true</c> on success; <c>false</c> if a parse or arithmetic error occurs.</returns>
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
                if (!double.IsFinite(result) || double.IsNaN(result)) return false;

                // Replace the [operand, operator, operand] triple with the computed result.
                _tokens.RemoveRange(_tokens.Count - 3, 3);
                _tokens.Add(result.ToString(CultureInfo.InvariantCulture));
                Display = FormatNumber(result);
            }
            return true;
        }

        /// <summary>Returns the arithmetic precedence level for the given operator.</summary>
        /// <param name="op">Operator string.</param>
        /// <returns>2 for <c>*</c> and <c>/</c>; 1 for <c>+</c> and <c>-</c>; 0 for unknown.</returns>
        private static int GetPrecedence(string op) => op switch
        {
            "+" or "-" => 1,
            "*" or "/" => 2,
            _ => 0
        };

        /// <summary>
        /// Builds the human-readable expression string from <see cref="_expressionTokens"/>,
        /// replacing internal operator symbols with their display counterparts.
        /// </summary>
        /// <returns>Formatted expression string.</returns>
        private string BuildDisplayExpression()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < _expressionTokens.Count; i++)
            {
                if (i > 0) sb.Append(' ');
                string token = _expressionTokens[i];
                if (i % 2 == 1)
                {
                    sb.Append(GetOperatorSymbol(token));
                }
                else
                {
                    if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
                        sb.Append(FormatNumber(val));
                    else
                        sb.Append(token);
                }
            }
            return sb.ToString();
        }

        /// <summary>Clears both token lists atomically.</summary>
        private void ClearTokens()
        {
            _tokens.Clear();
            _expressionTokens.Clear();
        }

        /// <summary>
        /// Transitions the calculator into an error state, clearing all pending tokens.
        /// The user must press <c>C</c> to recover.
        /// </summary>
        private void TriggerError()
        {
            Display = "Error";
            Expression = string.Empty;
            ClearTokens();
            _hasError = true;
            _waitingForSecond = true;
            _justEvaluated = false;
        }

        /// <summary>Attempts to parse the current display string as a <see cref="double"/>.</summary>
        /// <param name="value">Parsed value on success.</param>
        /// <returns><c>true</c> if parsing succeeded.</returns>
        private bool ParseDisplay(out double value)
            => double.TryParse(Display, NumberStyles.Any, CultureInfo.InvariantCulture, out value);

        /// <summary>
        /// Formats a <see cref="double"/> for display, omitting the decimal point for
        /// whole numbers within the safe integer range.
        /// </summary>
        /// <param name="value">Value to format.</param>
        /// <returns>Locale-independent formatted string.</returns>
        private static string FormatNumber(double value)
        {
            if (value == Math.Floor(value) && Math.Abs(value) < 1e15)
                return ((long)value).ToString(CultureInfo.InvariantCulture);
            return value.ToString("G10", CultureInfo.InvariantCulture);
        }

        /// <summary>Maps an internal operator token to its Unicode display symbol.</summary>
        /// <param name="op">Internal operator string.</param>
        /// <returns>Display symbol, e.g. <c>"×"</c> for <c>"*"</c>.</returns>
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