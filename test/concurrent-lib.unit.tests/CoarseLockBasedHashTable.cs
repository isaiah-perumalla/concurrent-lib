using System;
using System.Collections.Generic;
using System.Linq;

namespace concurrent_lib.unit.tests
{
    public class LockBasedLongHashTable<T> where T : class
    {
        private readonly long[] _keys;
        private readonly object[] _values;
        private readonly uint _tableLength;
        private static readonly object EMPTY = new object();
        private int size;
        private readonly object lockobj = new object();
        private const long NO_KEY = 0;

        public LockBasedLongHashTable(uint capacity)
        {
            _tableLength = PowerOf2ClosestTo(capacity * 2);
            _keys = new long[_tableLength];
            _values = new object[_tableLength];
        }



        private static uint PowerOf2ClosestTo(uint num)
        {
            var n = num > 0 ? num - 1 : 0;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            n++;
            return num;
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
            lock (lockobj)
            {
                var index = ClaimSlotFor(key);
                var previousVal = _values[index];
                _values[index] = val1;
                if (previousVal == EMPTY || previousVal == null)
                {
                    size++;
                    return null;
                }
                return (T)previousVal;
            }
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

                index = Next(index);
            }
            return index;
        }

        public T PutIfAbsent(long key, Func<T> factoryFunction)
        {
            lock (lockobj)
            {


                var index = ClaimSlotFor(key);
                var previousVal = _values[index];
                if (previousVal == EMPTY || previousVal == null)
                {
                    _values[index] = factoryFunction();
                    size++;
                    return null;
                }
                return (T)previousVal;
            }
        }

        public T PutIfAbsent(long key, T val)
        {
            lock (lockobj)
            {


                var index = ClaimSlotFor(key);
                var previousVal = _values[index];
                if (previousVal == EMPTY || previousVal == null)
                {
                    _values[index] = val;
                    size++;
                    return (T)null;
                }
                return (T)previousVal;
            }
        }

        public T Remove(long key)
        {
            lock (lockobj)
            {


                var index = IndexOf(key);
                while (true)
                {
                    if (_keys[index] == NO_KEY) return null;
                    if (_keys[index] == key)
                    {
                        var v = _values[index];
                        if (v == EMPTY) return null;
                        var currentVal = (T)v;
                        _values[index] = EMPTY;
                        size--;
                        return currentVal;
                    }
                    index = Next(index);
                }
            }
        }

        public bool TryGetValue(long key, out T val)
        {
            lock (lockobj)
            {


                val = null;
                var index = IndexOf(key);
                while (true)
                {
                    if (_keys[index] == NO_KEY) return false;
                    var value = _values[index];
                    if (_keys[index] == key && value != EMPTY && value != null)
                    {
                        val = (T)value;
                        return true;
                    }
                    index = Next(index);

                }
            }
        }

        private uint Next(uint index)
        {
            return (index + 1) & (_tableLength - 1);
        }

        private uint IndexOf(long key)
        {
            return (uint)key & (_tableLength - 1);
        }


        public IEnumerable<long> KeySet()
        {
            T unused;
            return _keys.Where(k => TryGetValue(k, out unused)).ToArray();
        }
    }
}