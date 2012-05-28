using System;
using System.Diagnostics;
using System.Threading;
using concurrent_collections.conurrent.utils;

namespace concurrent_collections
{
    public interface IProducerConsumerQueue<T>
    {
        bool TryAdd(T item);
        bool TryTake(out T item);
        void BatchTake(int batchSize, IBatchConsumer<T> consumer);
    }

    public interface IBatchConsumer<T>
    {
        void NotifyItemTaken(T item, bool endOfBatch);
    }

    public class MultiProducerSingleConsumerQueue<T> : IProducerConsumerQueue<T>
    {
        private AtomicArray<T> items;
        private readonly uint mask;
        private readonly uint capacity;
        private Sequence tailSequence = new Sequence(0);
        private Sequence headSequence = new Sequence(0);
        private static readonly T SENTINEL_ITEM = default(T);

        public MultiProducerSingleConsumerQueue(uint minSize)
        {
            this.capacity = minSize.NextPowerOfTwo();
            mask = capacity - 1;
            Debug.Assert((mask + 1).IsPowerOf2()); 
            items = new AtomicArray<T>(this.capacity);
        }

       
    public bool TryAdd(T item)
    {
        if(Equals(item, SENTINEL_ITEM)) throw new ArgumentException(string.Format("cannot insert default valued item {0}", item));
        long slot;
        if (!TryClaimSlot(out slot)) return false;

        var index = (uint) (slot & mask);

        items.LazyPutAt(index, item);

        return true;
    }
    public bool TryTake(out T item)
    {
        item = SENTINEL_ITEM;
        Thread.MemoryBarrier();
        var head = headSequence.Get();
        var tail = tailSequence.Get();
        
        if (head == tail) return false;
    
        item = ConsumeValueAt(head);

        headSequence.LazySet(head + 1);
        return true;
    }

        private T ConsumeValueAt(long seq)
        {
            T item;
            uint index = (uint)seq & mask;
            do
            {
                item = items.VolatileGetElementAt(index);
            } while (Equals(item, SENTINEL_ITEM));
            items.LazyPutAt(index, SENTINEL_ITEM);

            return item;
        }

        public void BatchTake(int batchSize, IBatchConsumer<T> consumer)
        {
           
        long nextSequence = headSequence.Get();
        long maxSequence = nextSequence + batchSize;
        while (nextSequence < maxSequence)
        {
            long currentTail;
            while (nextSequence == (currentTail = tailSequence.VolatileGet()))
            {
                Thread.Yield();
            }

            do
            {
                var item = ConsumeValueAt(nextSequence);
                consumer.NotifyItemTaken(item, nextSequence == currentTail - 1);
            } while (++nextSequence < currentTail);
            headSequence.LazySet(nextSequence);
        }
      }


        private bool TryClaimSlot(out long slot)
        {
            
            do
            {
                slot = tailSequence.Get();
                long size = slot - capacity;
                long head = headSequence.Get();
                if (size >= head)
                {
                    return false;
                }
            } while (slot != tailSequence.CompareAndSet(slot, slot + 1));
            return true;
        }
    }
}