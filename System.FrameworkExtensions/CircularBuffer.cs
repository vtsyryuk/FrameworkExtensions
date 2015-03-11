using System;
using System.Threading;

namespace System.Collections.Generic
{
    public class CircularBuffer<T>
    {
        private readonly T[] _buffer = null;

        private int _head = 0;
        private int _length = 0;

        public CircularBuffer(int size)
        {
            if (size <= 0)
                throw new Exception("size must always be larger than 0");

            _buffer = new T[size];
        }

        public void Enqueue(T o)
        {
            lock (this)
            {
                var bufferLength = _buffer.Length;
                var tail = (_head + _length)%bufferLength;
                _buffer[tail] = o;

                if (_length == bufferLength)
                    _head = (_head + 1)%bufferLength;
                else
                    ++_length;
            }
        }

        public bool Dequeue(ref T o)
        {
            lock (this)
            {
                if (_length == 0)
                {
                    return false;
                }

                o = _buffer[_head];
                _head = (_head + 1)%_buffer.Length;
                --_length;
                return true;
            }
        }

        public List<T> ToList()
        {
            lock (this)
            {
                var bufferLength = _buffer.Length;
                var list = new List<T>();
                for (int i = 0, index = _head; i < _length; ++i)
                {
                    list.Add(_buffer[index]);
                    index = (index + 1)%bufferLength;
                }
                return list;
            }
        }
    }

    //lockless queue, but should only be used when there are at most 1 producer and 1 comsumer thread.
    //one classic scenario is doing async-io with dispatcher. If you are not too sure of this queue, please
    //use circularbuffer instead, as it is designed to be general purpose.
    public class SpscQueue<T>
    {
        private int _missing = 0;

        private volatile int _head = 0;
        private volatile int _tail = 0;

        private readonly T[] _buffer = null;

        public SpscQueue(int length)
        {
            bool topower2 = (length & (length - 1)) == 0;
            if (length == 0 || !topower2)
                throw new Exception("length must be set at least greater than 0 and power of 2");

            _buffer = new T[length];
        }

        private int Incr(int v)
        {
            return (v + 1) & (~(2*_buffer.Length));
        }

        private int WrapIndex(int i)
        {
            return i & ~_buffer.Length;
        }

        public bool IsFull()
        {
            return _tail == (_head ^ _buffer.Length);
        }

        public bool IsEmpty()
        {
            return _head == _tail;
        }

        public bool Enqueue(T o)
        {
            if (IsFull())
            {
                _head = Incr(_head);
                Interlocked.Increment(ref _missing);
            }
            _buffer[WrapIndex(_tail)] = o;
            _tail = Incr(_tail);
            return true;
        }

        public bool Dequeue(ref T o)
        {
            if (IsEmpty())
                return false;

            o = _buffer[WrapIndex(_head)];
            _head = Incr(_head);
            return true;
        }
    }
}