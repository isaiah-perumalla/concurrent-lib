using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using concurrent_collections.conurrent.utils;

namespace concurrent_collections
{
   
    public class MProducerSConsumerQueue<T> : IProducerConsumerQueue<T>
    {
        private QueueItem<T>[] items;
        private readonly uint mask;
        private readonly uint capacity;
        private Sequence tailSequence = new Sequence(0);
        private Sequence headSequence = new Sequence(0);
        private static readonly T SENTINEL_ITEM = default(T);

        public MProducerSConsumerQueue(uint minSize)
        {
            this.capacity = minSize.NextPowerOfTwo();
            mask = capacity - 1;
            Debug.Assert((mask + 1).IsPowerOf2()); 
            items = new QueueItem<T>[this.capacity];
            for (int i = 0; i < capacity; i++)
            {
                items[i].SetSeq(i);
            }
        }

       
    public bool TryAdd(T item)
    {
        long tail;
        do
        {
            tail = tailSequence.CompilerFenceGet();
            var qitem = items[tail&mask];
            var seq = qitem.GetSeq();
            long diff = seq - tail;
            if(diff < 0) return false;
            
            if(diff == 0)
            {
                if (tail == tailSequence.CompareAndSet(tail, tail + 1)) break;
            }
        } while (true);

        var index = (tail & mask);

        SetAt(index, item, tail);

        return true;
    }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private void SetAt(long index, T item, long tail)
        {
            items[index].SetValue(item);
            items[index].SetSeq(tail + 1);
        }

    public bool TryTake(out T item)
    {
        item = SENTINEL_ITEM;
        
        var head = headSequence.Get();

        var index = (uint)head&mask;
        do
        {
            var qitem = GetItemAt(index);
            var seq = qitem.GetSeq();
            long diff = seq - (head+1);
            if(diff ==0) break;
            if (diff < 0) return false; //queue is empty

        } while (true);
        item = ConsumeValueAt(head, index);
        headSequence.LazySet(head + 1);
        return true;
    }
        [MethodImpl(MethodImplOptions.NoOptimization)]
        private T ConsumeValueAt(long head, uint index)
        {
            T item;
            item = items[index].GetValue();
            items[index].SetSeq(head + mask + 1);
            return item;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private QueueItem<T> GetItemAt(uint index)
        {
            var qitem = items[index];
            return qitem;
        }


        public void BatchTake(int batchSize, IBatchConsumer<T> consumer)
        {
            var nextSeq = headSequence.Get();
            var maxSeq = nextSeq + batchSize;
           
                do
                {
                    uint index = (uint) nextSeq & mask;
                    var qitem = GetItemAt(index);
                    var seq = qitem.GetSeq();
                    var diff = seq - (nextSeq + 1);
                    if(diff == 0)
                    {
                        var item = ConsumeValueAt(nextSeq, index);
                        consumer.NotifyItemTaken(item, maxSeq == nextSeq+1);
                        nextSeq++;
                        headSequence.Set(nextSeq);
                    }
                    else if(diff < 0) //queue is full
                    {
                        Thread.Yield();
                    }
                } while (nextSeq < maxSeq);
        }
    }

    internal struct QueueItem<T>
    {
        private long seq;
        private T value;

        public long GetSeq()
        {
            return seq;
        }

        public void SetValue(T item)
        {
            this.value = item;
        }

        public void SetSeq(long l)
        {
            this.seq =l;
        }

        public T GetValue()
        {
            return this.value;
        }
    }
}