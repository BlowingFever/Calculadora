using System;

namespace Calculadora.Models
{
    /// <summary>
    /// Stateless arithmetic engine, fully decoupled from WPF and MVVM infrastructure.
    /// </summary>
    public class CalculatorModel
    {
        /// <summary>
        /// Performs the arithmetic operation identified by <paramref name="op"/> on the two operands.
        /// </summary>
        /// <param name="a">Left-hand operand.</param>
        /// <param name="b">Right-hand operand.</param>
        /// <param name="op">Operator string: one of <c>"+"</c>, <c>"-"</c>, <c>"*"</c>, <c>"/"</c>.</param>
        /// <returns>
        /// The computed result, or <see cref="double.NaN"/> for invalid operations
        /// (e.g. division by zero). Unknown operators return <paramref name="b"/> unchanged.
        /// </returns>
        public double Calculate(double a, double b, string op) => op switch
        {
            "+" => a + b,
            "-" => a - b,
            "*" => a * b,
            "/" => b != 0 ? a / b : double.NaN,
            _ => b
        };
    }
}