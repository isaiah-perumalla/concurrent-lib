using System;

namespace concurrent_collections
{
    public class NonBlockingLongHashTable<T> where T: class 
    {
        private readonly long[] _keys;
        private readonly object[] _values;
        private readonly int _tableCapacity;
        private static readonly object EMPTY = new object();
        private int size;
        private const long NO_KEY = 0;

        public NonBlockingLongHashTable(int capacity)
        {
            _tableCapacity = PrimeClosestTo(capacity);
            _keys = new long[_tableCapacity];
             _values = new object[_tableCapacity];
        }

      

        private static int PrimeClosestTo(int capacity)
        {
            return capacity;
        }


        public int Count
        {
            get { return size; }
        }

        /// <summary>
        /// put value val1 with key
        /// return previous value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val1"></param>
        /// <returns></returns>
        public T Put(long key, T val1)
        {
            var index = ClaimSlotFor(key);
            var previousVal = _values[index];
            _values[index] = val1;
            if( previousVal == EMPTY || previousVal == null)
            {
                size++;
                return  null;             
            }
            return  (T)previousVal;    

        }

        private long ClaimSlotFor(long key)
        {
            var index = IndexOf(key);
            while (true)
            {
                if (_keys[index] == NO_KEY)
                {
                    _keys[index] = key;
                    break;
                }
                if (_keys[index] == key) break;

                index++;
            }
            return index;
        }

        public T PutIfAbsent(long key, Func<T> factoryFunction)
        {
            var index = ClaimSlotFor(key);
            var previousVal = _values[index];
            if (previousVal == EMPTY || previousVal == null)
            {
                _values[index] = factoryFunction();
                size++;
                return (T)_values[index];
            }
            return (T)previousVal;
        }
        public T Remove(long key)
        {
            var index = IndexOf(key);
            while (true)
            {
                if (_keys[index] == NO_KEY) return null;
                if (_keys[index] == key)
                {
                    var currentVal = (T)_values[index];
                    _values[index] = EMPTY;
                    size--;
                    return currentVal;
                }
                index++;
            }

        }

        public bool TryGetValue(long key, out T val)
        {
            val = null;
            var index = IndexOf(key);
            while(true)
            {
                if (_keys[index] == NO_KEY) return false;
                if(_keys[index] == key)
                {
                    val = (T)_values[index];
                    return true;
                }
                index++;

            }
        }

        private long IndexOf(long key)
        {
            var index = key%(_tableCapacity-1);
            return index;
        }


    }
}