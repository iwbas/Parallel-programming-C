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
            Console.WriteLine("Лабораторная работа 2. Выполнил студент ДИПРБ-41 Дабысов А.С.");
            var N = new List<int>() { 10, 100, 1000, 100000, 10000000 }; // число элементов вектора
            var M = new List<int>() { 2, 3, 4, 5, 10 };        // число потоков

            Console.WriteLine("=================================================================================================================");
            Console.WriteLine("Анализ эффективности многопоточной обработки при разных параметрах N (10, 100, 1000, 100000) и M (2, 3, 4, 5, 10)");

            for (int i = 0; i < N.Count; i++) {
                int n = N[i];
                Console.WriteLine("============ " + "N: " + n + " ============");

                for (int j = 0; j < M.Count; j++)  {
                    int m = M[j];
                    Console.WriteLine("============ " + "M: " + m + " ============");

                    var list = new List<int>(Enumerable.Range(0, n));

                    // 1. Последовательная обработка элементов вектора
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    for (int k = 0; k < n; k++)
                        list[k] = (int)Math.Pow(list[k], 2);

                    sw.Stop();
                    TimeSpan ts = sw.Elapsed;
                    Console.WriteLine("1. Total time: {0}", ts.TotalMilliseconds);

                    // 2. Многопоточная обработка элементов вектора, 
                    //    используя разделение вектора на равное число элементов. 

                    sw.Start();

                    int blockSize = n / m;
                    var events = new ManualResetEvent[m];

                    for (int k = 0; k < m; k++)
                    {
                        int start = k * blockSize;
                        int end = (k * blockSize + blockSize);
                        if (end > n)
                            end = n;

                        var e = events[k] = new ManualResetEvent(false);
                        
                        Thread thread = new Thread(() => PowerList(list, start, end, e));
                        thread.Name = k.ToString();

                        thread.Start();
                    }

                    WaitHandle.WaitAll(events);

                    sw.Stop();
                    ts = sw.Elapsed;
                    Console.WriteLine("2. Total time: {0}", ts.TotalMilliseconds);

                    // 4. Анализ эффективности при усложнении обработки каждого элемента вектора.

                }
            }
            Console.ReadLine();
        }

        static void PowerList(List<int> list, int start, int end, ManualResetEvent state)
        {
            for (int i = start; i < end; i++)
                list[i] = (int)Math.Pow(list[i], 2);

            state.Set();
        }
    }
}