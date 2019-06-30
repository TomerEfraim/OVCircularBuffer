using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OVCircularBuffer
{
    public class OVCircularBuffer<T>
    {
        protected readonly T[] _buffer;
        protected int _head;
        protected int _tail;
        protected readonly object _lock = new object();

        public OVCircularBuffer(int bufferSize)
        {
            if (bufferSize < 1)
            {
                throw new ArgumentOutOfRangeException("CircularBuffer - constructor exception: buffer size should be > 0");
            }
            _buffer = new T[bufferSize];
            BufferSize = bufferSize;
            _head = bufferSize - 1;
            _tail = 0;
            Count = 0;
        }

        public int Count { get; protected set; }

        public int BufferSize { get; protected set; }

        public bool IsEmpty
        {
            get
            {
                return Count == 0;
            }
        }

        public bool IsFull
        {
            get
            {
                return Count == BufferSize;
            }
        }

        protected virtual int NextPosition(int currPosition)
        {
            return (currPosition + 1) % BufferSize;
        }

        public virtual bool TryDequeue(out T dequeued)
        {
            lock (_lock)
            {
                try
                {
                    if (IsEmpty)
                    {
                        dequeued = default;
                        return false;
                    }
                    dequeued = _buffer[_tail];
                    _tail = NextPosition(_tail);
                    Count--;
                    return true;
                }
                catch (Exception)
                {
                    dequeued = default;
                    return false;
                }
            }
        }

        public virtual void Enqueue(T toAdd)
        {
            lock (_lock)
            {
                _head = NextPosition(_head);
                _buffer[_head] = toAdd;
                if (IsFull)
                {
                    _tail = NextPosition(_tail);
                }
                else
                {
                    Count++;
                }
            }
        }

        public virtual bool TryPeek(out T peeked)
        {
            lock (_lock)
            {
                try
                {
                    if (IsEmpty)
                    {
                        peeked = default;
                        return false;
                    }
                    peeked = _buffer[_tail];
                    return true;
                }
                catch (Exception)
                {
                    peeked = default;
                    return false;
                }
            }
        }

        public virtual T[] ToArray()
        {
            lock (_lock)
            {
                T[] retArray = new T[Count];
                int currPos = _tail;
                for (int i = 0; i < Count; i++)
                {
                    retArray[i] = _buffer[currPos];
                    currPos = NextPosition(currPos);
                }
                return retArray;
            }
        }

        public virtual void Clear()
        {
            lock (_lock)
            {
                for (int i = 0; i < BufferSize; i++)
                {
                    _buffer[i] = default;
                }
                _head = BufferSize - 1;
                _tail = 0;
                Count = 0;
            }
        }
    }
}
