using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace System.FrameworkExtensions.Tests
{
    [TestFixture]
    public sealed class TaskSchedulerTests
    {
        public class N1Test
        {
            private volatile int _done = 0;

            public void CreateN1Thread(TaskScheduler taskScheduler, int threadcount, string message)
            {
                _done = 0;
                const int totalworkdone = 256*10;
                var are = new AutoResetEvent(false);

                var threads = new Thread[threadcount];
                for (int i = 0; i < threadcount; ++i)
                    threads[i] = new Thread(o =>
                    {
                        var ts = (TaskScheduler) o;
                        for (int j = 0; j < totalworkdone/threadcount; j++)
                        {
                            Task.Factory.StartNew(() =>
                            {
                                for (int k = 0; k < 1000; ++k)
                                    DoSomethingStupid();
                                var r = new Random().Next()%3;
                                if (r != 0)
                                    Thread.Sleep(r - 1);

                                ++_done;
                                if (_done == totalworkdone)
                                    are.Set();
                            }, CancellationToken.None, TaskCreationOptions.None, ts)
                                .ContinueWith(x => { }, CancellationToken.None,
                                    TaskContinuationOptions.ExecuteSynchronously, ts);
                        }
                    });

                var sw = Stopwatch.StartNew();
                for (int i = 0; i < threadcount; ++i)
                    threads[i].Start(taskScheduler);

                are.WaitOne();
                sw.Stop();
                Console.WriteLine("Time taken to complete {0}-{1}: {2}ms {3}ticks",
                    message, threadcount, sw.ElapsedMilliseconds, sw.ElapsedTicks);
            }
        }

        public class AsyncQueueTest
        {
            private volatile int _done = 0;

            public void CreateThread(AsyncQueue asyncQueue, int threadcount, string message)
            {
                _done = 0;
                const int totalworkdone = 256*10;
                var are = new AutoResetEvent(false);

                var threads = new Thread[threadcount];
                for (int i = 0; i < threadcount; ++i)
                    threads[i] = new Thread(o =>
                    {
                        var aq = (AsyncQueue) o;
                        for (int j = 0; j < totalworkdone/threadcount; j++)
                        {
                            aq.Enqueue(() =>
                            {
                                for (int k = 0; k < 1000; ++k)
                                    DoSomethingStupid();
                                var r = new Random().Next()%3;
                                if (r != 0)
                                    Thread.Sleep(r - 1);

                                ++_done;
                                if (_done == totalworkdone)
                                    are.Set();
                            });
                        }
                    });

                Stopwatch sw = Stopwatch.StartNew();
                for (int i = 0; i < threadcount; ++i)
                    threads[i].Start(asyncQueue);

                are.WaitOne();
                sw.Stop();
                Console.WriteLine("Time taken to complete {0}-{1}: {2}ms {3}ticks",
                    message, threadcount, sw.ElapsedMilliseconds, sw.ElapsedTicks);
            }
        }

        private delegate void CreateThreadFunc(TaskScheduler taskScheduler, int threadcount, string message);

        private static void CreateNmThread(TaskScheduler taskScheduler, int threadcount, string message)
        {
            int done = 0;
            const int totalworkdone = 256*10;
            var are = new AutoResetEvent(false);

            var threads = new Thread[threadcount];
            for (int i = 0; i < threadcount; ++i)
                threads[i] = new Thread(o =>
                {
                    var ts = (TaskScheduler) o;
                    for (int j = 0; j < totalworkdone/threadcount; j++)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            for (int k = 0; k < 1000; ++k)
                                DoSomethingStupid();
                            var r = new Random().Next()%3;
                            if (r != 0)
                                Thread.Sleep(r - 1);

                            Interlocked.Increment(ref done);
                            if (done == totalworkdone)
                                are.Set();
                        }, CancellationToken.None, TaskCreationOptions.None, ts)
                            .ContinueWith(x => { }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously,
                                ts);
                    }
                });

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < threadcount; ++i)
                threads[i].Start(taskScheduler);

            are.WaitOne();
            sw.Stop();
            Console.WriteLine("Time taken to complete {0}-{1}: {2}ms {3}ticks",
                message, threadcount, sw.ElapsedMilliseconds, sw.ElapsedTicks);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void DoSomethingStupid()
        {
            var i = 0;
            ++i;
            if (i == 1)
                return;
            return;
        }

        private static void CreateTest(TaskScheduler taskScheduler, CreateThreadFunc func)
        {
            string name = taskScheduler.GetType().Name;

            Console.WriteLine("********************{0}********************", name);
            func(taskScheduler, 2, name);
            func(taskScheduler, 4, name);
            func(taskScheduler, 8, name);
            func(taskScheduler, 16, name);
            func(taskScheduler, 32, name);
            func(taskScheduler, 64, name);
            func(taskScheduler, 128, name);
            func(taskScheduler, 256, name);
            Console.WriteLine("*****************************************************************************");
            Console.WriteLine();
            GC.Collect();
        }

        private static void CreateTest()
        {
            var aqtest = new AsyncQueueTest();
            var async = new AsyncQueue();
            string name = typeof (AsyncQueue).Name;

            Console.WriteLine("********************{0}********************", name);
            aqtest.CreateThread(async, 2, name);
            aqtest.CreateThread(async, 4, name);
            aqtest.CreateThread(async, 8, name);
            aqtest.CreateThread(async, 16, name);
            aqtest.CreateThread(async, 32, name);
            aqtest.CreateThread(async, 64, name);
            aqtest.CreateThread(async, 128, name);
            aqtest.CreateThread(async, 256, name);
            Console.WriteLine("*****************************************************************************");
            Console.WriteLine();
            GC.Collect();
        }

        [Test, Ignore]
        public void N1TaskScheduler()
        {
            var o = new N1TaskScheduler();
            var test = new N1Test();
            CreateTest(o, test.CreateN1Thread);
            GC.KeepAlive(o);
        }

        [Test, Ignore]
        public void NmTaskScheduler()
        {
            var o = new NmTaskScheduler(8);
            CreateTest(o, CreateNmThread);
            GC.KeepAlive(o);
        }

        [Test]
        public void AsyncQueue()
        {
            CreateTest();
        }
    }
}