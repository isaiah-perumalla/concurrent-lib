using System;
using NUnit.Framework;
using concurrent_collections;

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
            Assert.That(_table.Count, Is.EqualTo(0));
            Assert.That(_table.Put(1000L, "val1"), Is.Null);
            Assert.That(_table.Count, Is.EqualTo(1));
        }

        [Test]
        public void AddToTableIfKeyAbsent()
        {
            _table.Put(10000L, "val1");
            Assert.That(_table.PutIfAbsent(10000L, dontExecute), Is.EqualTo("val1"));
            Assert.That(_table.Count, Is.EqualTo(1));
            Assert.That(_table.PutIfAbsent(10001L, () => "val2"), Is.EqualTo("val2"));
            Assert.That(_table.Count, Is.EqualTo(2));

        }

        private string dontExecute()
        {
            throw new NotImplementedException();
        }
    }
}