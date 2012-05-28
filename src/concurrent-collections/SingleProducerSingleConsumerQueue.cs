using System;
using System.Diagnostics;
using System.Threading;
using concurrent_collections.conurrent.utils;

namespace concurrent_collections
{
    public class SingleProducerSingleConsumerQueue<T>
    {

         private AtomicArray<T> items;
        private readonly uint mask;
        private readonly uint capacity;
        private Sequence tailSequence = new Sequence(0);
        private Sequence headSequence = new Sequence(0);
        private static readonly T SENTINEL_ITEM = default(T);

        public SingleProducerSingleConsumerQueue(uint minSize)
        {
            this.capacity = minSize.NextPowerOfTwo();
            mask = capacity - 1;
            Debug.Assert((mask + 1).IsPowerOf2()); 
            items = new AtomicArray<T>(this.capacity);
        }

       
    public bool TryAdd(T item)
    {
        if(Equals(item, SENTINEL_ITEM)) throw new ArgumentException(string.Format("cannot insert default valued item {0}", item));
        var head = headSequence.Get();
        var tail = tailSequence.Get();
        if(tail-head >= capacity) return false;


        var index = (uint) (tail & mask);
        tailSequence.LazySet(tail+1);
        items.LazyPutAt(index, item);

        return true;
    }

    public bool TryTake(out T item)
    {
        item = SENTINEL_ITEM;
        var head = headSequence.Get();
        long tail = tailSequence.Get();
        
        if (head == tail) return false;
        var index = (uint) (head & mask);
        item = items.GetAt(index);
        if (Equals(item, default(T))) return false;
        items.LazyPutAt(index, SENTINEL_ITEM);
        headSequence.LazySet(head + 1);
        return true;
    }
    
    }
}
