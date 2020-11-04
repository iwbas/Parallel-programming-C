using System;
using System.Collections.Generic;
using System.Linq;

namespace LR3
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Лабораторная работа №3. Простые числа.");
            Console.WriteLine("Выполнил студент ДИПРБ-41 Дабысов А.С.");

            int n = 20;
            List<int> numbers = new List<int>(Enumerable.Range(2, n));
            List<int> primeNumbers = new List<int>(capacity: numbers.Count);

            // Последовательное решето Эратосфена
            for (int m = 2; m < Math.Sqrt(n); m++)
            {
                
            }

        }
    }
}
