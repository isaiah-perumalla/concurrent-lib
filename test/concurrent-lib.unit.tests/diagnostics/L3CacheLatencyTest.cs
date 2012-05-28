using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace concurrent_lib.unit.tests.diagnostics
{
    [TestFixture]
    public class L3CacheLatencyTest
    {
         
        [Test, Description("measure approximate time taken to access L3 shared cache")]
        public void MeasureL3CacheLatency()
        {
            CountdownEvent latch = new CountdownEvent(2);
            var cpu1 = new Thread(x => L3Latency.RunCpu1(latch));
            cpu1.Start();
            var cpu2 = new Thread(x => L3Latency.RunCpu2(latch));
            cpu2.Start();
            var stopWatch = new Stopwatch();
            latch.Wait();

            stopWatch.Start();
            cpu2.Join();

            stopWatch.Stop();
            var durationInNanos = stopWatch.ElapsedMilliseconds * 1000 * 1000;

            Console.WriteLine("{0} duration in nano seconds", durationInNanos);
            Console.WriteLine("{0} ns/op\n", durationInNanos / (L3Latency.ITERATIONS * 2L));
            Console.WriteLine("{0} ops/s\n", (L3Latency.ITERATIONS * 2L * 1000000000L) / durationInNanos);
            

        }
    }

    public static class L3Latency
    {
        private static volatile int cpu1Value = -1;
        private static volatile int cpu2Value = -1;
        public const long ITERATIONS = 100 * 1000 * 1000;

        public static void RunCpu1(CountdownEvent latch)
        {

            latch.Signal();

            long value = cpu1Value;
            while (value < ITERATIONS)
            {
                while (cpu2Value != value)
                {
                    // busy spin
                }
                
                value = ++cpu1Value;
            }
        }

        public static void RunCpu2(CountdownEvent latch)
        {
            latch.Signal();

            long value = cpu2Value;
            while (value < ITERATIONS)
            {
                while (value == cpu1Value)
                {
                    // busy spin
                }
                value = ++cpu2Value;
            }
        }
    }
}