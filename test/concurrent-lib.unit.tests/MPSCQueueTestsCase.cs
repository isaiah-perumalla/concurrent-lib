using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using concurrent_collections;

namespace concurrent_lib.unit.tests
{
    public abstract class MPSCQueueTestsCase
    {
        protected  int numberOfProducers =3;
        protected int repetitions = 20*1000*1000;
        protected IProducerConsumerQueue<int> queue = new MultiProducerSingleConsumerQueue<int>(1024*8);

        [TestFixtureSetUp]
        public void BeforeTestFixture()
        {
            SetUpFixture();
        }

        protected abstract void SetUpFixture();

        protected void Run(int runNum)
        {
            StartProducers();

            var producerCounts = new int[numberOfProducers];


            var stopWatch = new Stopwatch();
            stopWatch.Start();

            int id;

            for (var i = 0; i < numberOfProducers * repetitions; i++)
            {
                while (!queue.TryTake(out id))
                {
                    /*busy spin */
                }
                var producerNum = id - 1;
                ++producerCounts[producerNum];
            }

            stopWatch.Stop();

            double durationMs = stopWatch.Elapsed.TotalMilliseconds;
            var opsPerSec = Convert.ToInt64(repetitions * numberOfProducers * 1000L / durationMs);
            Console.WriteLine("Run {0} - {1} producers: duration {2}(ms) , {3} ops/sec\n", runNum, numberOfProducers, durationMs, opsPerSec.ToString("N"));

            foreach (var producerCount in producerCounts)
            {
                Assert.AreEqual(repetitions, producerCount);
            }
        }

        protected void BatchConsume(int runNum)
        {
            StartProducers();

            var producerCounts = new int[numberOfProducers];

            
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            IBatchConsumer<int> consumer = new TestBatchConsumer(producerCounts);
            queue.BatchTake(repetitions * numberOfProducers, consumer);

         
            stopWatch.Stop();

            double durationMs = stopWatch.Elapsed.TotalMilliseconds;
            var opsPerSec = Convert.ToInt64(repetitions*numberOfProducers*1000L/durationMs);
            Console.WriteLine("Run {0} - {1} producers: duration {2}(ms) , {3} ops/sec\n", runNum, numberOfProducers, durationMs, opsPerSec.ToString("N"));

            foreach (var producerCount in producerCounts)
            {
                Assert.AreEqual(repetitions, producerCount);
            }
        }

        private void StartProducers()
        {
            var barrier = new Barrier(numberOfProducers);
            for (int i = 0; i < numberOfProducers; i++)
            {
                var id = i + 1;
                new Thread(x => RunProducer(queue, id, barrier)).Start();
            }
        }

        private  void RunProducer(IProducerConsumerQueue<int> producerConsumerCollection, int id, Barrier barrier)
        {
            barrier.SignalAndWait();
            for (var i = 0; i < repetitions; i++)
            {
                while (!producerConsumerCollection.TryAdd(id))
                {
                  
                    Thread.Yield();
                }
            }
        }
    }
}