using System.Threading;

namespace concurrent_collections
{
    public class AtomicInt
    {
        private int count =0;

        public int Value
        {
            get { return count; }
        }

        public void Increment()
        {
            Interlocked.Increment(ref count);
        }

        public void Decrement()
        {
            Interlocked.Decrement(ref count);
        }
    }
}