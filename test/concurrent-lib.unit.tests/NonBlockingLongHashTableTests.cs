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
            const int count = 2048;
            var map = new NonBlockingLongHashTable<string>(count);
            var hashmapBlitzer = new HashMapBlitzer(numOfThreads, count, map);
            hashmapBlitzer.Blitz();

            Assert.That(map.KeySet(), Is.EqualTo(new long[0]), "all keys should have been deleted");
        }

     
    [Test]
    public void LargeInteration() {
     const uint capacity = 10000;
        var map = new NonBlockingLongHashTable<string>(capacity);
        Assert.That( map.Count, Is.EqualTo(0) );
    for( int i=1; i<capacity+1; i++ )
           map.Put(i,"v"+i);
    Assert.That( map.Count, Is.EqualTo(capacity) );

    int sz =0;
    long sum = 0;
    foreach( var key in map.KeySet() ) {
      sz++;
      sum +=key;
      Assert.That(key> 0 && key<=(capacity));
    }

    Assert.That(sum,Is.EqualTo(capacity*(capacity+1)/2), "Found all integers in list");

    Assert.That( map.Remove(3), Is.EqualTo("v3") ,"can remove 3" );
    Assert.That(  map.Remove(4), Is.EqualTo("v4"), "can remove 4" );
   /* sz =0;
    sum = 0;
    for( long x : _nbhml.keySet() ) {
      sz++;
      sum += x;
      assertTrue(x>=0 && x<=(capacity-1));
      String v = _nbhml.get(x);
      assertThat("",v.charAt(0),is('v'));
      assertThat("",x,is(Long.parseLong(v.substring(1))));
    }
    assertThat("Found "+(capacity-2)+" ints",sz,is(capacity-2));
    assertThat("Found all integers in list",sum,is(capacity*(capacity-1)/2 - (3+4)));*/
  }
        private static string DontExecute()
        {
            throw new NotImplementedException();
        }
    }
}