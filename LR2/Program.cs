using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace LR2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("========================= Лабораторная работа 2. Выполнил студент ДИПРБ-41 Дабысов А.С. =========================");
            var N = new List<int>() { 10, 100, 1000, 100000 }; // число элементов вектора
            var M = new List<int>() { 2, 3, 4, 5, 10 };        // число потоков

            Console.WriteLine("=================================================================================================================");
            Console.WriteLine("Анализ эффективности многопоточной обработки");
            Console.WriteLine("при разных параметрах N (10, 100, 1000, 100000) и M (2, 3, 4, 5, 10)");

            for (int i = 0; i < N.Count; i++) {
                int n = N[i];
                Console.WriteLine("============ " + "N: " + n + " ============");

                var list = new List<int>(Enumerable.Range(0, n));

                // 1. Последовательная обработка элементов вектора
                Console.WriteLine("Последовательная:");
                Console.WriteLine("Total time: " + Measure(PowerList, list, 0, n).ToString());

                // 2. Многопоточная обработка элементов вектора, 
                //    используя разделение вектора на равное число элементов. 

                Console.WriteLine("2. Многопоточная:");

                for (int j = 0; j < M.Count; j++)
                {
                    int m = M[j];
                    Console.WriteLine("===== M: " + m.ToString() + " =====");
                    Console.WriteLine("Total time: " + Measure(Parallelize, list, n, m, (Action<object[]>)PowerList).ToString());
                }

                // 4. Анализ эффективности при усложнении обработки каждого элемента вектора.

                int complexity = 50;

                Console.WriteLine("Усложненная обработка элементов вектора (k = " + complexity + ")");

                // Последовательная обработка

                Console.WriteLine("1. Последовательная:");
                Console.WriteLine("Total time: " + Measure(ComplexPowerList, list, 0, n, complexity).ToString());

                // Многоядерная обработка

                Console.WriteLine("2. Многопоточная:");

                for (int j = 0; j < M.Count; j++)
                {
                    int m = M[j];
                    Console.WriteLine("===== M: " + m.ToString() + " =====");
                    Console.WriteLine("Total time: " + Measure(Parallelize, list, n, m, (Action<object[]>)ComplexPowerList, complexity).ToString());
                }

                // Исследуйте эффективность разделения по диапазону при неравномерной вычислительной сложности обработки элементов вектора.

                Console.WriteLine("Неравномерная вычислительная сложность обработки элементов вектора");

                // Последовательная обработка

                Console.WriteLine("Последовательная:");
                Console.WriteLine("Total time: " + Measure(UnevenComplexPowerList, list, 0, n).ToString());

                // Многоядерная обработка

                Console.WriteLine("Многопоточная:");

                for (int j = 0; j < M.Count; j++)
                {
                    int m = M[j];
                    Console.WriteLine("===== M: " + m.ToString() + " =====");
                    Console.WriteLine("Total time: " + Measure(Parallelize, list, n, m, (Action<object[]>)UnevenComplexPowerList).ToString());
                }

                // Круговое разделение
                Console.WriteLine("Круговое разделение");


                for (int j = 0; j < M.Count; j++)
                {
                    int m = M[j];
                    Console.WriteLine("===== M: " + m.ToString() + " =====");

                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    var events = new ManualResetEvent[m];

                    for (int k = 0; k < m; k++)
                    {
                        int start = k;
                        var e = events[k] = new ManualResetEvent(false);

                        Thread thread = new Thread(() =>
                        {
                            for (int i = start; i < n; i+=m)
                            {
                                for (int j = 0; j <= i; j++)
                                    list[i] = (int)Math.Pow(list[i], 2);
                            }
                            e.Set();
                        });

                        thread.Name = k.ToString();
                        thread.Start();
                    }

                    WaitHandle.WaitAll(events);

                    sw.Stop();
                    TimeSpan ts = sw.Elapsed;
                    Console.WriteLine("Total time: " + ts.TotalMilliseconds);
                }
            }
            Console.ReadLine();
        }

        static double Measure(Action<object[]> action, params object[] args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            action(args);

            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            return ts.TotalMilliseconds;
        }

        static void Parallelize(params object[] args)
        {
            var list = args[0] as List<int>;
            int n = (int)args[1];
            int m = (int)args[2];

            var action = args[3] as Action<object[]>;
            var K = args.Length == 5 ? args[4] : null;

            int blockSize = n / m;
            var events = new ManualResetEvent[m];

            for (int k = 0; k < m; k++)
            {
                int start = k * blockSize;
                int end = (k * blockSize + blockSize);
                if (k == m - 1)
                    end = n;

                var e = events[k] = new ManualResetEvent(false);

                List<object> actionArgs = new List<object> { list, start, end };

                if (K != null) actionArgs.Add(K);

                Thread thread = new Thread(() =>
                {
                    action(actionArgs.ToArray());
                    e.Set();
                });
                thread.Name = k.ToString();

                thread.Start();
            }

            WaitHandle.WaitAll(events);
        }

        static void PowerList(params object[] args)
        {
            var list = args[0] as List<int>;
            int start = (int)args[1];
            int end = (int)args[2];

            for (int i = start; i < end; i++)
                list[i] = (int)Math.Pow(list[i], 2);
        }

        static void ComplexPowerList(params object[] args)
        {
            var list = args[0] as List<int>;
            var start = (int)args[1];
            var end = (int)args[2];
            var K = (int)args[3];

            for (int i = start; i < end; i++)
                for (int j = 0; j < K; j++)
                    list[i] = (int)Math.Pow(list[i], 2);
        }

        static void UnevenComplexPowerList(params object[] args)
        {
            var list = args[0] as List<int>;
            var start = (int)args[1];
            var end = (int)args[2];

            for (int i = start; i < end; i++)
                for (int j = 0; j <= i; j++)
                    list[i] = (int)Math.Pow(list[i], 2);
        }
    }
}