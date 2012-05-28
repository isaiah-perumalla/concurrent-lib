using System.Runtime.CompilerServices;
using System.Threading;

namespace concurrent_collections
{
    internal struct AtomicArray<T>  
    {
        private readonly T[] items;

        public AtomicArray(uint capacity)
        {
            items = new T[capacity];
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void LazyPutAt(uint index, T item)
        {
            items[index] = item;
        }

        public T VolatileGetElementAt(uint index)
        {
            Thread.MemoryBarrier();
            return items[index];
        }

        public void PutAt(uint index, T sentinelItem)
        {
            items[index] = sentinelItem;
            Thread.MemoryBarrier();
        }

        public T GetAt(uint index)
        {
            return items[index];
        }
    }
}