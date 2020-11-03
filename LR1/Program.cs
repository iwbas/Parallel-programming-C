using System;
using System.Threading;

namespace LR1
{
    class Program
    {
        const UInt16 N = 4;
        const String literals = "ABCDF";
        static void Main(string[] args)
        {
            Console.WriteLine("Последовательная обработка");

            for (ushort c = 0; c < N + 1; c++)
            {
                Console.WriteLine("Последовательно программ: " + (c + 1));
                DateTime t1 = DateTime.Now;
                for (ushort k = 0; k <= c; k++)
                {
                    double x = 12345.6789;
                    for (int i = 0; i < 10000; i++)
                    {
                        for (int j = 0; j < 10000; j++)
                        {
                            x = Math.Sqrt(x);
                            x = x + 0.000000001;
                            x = Math.Pow(x, 2);
                        }
                    }
                    Console.WriteLine(literals[k].ToString() + ": " + x.ToString());
                }
                DateTime t2 = DateTime.Now;
                Console.WriteLine(t2 - t1);
            }

            Console.WriteLine("Многопоточная обработка с лямбда выражением");

            multithreading((state) =>
            {
                double x = 12345.6789;

                for (int i = 0; i < 10000; i++)
                {
                    for (int j = 0; j < 10000; j++)
                    {
                        x = Math.Sqrt(x);
                        x = x + 0.000000001;
                        x = Math.Pow(x, 2);
                    }
                }

                Console.WriteLine(Thread.CurrentThread.Name + ": " + x.ToString());

                ManualResetEvent e = state as ManualResetEvent;
                e.Set();
            });

            Console.WriteLine("Многопоточная обработка со статической процедурой");

            multithreading(JobForAThread);

            Console.WriteLine("Многопоточная обработка со статической процедурой и пулом потоков");
            for (ushort i = 0; i < N + 1; i++)
            {
                ushort n = (ushort)(i + 1);
                var events = new ManualResetEvent[n];
                Console.WriteLine("Потоков: " + n);
                DateTime t1 = DateTime.Now;
                for (UInt16 j = 0; j <= i; j++)
                {
                    events[j] = new ManualResetEvent(false);
                    ThreadPool.QueueUserWorkItem(JobForAThread, events[j]);
                }
                WaitHandle.WaitAll(events);
                DateTime t2 = DateTime.Now;
                Console.WriteLine(t2 - t1);
            }

            Console.WriteLine("Многопоточная обработка с разными приоритетами");
            for (ushort i = 0; i < N + 1; i++)
            {
                ushort threadAmount = (ushort)(i + 1);
                var events = new ManualResetEvent[threadAmount];

                Console.WriteLine("Параллельных потоков: " + (threadAmount));

                DateTime t1 = DateTime.Now;

                for (UInt16 j = 0; j < threadAmount; j++)
                {
                    events[j] = new ManualResetEvent(false);
                    Thread thread = new Thread(new ParameterizedThreadStart(JobForAThread));

                    if (j % 2 == 0) 
                        thread.Priority = ThreadPriority.Highest;
                    else
                        thread.Priority = ThreadPriority.Lowest;

                    thread.Name = literals[j].ToString();
                    thread.Start(events[j]);
                }
                WaitHandle.WaitAll(events);

                DateTime t2 = DateTime.Now;
                Console.WriteLine(t2 - t1);
            }
        }

        public static void multithreading(Action<object> func)
        {
            for (ushort i = 0; i < N + 1; i++)
            {
                ushort threadAmount = (ushort)(i + 1);
                var events = new ManualResetEvent[threadAmount];

                Console.WriteLine("Параллельных потоков: " + (threadAmount));

                DateTime t1 = DateTime.Now;

                for (UInt16 j = 0; j < threadAmount; j++)
                {
                    events[j] = new ManualResetEvent(false);

                    Thread thread = new Thread(new ParameterizedThreadStart(func));

                    thread.Name = literals[j].ToString();
                    thread.Start(events[j]);
                }
                WaitHandle.WaitAll(events);

                DateTime t2 = DateTime.Now;
                Console.WriteLine(t2 - t1);
            }
        }

        static void JobForAThread(object state)
        {
            double x = 12345.6789;
            for (int i = 0; i < 10000; i++)
            {
                for (int j = 0; j < 10000; j++)
                {
                    x = Math.Sqrt(x);
                    x = x + 0.000000001;
                    x = Math.Pow(x, 2);
                }
            }
            Console.WriteLine(Thread.CurrentThread.Name + " " + Thread.CurrentThread.ManagedThreadId + ": " + x.ToString());

            ManualResetEvent ev = state as ManualResetEvent;
            ev.Set();
        }
    }
}
