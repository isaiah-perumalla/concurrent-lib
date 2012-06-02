using System.Threading;
using NUnit.Framework;
using concurrent_collections;

namespace concurrent_lib.unit.tests
{
    [TestFixture]
    public class MProducerSConsumerQueueTest : MPSCQueueTestsCase
    {
        protected override void SetUpFixture()
        {
            queue = new MProducerSConsumerQueue<int>(1024 * 8);
            repetitions = 20 * 1000 * 1000;
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
}