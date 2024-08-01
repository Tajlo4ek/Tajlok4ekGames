using System;
using System.Collections.Generic;

namespace SvoyaIgra.Utils
{
    public class MyQueue<T>
    {
        private readonly List<T> list;

        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return Count == 0;
            }
        }

        public MyQueue()
        {
            list = new List<T>();
        }

        public void Enqueue(T value)
        {
            list.Add(value);
        }

        public T Dequeue()
        {
            if (list.Count == 0)
            {
                throw new Exception("empty");
            }

            var value = list[0];
            list.RemoveAt(0);

            return value;
        }

        public T Peek()
        {
            if (list.Count == 0)
            {
                throw new Exception("empty");
            }

            return list[0];
        }

        public void Clear()
        {
            list.Clear();
        }

        public void Remove(T value)
        {
            list.Remove(value);
        }

        public void RemoveAll(Predicate<T> predicate)
        {
            list.RemoveAll(predicate);
        }

        public bool Contains(T value)
        {
            return list.Contains(value);
        }

    }
}
