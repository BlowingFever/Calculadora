using System;

namespace Calculadora.Models
{
    // Decoupled arithmetic logic that processes calculations independently of WPF/MVVM.
    public class CalculatorModel
    {
        // Performs the operation. Returns double.NaN for invalid operations (e.g., division by zero).
        public double Calculate(double a, double b, string op)
        {
            return op switch
            {
                "+" => a + b,
                "-" => a - b,
                "*" => a * b,
                "/" => b != 0 ? a / b : double.NaN,
                _ => b  // Unknown operator: returns b unchanged
            };
        }
    }
}