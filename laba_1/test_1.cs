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

            Console.WriteLine("Generated numbers:");
            PrintList(numbers);

            Console.WriteLine("Filtered even numbers:");
            PrintList(filtered);

            Console.WriteLine("Squared numbers:");
            PrintList(squared);

            Console.WriteLine($"Sum of squared even numbers: {sum}");

            var stats = CalculateStatistics(numbers);
            Console.WriteLine($"Min: {stats.min}, Max: {stats.max}, Avg: {stats.avg:F2}");

            var dict = BuildDictionary(numbers);
            Console.WriteLine("Dictionary contents:");
            foreach (var kv in dict)
            {
                Console.WriteLine($"{kv.Key} -> {kv.Value}");
            }

            Console.WriteLine("Program finished.");
        }

        static List<int> GenerateNumbers(int count)
        {
            var list = new List<int>();
            for (int i = 1; i <= count; i++)
            {
                list.Add(i);
            }
            return list;
        }

        static List<int> FilterEven(List<int> list)
        {
            var result = new List<int>();
            foreach (var n in list)
            {
                if (n % 2 == 0)
                    result.Add(n);
            }
            return result;
        }

        static List<int> SquareNumbers(List<int> list)
        {
            var result = new List<int>();
            foreach (var n in list)
            {
                result.Add(n * n);
            }
            return result;
        }

        static int SumNumbers(List<int> list)
        {
            int sum = 0;
            foreach (var n in list)
            {
                sum += n;
            }
            return sum;
        }

        static (int min, int max, double avg) CalculateStatistics(List<int> list)
        {
            int min = int.MaxValue;
            int max = int.MinValue;
            int sum = 0;

            foreach (var n in list)
            {
                if (n < min) min = n;
                if (n > max) max = n;
                sum += n;
            }

            double avg = (double)sum / list.Count;
            return (min, max, avg);
        }

        static Dictionary<int, string> BuildDictionary(List<int> list)
        {
            var dict = new Dictionary<int, string>();
            foreach (var n in list)
            {
                dict[n] = $"Number_{n}";
            }
            return dict;
        }

        static void PrintList(List<int> list)
        {
            foreach (var n in list)
            {
                Console.Write(n + " ");
            }
            Console.WriteLine();
        }
    }
}
