using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace concurrent_collections
{
    /// <summary>
    /// high performance, Non-blocking concurrent hash table where keys are 64bit unsigned integers
    /// Open addressing scheme using linear probing, makes it cpu-cache friendly
    /// non resizeable must provide size at creation
    /// Based on ideas from Cliff click's high scale lib
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NonBlockingLongHashTable<T> where T : class
    {
        private readonly long[] _keys;
        private readonly object[] _values;
        private readonly uint _tableLength;
        private static readonly object EMPTY = new object();
        private AtomicInt slots = new AtomicInt();
        private static readonly object NO_MATCH_OLD = new object();
        private static readonly object MATCH_ANY = new object();
        private AtomicInt _size = new AtomicInt();
        private const long NO_KEY = 0;

        public NonBlockingLongHashTable(uint capacity)
        {
            _tableLength = PowerOf2ClosestTo(capacity*2);
            Debug.Assert(IsPowerOf2(_tableLength));
            _keys = new long[_tableLength];
            _values = new object[_tableLength];
        }

        private bool IsPowerOf2(uint x)
        {
            return (x & (x - 1)) == 0;
        }


        private static uint PowerOf2ClosestTo(uint num)
        {
            var n = num > 0 ? num - 1 : 0;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            return ++n;
        }


        public int Count
        {
            get { return _size.Value; }
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
            return PutIfMatch(key, val1, NO_MATCH_OLD);
        }

        private long ClaimSlotFor(long key, out object previousVal)
        {
            var index = IndexOf(key);
            while (true)
            {
                previousVal = _values[index];
                if (_keys[index] == NO_KEY)
                {
                    if (CAS_key(index, NO_KEY, key))
                    {
                        slots.Increment();
                        break;
                    }
                }
                if (_keys[index] == key) break;

                index = Next(index);
            }
            return index;
        }

        private bool CAS_key(uint index, long compare, long val)
        {
            return Interlocked.CompareExchange(ref _keys[index], val, compare) == compare;
        }

        public T PutIfAbsent(long key, Func<T> factoryFunction)
        {
            object previousVal;
            ClaimSlotFor(key, out previousVal);
            if (previousVal == EMPTY || previousVal == null)
            {
                return PutIfMatch(key, factoryFunction(), EMPTY);
            }
            return  previousVal as T;
       }


        private T PutIfMatch(long key, object putVal, object expectedVal)
        {
            if (putVal == null) throw new ArgumentException("null values cannot be inserted");
            if (key <= 0) throw new ArgumentException("invalid key, key have to be greater than zero");

            if (_keys[IndexOf(key)] == NO_KEY && putVal == EMPTY) return putVal as T;
            object previousVal;
            var idx = ClaimSlotFor(key, out previousVal);
            while (true)
            {
                if (CannotMatchExpectedVal(expectedVal, previousVal)) return previousVal as T;
                var casValue = CAS_Value(idx, previousVal, putVal);
                if(casValue == previousVal)
                {
                    if ((previousVal == null || previousVal == EMPTY) && putVal != EMPTY) _size.Increment();
                    if (!(previousVal == null || previousVal == EMPTY) && putVal == EMPTY) _size.Decrement();
                    return previousVal as T;
                }   
                //cas failed get previous value, and try again if required
                previousVal = _values[idx];
            }


        }

        private object CAS_Value(long idx, object previousVal, object putVal)
        {
            return Interlocked.CompareExchange(ref _values[idx], putVal, previousVal);
        }

        private static bool CannotMatchExpectedVal(object expectedVal, object previousVal)
        {
            return expectedVal != NO_MATCH_OLD && // Do we care about expected-Value at all?
                   previousVal != expectedVal && // No instant match already?
                   (expectedVal != MATCH_ANY || previousVal == EMPTY || previousVal == null) &&
                   previousVal != expectedVal &&
                   !(previousVal == null && expectedVal == EMPTY);
        }

        public T PutIfAbsent(long key, T val)
        {
            return PutIfMatch(key, val, EMPTY);
        }

        public T Remove(long key)
        {
            return PutIfMatch(key, EMPTY, NO_MATCH_OLD);

        }

        public bool TryGetValue(long key, out T val)
        {
            val = null;
            var index = IndexOf(key);
            while (true)
            {
                if (_keys[index] == NO_KEY) return false;
                var value = _values[index];
                if (_keys[index] == key && value != EMPTY && value != null)
                {
                    val = (T) value;
                    return true;
                }
                index = Next(index);
            }
        }

        private uint Next(uint index)
        {
            return (index + 1) & (_tableLength - 1);
        }

        private uint IndexOf(long key)
        {
            return (uint) key & (_tableLength - 1);
            
        }


        public IEnumerable<long> KeySet()
        {
            T unused;
            return _keys.Where(k => TryGetValue(k, out unused)).ToArray();
        }
    }
}
