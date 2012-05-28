using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using concurrent_collections;


namespace concurrent_lib.unit.tests{
    [TestFixture]
    public class MultiProducerSingleConsumerQueueTests
    {
        readonly IProducerConsumerQueue<int> queue = new MultiProducerSingleConsumerQueue<int>(1024*8);
        //readonly BlockingCollection<int> queue = new BlockingCollection<int>( (1024*8));
        private const int NUM_PRODUCERS = 3;
        private const int REPETITIONS = 20*1000*1000 ;


    [Test]
    public void PerfTestAddAndTake()
    {
        for (int i = 0; i < 5; i++)
        {
            Run(i);
            Thread.Sleep(3000);
        }
    }

    private void Run(int runNum)
        {
            StartProducers();

            var producerCounts = new int[NUM_PRODUCERS];


            var stopWatch = new Stopwatch();
            stopWatch.Start();

        int id;

        for (var i = 0; i < NUM_PRODUCERS * REPETITIONS; i++)
        {
            while (!queue.TryTake(out id)) { /*busy spin */ }
            var producerNum = id - 1;
            ++producerCounts[producerNum];
        }

        stopWatch.Stop();

        double durationMs = stopWatch.Elapsed.TotalMilliseconds;
        var opsPerSec = Convert.ToInt64(REPETITIONS * NUM_PRODUCERS * 1000L / durationMs);
        Console.WriteLine("Run {0} - {1} producers: duration {2}(ms) , {3} ops/sec\n", runNum, NUM_PRODUCERS, durationMs, opsPerSec.ToString("N"));

            foreach (var producerCount in producerCounts)
            {
                Assert.AreEqual(REPETITIONS, producerCount);
            }
        }

        [Test]
    public void PerfTestAddAndBatchTake()
    {
        for (int i = 0; i < 5; i++)
        {
            BatchConsume(i);
            Thread.Sleep(3000);
        }
    }



        private void BatchConsume(int runNum)
        {
            StartProducers();

            var producerCounts = new int[NUM_PRODUCERS];

            
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            IBatchConsumer<int> consumer = new TestBatchConsumer(producerCounts);
            queue.BatchTake(REPETITIONS * NUM_PRODUCERS, consumer);

         
            stopWatch.Stop();

            double durationMs = stopWatch.Elapsed.TotalMilliseconds;
            var opsPerSec = Convert.ToInt64(REPETITIONS*NUM_PRODUCERS*1000L/durationMs);
            Console.WriteLine("Run {0} - {1} producers: duration {2}(ms) , {3} ops/sec\n", runNum, NUM_PRODUCERS, durationMs, opsPerSec.ToString("N"));

            foreach (var producerCount in producerCounts)
            {
                Assert.AreEqual(REPETITIONS, producerCount);
            }
        }

        private void StartProducers()
        {
            var barrier = new Barrier(NUM_PRODUCERS);
            for (int i = 0; i < NUM_PRODUCERS; i++)
            {
                var id = i + 1;
                new Thread(x => RunProducer(queue, id, barrier)).Start();
            }
        }

        private static void RunProducer(IProducerConsumerQueue<int> producerConsumerCollection, int id, Barrier barrier)
        {
            barrier.SignalAndWait();
            for (var i = 0; i < REPETITIONS; i++)
            {
                while (!producerConsumerCollection.TryAdd(id))
                {
                  
                    Thread.Yield();
                }
            }
        }
    }

    internal class TestBatchConsumer : IBatchConsumer<int>
    {
        private readonly int[] _producerCounts;

        public TestBatchConsumer(int[] producerCounts)
        {
            _producerCounts = producerCounts;
        }

        public void NotifyItemTaken(int item, bool endOfBatch)
        {
            int producerNum = item - 1;
            ++_producerCounts[producerNum];
        }
    }
}