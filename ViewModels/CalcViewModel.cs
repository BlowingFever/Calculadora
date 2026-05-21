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
    /// ViewModel de la calculadora estándar (no gráfica).
    /// Implementa un evaluador de expresiones infijas con precedencia de operadores
    /// mediante dos listas de tokens paralelas: una para el cómputo y otra para
    /// el historial visual que se muestra encima del display.
    /// </summary>
    /// <remarks>
    /// Algoritmo de evaluación:
    /// <list type="number">
    ///   <item><description>Cada dígito se acumula en <see cref="Display"/>.</description></item>
    ///   <item><description>Al pulsar un operador, el display se añade a <c>_tokens</c> y se llama
    ///     a <see cref="CollapseTokens"/> para reducir cualquier operación pendiente de mayor o igual
    ///     precedencia.</description></item>
    ///   <item><description>Al pulsar <c>=</c>, se fuerza la reducción completa (precedencia 0)
    ///     y se muestra el resultado.</description></item>
    /// </list>
    /// La pila <c>_expressionTokens</c> nunca se colapsa: sólo sirve para construir
    /// la cadena de historial que ve el usuario.
    /// </remarks>
    public class NormalCalcViewModel : ViewModelBase
    {
        // ── Dependencias ──────────────────────────────────────────────────────

        /// <summary>Motor aritmético que realiza las operaciones básicas.</summary>
        private readonly CalculatorModel _model = new();

        // ── Estado interno ────────────────────────────────────────────────────

        /// <summary>
        /// Lista de tokens de trabajo usada para el cómputo.
        /// Formato alternante: [número, operador, número, operador, …].
        /// Los resultados intermedios reemplazan los tripletes reducidos.
        /// </summary>
        private readonly List<string> _tokens = new();

        /// <summary>
        /// Lista de tokens de historial; nunca se colapsa.
        /// Se usa exclusivamente para construir <see cref="Expression"/>.
        /// </summary>
        private readonly List<string> _expressionTokens = new();

        /// <summary>
        /// <c>true</c> cuando el display espera el inicio de un segundo operando.
        /// En este estado, pulsar un dígito reemplaza el display en lugar de concatenar.
        /// </summary>
        private bool _waitingForSecond = true;

        /// <summary><c>true</c> cuando la calculadora está en estado de error y bloquea la entrada.</summary>
        private bool _hasError = false;

        /// <summary>
        /// <c>true</c> justo después de pulsar <c>=</c>.
        /// El primer dígito siguiente inicia una nueva operación; un operador encadena desde el resultado.
        /// </summary>
        private bool _justEvaluated = false;

        // ── Propiedades enlazadas ─────────────────────────────────────────────

        private string _displayValue = "0";

        /// <summary>
        /// Obtiene la cadena numérica principal que se muestra en el display.
        /// </summary>
        public string Display
        {
            get => _displayValue;
            private set => SetProperty(ref _displayValue, value);
        }

        private string _expression = string.Empty;

        /// <summary>
        /// Obtiene la expresión acumulada que se muestra encima del display principal.
        /// Ejemplo: <c>"12 + 34 × 5 ="</c>.
        /// </summary>
        public string Expression
        {
            get => _expression;
            private set => SetProperty(ref _expression, value);
        }

        // ── Comandos ──────────────────────────────────────────────────────────

        /// <summary>Añade un dígito al operando actual en el display.</summary>
        public ICommand DigitCommand { get; }

        /// <summary>
        /// Registra el operando actual y el operador elegido, reduciendo
        /// las operaciones pendientes de mayor o igual precedencia.
        /// </summary>
        public ICommand OperatorCommand { get; }

        /// <summary>Evalúa la expresión completa y muestra el resultado final.</summary>
        public ICommand EqualsCommand { get; }

        /// <summary>Resetea la calculadora a su estado inicial.</summary>
        public ICommand ClearCommand { get; }

        /// <summary>Añade el separador decimal al operando actual.</summary>
        public ICommand DecimalCommand { get; }

        /// <summary>Invierte el signo del valor mostrado en el display.</summary>
        public ICommand ToggleSignCommand { get; }

        /// <summary>Divide el valor del display entre 100 (convierte a porcentaje).</summary>
        public ICommand PercentCommand { get; }

        // ── Constructor ───────────────────────────────────────────────────────

        /// <summary>
        /// Inicializa todos los comandos de la calculadora estándar.
        /// </summary>
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

        // ── Manejadores de comandos ───────────────────────────────────────────

        /// <summary>
        /// Agrega el dígito <paramref name="digit"/> al display.
        /// Si se acaba de evaluar una expresión, descarta el historial y comienza de nuevo.
        /// Si la calculadora espera el segundo operando, el dígito reemplaza el display en lugar
        /// de concatenarse.
        /// </summary>
        /// <param name="digit">Carácter de dígito, de <c>"0"</c> a <c>"9"</c>.</param>
        private void OnDigit(string? digit)
        {
            if (_hasError || digit == null) return;

            if (_justEvaluated)
            {
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
                // Limita la longitud del operando para evitar desbordamiento visual.
                if (Display.Replace("-", "").Replace(".", "").Length >= 12) return;
                Display = Display == "0" ? digit : Display + digit;
            }
        }

        /// <summary>
        /// Registra el operando actual en los stacks de tokens y añade el operador
        /// <paramref name="op"/>. Antes de añadir el operador, colapsa las operaciones
        /// pendientes cuya precedencia sea mayor o igual a la del nuevo operador.
        /// </summary>
        /// <param name="op">Operador aritmético: <c>"+"</c>, <c>"-"</c>, <c>"*"</c> o <c>"/"</c>.</param>
        private void OnOperator(string? op)
        {
            if (_hasError || op == null) return;

            if (_justEvaluated)
            {
                // Encadena una nueva operación partiendo del último resultado.
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
                // Dos operadores seguidos sin operando intermedio es un error de sintaxis.
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
        /// Finaliza la expresión, evalúa todos los operadores pendientes (precedencia 0)
        /// y muestra el resultado. Activa <see cref="_justEvaluated"/> para que el
        /// siguiente dígito inicie un cálculo nuevo.
        /// </summary>
        private void OnEquals()
        {
            if (_hasError || _justEvaluated) return;

            if (_waitingForSecond)
            {
                // Pulsar '=' sin un operando final (p.ej. "5 + =") es un error.
                TriggerError();
                return;
            }

            _tokens.Add(Display);
            _expressionTokens.Add(Display);

            string fullExpression = BuildDisplayExpression() + " =";

            // Precedencia 0 fuerza la evaluación de todos los operadores pendientes.
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

        /// <summary>
        /// Restaura la calculadora a su estado inicial: display <c>"0"</c>,
        /// expresión vacía y sin errores.
        /// </summary>
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
        /// Añade el separador decimal al operando actual. Si el display ya contiene
        /// un punto no añade otro. Si se está esperando el segundo operando, comienza
        /// con <c>"0."</c>.
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

        /// <summary>
        /// Invierte el signo del valor actual en el display.
        /// Si el valor es <c>"0"</c>, no hace nada.
        /// </summary>
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

        /// <summary>
        /// Divide el valor actual del display entre 100.
        /// Útil para calcular porcentajes directamente.
        /// </summary>
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

        // ── Utilidades privadas ───────────────────────────────────────────────

        /// <summary>
        /// Reduce la lista <see cref="_tokens"/> evaluando todos los operadores cuya
        /// precedencia sea mayor o igual a <paramref name="targetPrecedence"/>.
        /// La lista <see cref="_expressionTokens"/> no se modifica.
        /// </summary>
        /// <param name="targetPrecedence">
        /// Umbral de precedencia: sólo se colapsan operadores con este nivel o superior.
        /// Pasar <c>0</c> colapsa absolutamente todos los operadores pendientes.
        /// </param>
        /// <returns>
        /// <c>true</c> si la reducción se completó sin errores;
        /// <c>false</c> si ocurrió un error de parseo o una operación inválida (p.ej. división por cero).
        /// </returns>
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

                // Sustituye el triplete [num1, op, num2] por el resultado calculado.
                _tokens.RemoveRange(_tokens.Count - 3, 3);
                _tokens.Add(result.ToString(CultureInfo.InvariantCulture));
                Display = FormatNumber(result);
            }
            return true;
        }

        /// <summary>
        /// Devuelve el nivel de precedencia aritmética del operador dado.
        /// </summary>
        /// <param name="op">Cadena de operador.</param>
        /// <returns>
        /// <c>2</c> para <c>*</c> y <c>/</c>;
        /// <c>1</c> para <c>+</c> y <c>-</c>;
        /// <c>0</c> para cualquier otro valor.
        /// </returns>
        private static int GetPrecedence(string op) => op switch
        {
            "+" or "-" => 1,
            "*" or "/" => 2,
            _ => 0
        };

        /// <summary>
        /// Construye la cadena de expresión legible a partir de <see cref="_expressionTokens"/>,
        /// sustituyendo los símbolos internos de operador por sus equivalentes Unicode.
        /// </summary>
        /// <returns>Cadena formateada lista para mostrar al usuario.</returns>
        private string BuildDisplayExpression()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < _expressionTokens.Count; i++)
            {
                if (i > 0) sb.Append(' ');
                string token = _expressionTokens[i];
                if (i % 2 == 1) // Los tokens en posición impar son operadores.
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

        /// <summary>Vacía atómicamente las dos listas de tokens.</summary>
        private void ClearTokens()
        {
            _tokens.Clear();
            _expressionTokens.Clear();
        }

        /// <summary>
        /// Pone la calculadora en estado de error: limpia los tokens,
        /// muestra <c>"Error"</c> en el display y bloquea la entrada.
        /// El usuario debe pulsar <c>C</c> para recuperarse del error.
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

        /// <summary>
        /// Intenta convertir la cadena actual del display a un <see cref="double"/>.
        /// </summary>
        /// <param name="value">Valor parseado si la conversión tiene éxito.</param>
        /// <returns><c>true</c> si el parseo fue correcto; <c>false</c> en caso contrario.</returns>
        private bool ParseDisplay(out double value)
            => double.TryParse(Display, NumberStyles.Any, CultureInfo.InvariantCulture, out value);

        /// <summary>
        /// Formatea un número <see cref="double"/> para mostrarlo en el display.
        /// Los números enteros dentro del rango seguro se muestran sin punto decimal.
        /// </summary>
        /// <param name="value">Valor a formatear.</param>
        /// <returns>Cadena formateada independiente de la cultura local.</returns>
        private static string FormatNumber(double value)
        {
            if (value == Math.Floor(value) && Math.Abs(value) < 1e15)
                return ((long)value).ToString(CultureInfo.InvariantCulture);
            return value.ToString("G10", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Mapea el símbolo interno del operador al carácter Unicode correspondiente
        /// para la cadena de historial.
        /// </summary>
        /// <param name="op">Operador interno (<c>"*"</c>, <c>"/"</c>, etc.).</param>
        /// <returns>Símbolo de presentación (<c>"×"</c>, <c>"÷"</c>, etc.).</returns>
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