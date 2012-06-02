using System.Collections.Concurrent;
using System.Threading;
using NUnit.Framework;
using concurrent_collections;


namespace concurrent_lib.unit.tests{
    [TestFixture]
    public class MultiProducerSingleConsumerQueueTests : MPSCQueueTestsCase
    {
        //readonly IProducerConsumerQueue<int> queue = new MProducerSConsumerQueue<int>(1024*8);
        //readonly BlockingCollection<int> queue = new BlockingCollection<int>( (1024*8));


        protected override void SetUpFixture()
        {
            queue = new MultiProducerSingleConsumerQueue<int>(1024*8);
            repetitions = 20*1000*1000;
            numberOfProducers = 3;
        }

    [Test]
    public void PerfTestAddAndTake()
    {
        for (int i = 0; i < 5; i++)
        {
            Run(i);
            Thread.Sleep(3000);
        }
    }

    [Test]
    public void PerfTestAddAndBatchTake()
    {
        for (var i = 0; i < 5; i++)
        {
            BatchConsume(i);
            Thread.Sleep(3000);
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