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
        private CountdownEvent _countdown;

        public HashMapBlitzer(int numOfThreads, int count, NonBlockingLongHashTable<string> map)
        {
            _barrier = new Barrier(numOfThreads);
            _numOfThreads = numOfThreads;
            _count = count;
            _map = map;
            _countdown = new CountdownEvent(numOfThreads);
        }

        public void Blitz()
        {
            int startKey = 0;
            for (int i = 0; i < _numOfThreads; i++)
            {
                if (i % 2 == 0)
                    startKey = (1 << (i + 1)) * _count;
                else startKey += 1;
                int trdId = i;
                int key = startKey;
                OnNewThread(x => AddRemove(key, "T"+trdId));
                
            }

            _countdown.Wait();

            Assert.That(error, Is.Null, "error detected");
        }

        private Thread OnNewThread(ParameterizedThreadStart action)
        {
            var t = new Thread(action);
            t.Start();
            return t;
        }


        private void AddRemove(int startKey, string s)
        {
            try
            {
                AddRemoveKeyValues(startKey, s, _count/_numOfThreads);
            }
            catch (AssertionException e)
            {
                error = e;
                throw;
            }
        }

        private void AddRemoveKeyValues(int startKey, string s, int count)
        {
            _barrier.SignalAndWait();
            try
            {
                for (var i = 0; i / 2 < count; i += 2)
                {
                    var key = startKey + i;

                    Assert.That(_map.PutIfAbsent(key, s), Is.Null,
                                "there should not be any previous value for key {0}", key);

                }

                for (var i = 0; i / 2 < count; i += 2)
                {
                    var key = startKey + i;
                    Assert.That(_map.Remove(key), Is.Not.Null,
                                "error detected when {0} tried to remove, there should have been value for key {1} ", s,
                                key);
                }
            }
            finally
            {
                _countdown.Signal();
            }
        }

    
    }
}