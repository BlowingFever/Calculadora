using System;

namespace Calculadora.Models
{
    // Lógica aritmética pura.
    // Esta clase NO sabe nada de WPF, ViewModels ni UI.
    // Solo recibe números y operadores, y devuelve resultados.
    public class CalculatorModel
    {
        // Realiza la operación entre dos números.
        // Devuelve double.NaN si la operación es inválida (ej: x / 0).
        public double Calculate(double a, double b, string op)
        {
            return op switch
            {
                "+" => a + b,
                "-" => a - b,
                "*" => a * b,
                "/" => b != 0 ? a / b : double.NaN,
                _ => b  // Operador desconocido: devuelve b sin cambios
            };
        }
    }
}