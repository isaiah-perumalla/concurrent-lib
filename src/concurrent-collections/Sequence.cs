using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace concurrent_collections
{
    [StructLayout(LayoutKind.Explicit, Size = CacheLineSize * 2)]
    public struct Sequence
    {
        //avoid false share by ensuring this takes up an entire cache-line
       
        
        [FieldOffset(CacheLineSize)] long value;
        const int CacheLineSize=64;

        public Sequence(long initialValue)
        {
            value = initialValue;
        }

        public long CompareAndSet(long expected, long newValue)
        {
            return Interlocked.CompareExchange(ref value, newValue, expected);
        }

        public long Get()
        {
            return value;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void LazySet(long newValue)
        {
            this.value = newValue;
        }

        public void Set(long newValue)
        {
            
            this.value = newValue;
            Thread.MemoryBarrier();
            
        }

        public long VolatileGet()
        {
            Thread.MemoryBarrier();
            return value;
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        public long CompilerFenceGet()
        {
            return value;
        }
    }
}