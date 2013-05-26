using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry.ComponentModel
{
    public class Bag<T> : IEnumerable<T>
    {
        private T[] _elements;

        public Bag(int capacity = 16)
        {
            this._elements = new T[capacity];
            this.Count = 0;
        }
        public int Capacity
        {
            get
            {
                return this._elements.Length;
            }
        }
        public bool IsEmpty
        {
            get
            {
                return this.Count == 0;
            }
        }
        public int Count { get; private set; }
        public T this[int index]
        {
            get
            {
                return this._elements[index];
            }

            set
            {
                if (index >= this._elements.Length)
                {
                    this.Grow(index * 2);
                    this.Count = index + 1;
                }
                else if (index >= this.Count)
                {
                    this.Count = index + 1;
                }

                this._elements[index] = value;
            }
        }
        public void Add(T element)
        {
            if (this.Count == this._elements.Length)
                this.Grow();

            this._elements[this.Count] = element;
            ++this.Count;
        }
        public void AddRange(Bag<T> rangeOfElements)
        {
            for (int index = 0, j = rangeOfElements.Count; j > index; ++index)
            {
                this.Add(rangeOfElements.Get(index));
            }
        }
        public void Clear()
        {
            for (int index = this.Count - 1; index >= 0; --index)
            {
                this._elements[index] = default(T);
            }

            this.Count = 0;
        }
        public bool Contains(T element)
        {
            for (int index = this.Count - 1; index >= 0; --index)
            {
                if (element.Equals(this._elements[index]))
                {
                    return true;
                }
            }

            return false;
        }
        public T Get(int index)
        {
            return this._elements[index];
        }
        public bool TryGetValue(int index, out T element)
        {
            if (index < Count && _elements[index] != null)
            {
                element = _elements[index];
                return true;
            }
            element = default(T);
            return false;
        }

        public T Remove(int index)
        {
            T result = this._elements[index];
            --this.Count;

            this._elements[index] = this._elements[this.Count];

            this._elements[this.Count] = default(T);
            return result;
        }
        public bool Remove(T element)
        {
            for (int index = this.Count - 1; index >= 0; --index)
            {
                if (element.Equals(this._elements[index]))
                {
                    --this.Count;

                    this._elements[index] = this._elements[this.Count];
                    this._elements[this.Count] = default(T);

                    return true;
                }
            }

            return false;
        }
        public bool RemoveAll(Bag<T> bag)
        {
            bool isResult = false;
            for (int index = bag.Count - 1; index >= 0; --index)
            {
                if (this.Remove(bag.Get(index)))
                {
                    isResult = true;
                }
            }

            return isResult;
        }
        public T RemoveLast()
        {
            if (this.Count > 0)
            {
                --this.Count;
                T result = this._elements[this.Count];

                this._elements[this.Count] = default(T);
                return result;
            }

            return default(T);
        }
        public void Set(int index, T element)
        {
            if (index >= this._elements.Length)
            {
                this.Grow(index * 2);
                this.Count = index + 1;
            }
            else if (index >= this.Count)
            {
                this.Count = index + 1;
            }

            this._elements[index] = element;
        }
        private void Grow()
        {
            this.Grow((int)(this._elements.Length * 1.5) + 1);
        }
        private void Grow(int newCapacity)
        {
            T[] oldElements = this._elements;
            this._elements = new T[newCapacity];
            Array.Copy(oldElements, 0, this._elements, 0, oldElements.Length);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                if (_elements[i] != null)
                    yield return _elements[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                if (_elements[i] != null)
                    yield return _elements[i];
            }
        }
    }
}
