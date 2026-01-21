using System.Numerics;
namespace drawedOut
{
    internal class MinHeap<T> where T : INumber<T>
    {
        private T[] _array;
        private int _lastVal;

        public int Length => _lastVal;
        public T Root => _array[0]; 
        public T[] FullArray 
        { 
            get => _array;
            set
            {
                _array = (T[])(value.Clone());
                _lastVal = _array.Length;
                sort();
            }
        }

        public MinHeap()
        {
            if (_array is null)
            {
                _array = new T[0];
                _lastVal = 0;
            }
        }
        public MinHeap(int size)
        { 
            _array = new T[size];
            _lastVal = 0;
            sort();
        }
        public MinHeap(T[] array)
        { FullArray = array; }

        public void Add(T value)
        {
            if (_lastVal == _array.Length) Array.Resize(ref _array, _lastVal+1);
            _array[_lastVal++] = value;
            sort();
        }

        private void sort()
        {
            if (_lastVal < 2) return;
            for (int i = _lastVal / 2 - 1; i >= 0; i--)
            {
                upHeap(i, _lastVal);
            }
        }

        private void upHeap(int index, int _lastVal)
        {
            int min = index;
            int left = index*2 + 1;
            int right = index*2 + 2;
            if (left < _lastVal && _array[left] < _array[min])
                min = left;
            if (right < _lastVal && _array[right] < _array[min])
                min = right;

            if (min != index) 
            {
                swap(min, index);
                upHeap(index, _lastVal);
            }
        }


        private void swap(int i, int j)
        {
            T temp = _array[i];
            _array[i] = _array[j];
            _array[j] = temp;
        }

    }
}
