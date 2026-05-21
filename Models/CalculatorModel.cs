using System;

namespace Calculadora.Models
{
    /// <summary>
    /// Stateless arithmetic engine, fully decoupled from WPF and the MVVM infrastructure.
    /// Provides a single calculation operation that can be reused by any ViewModel.
    /// </summary>
    /// <remarks>
    /// Because this class holds no mutable state it is safe to create a single instance
    /// and share it across multiple ViewModels.
    /// </remarks>
    public class CalculatorModel
    {
        /// <summary>
        /// Performs the arithmetic operation identified by <paramref name="op"/>
        /// on the two supplied operands.
        /// </summary>
        /// <param name="a">Left-hand operand.</param>
        /// <param name="b">Right-hand operand.</param>
        /// <param name="op">
        /// Operator string. Recognised values:
        /// <list type="table">
        ///   <listheader><term>Value</term><description>Operation</description></listheader>
        ///   <item><term><c>"+"</c></term><description>Addition</description></item>
        ///   <item><term><c>"-"</c></term><description>Subtraction</description></item>
        ///   <item><term><c>"*"</c></term><description>Multiplication</description></item>
        ///   <item><term><c>"/"</c></term><description>Division — returns <see cref="double.NaN"/> when <paramref name="b"/> is zero.</description></item>
        /// </list>
        /// </param>
        /// <returns>
        /// The computed result, or <see cref="double.NaN"/> for division by zero.
        /// Unknown operators return <paramref name="b"/> unchanged.
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