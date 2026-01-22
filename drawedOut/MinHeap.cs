using System.Numerics;
namespace drawedOut
{
    internal class MinHeap<T> where T : INumber<T>
    {
        private T _min;
        private T[] _array;
        private int _lastVal;

        public int Length => _lastVal;
        public T Min => _min;
        public T[] FullArray 
        { 
            get => _array;
            set
            {
                _array = (T[])(value.Clone());
                _lastVal = _array.Length;
                _min = sort();
            }
        }

        public MinHeap()
        {
            _array = new T[0];
            _lastVal = 0;
        }
        public MinHeap(int size)
        { 
            _array = new T[size];
            _lastVal = 0;
        }
        public MinHeap(T[] array)
        { FullArray = array; }

        public void Add(T value)
        {
            if (_lastVal == _array.Length) Array.Resize(ref _array, _lastVal+1);
            _array[_lastVal++] = value;
            _min = sort();
        }

        private T sort()
        {
            T[] _sortArray = (T[])_array.Clone();
            if (_lastVal < 2) return _sortArray[0];
            for (int i = _lastVal / 2 - 1; i >= 0; i--)
            { upHeap(i, _lastVal, ref _sortArray); }
            return _sortArray[0];
        }

        private void upHeap(int index, int lastVal, ref T[] sortArray)
        {
            int min = index;
            int left = index*2 + 1;
            int right = index*2 + 2;
            if (left < lastVal && sortArray[left] < sortArray[min])
                min = left;
            if (right < lastVal && sortArray[right] < sortArray[min])
                min = right;

            if (min != index) 
            {
                swap(min, index, ref sortArray);
                upHeap(index, lastVal, ref sortArray);
            }
        }


        private void swap(int i, int j, ref T[] sortArray)
        {
            T temp = sortArray[i];
            sortArray[i] = sortArray[j];
            sortArray[j] = temp;
        }

    }
}
