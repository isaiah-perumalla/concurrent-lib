using System;
using System.Threading;
using NUnit.Framework;
using concurrent_collections;
using concurrent_lib.unit.tests.utils;

namespace concurrent_lib.unit.tests
{
    [TestFixture]
    public class NonBlockingLongHashTableTests
    {
        private NonBlockingLongHashTable<string> _table;

        [SetUp]
        public void BeforeTests()
        {
            const int maxSize = 100;
            _table = new NonBlockingLongHashTable<string>(maxSize);
        }

        [Test]
        public void AddRetrieveAndRemoveFromTable()
        {
            Assert.That(_table.Count, Is.EqualTo(0));
            Assert.That(_table.Put(1000L, "val1"), Is.Null);
            Assert.That(_table.Count, Is.EqualTo(1));

            string val;
            Assert.That(_table.TryGetValue(1000L, out val));
            Assert.That(val, Is.EqualTo("val1"));
            
            Assert.That(_table.Remove(1000L), Is.EqualTo("val1"));
            Assert.That(_table.Remove(1000L), Is.Null);
            Assert.That(_table.TryGetValue(1000L, out val), Is.False);
            Assert.That(_table.Count, Is.EqualTo(0));
            Assert.That(_table.Put(1000L, "val1"), Is.Null);
            Assert.That(_table.Count, Is.EqualTo(1));
        }

        [Test]
        public void AddToTableIfKeyAbsent()
        {
            _table.Put(10000L, "val1");
            Assert.That(_table.PutIfAbsent(10000L, DontExecute), Is.EqualTo("val1"));
            Assert.That(_table.Count, Is.EqualTo(1));
            Assert.That(_table.PutIfAbsent(10001L, () => "val2"), Is.Null);
            Assert.That(_table.Count, Is.EqualTo(2));

        }

        [Test]
        public void BasicConcurrencyAddRemoveConcurrenlty()
        {
            //In 4 threads concurrenlty add remove even odd numbered keys
            const int numOfThreads = 10;
            const int count = 1024;
            var map = new NonBlockingLongHashTable<string>(count);
            var hashmapBlitzer = new HashMapBlitzer(numOfThreads, count, map);
            hashmapBlitzer.Blitz();

            Assert.That(map.KeySet(), Is.EqualTo(new long[0]), "all keys should have been deleted");
        }

      
        private static string DontExecute()
        {
            throw new NotImplementedException();
        }
    }
}