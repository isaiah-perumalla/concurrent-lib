using System.Threading;
using NUnit.Framework;
using concurrent_collections;

namespace concurrent_lib.unit.tests.utils
{
    public class HashMapBlitzer
    {
        private readonly int _numOfThreads;
        private readonly int _count;
        private readonly NonBlockingLongHashTable<string> _map;
        private Barrier _barrier;
        private volatile AssertionException error;

        public HashMapBlitzer(int numOfThreads, int count, NonBlockingLongHashTable<string> map)
        {
            _barrier = new Barrier(numOfThreads);
            _numOfThreads = numOfThreads;
            _count = count;
            _map = map;
        }

        public void Blitz()
        {
            var threads = new Thread[_numOfThreads];
            for (var i = 0; i < _numOfThreads; i++)
            {
                int startKey;
                if((i % 2) == 0)
                 startKey = (_count *i);
                else
                {
                    startKey = (_count * i)+1;
                }
                threads[i] = new Thread(x => AddRemove(startKey));
                threads[i].Start();
                if(i == _numOfThreads-1) AddRemove(startKey);
            }
            Assert.That(error, Is.Null, "error detected");
        }

        private void AddRemove(int startKey)
        {
            try
            {
                AddRemoveKeyValues(startKey, _count/_numOfThreads);
            }
            catch (AssertionException e)
            {
                error = e;
                throw;
            }
        }

        private void AddRemoveKeyValues(int startKey, int count)
        {
            var val = Thread.CurrentThread.ManagedThreadId.ToString();
            _barrier.SignalAndWait();
            for (var i = 0; i < count; i++)
            {
                var key = startKey + (i*2);
              
                    Assert.That(_map.PutIfAbsent(key, val), Is.Null,
                                "there should not be any previous value for key {0}", key);
               
            }

            for (var i = 0; i < count; i++)
            {
                var key = startKey + (i*2);
                Assert.That(_map.Remove(key), Is.Not.Null, "error detected when remove, there should have been value for key {0} ", key);
            }
        }

    
    }
}