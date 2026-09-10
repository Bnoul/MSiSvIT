using System;
using System.Collections.Generic;
using System.Linq;

namespace HalsteadTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Test Program for Halstead Metrics ===");

            var numbers = GenerateNumbers(20);
            var filtered = FilterEven(numbers);
            var squared = SquareNumbers(filtered);
            var sum = SumNumbers(squared);
        }
    }
}
