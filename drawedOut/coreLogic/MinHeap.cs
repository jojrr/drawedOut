using System.Text.Json.Serialization;
using System.Numerics;
namespace drawedOut
{
    internal class MinHeap<T> where T : INumber<T>
    {
        private T[] _array;
        private T[] _sortedArray;
        private int _lastVal;

        [JsonIgnore]
        public int Length => _lastVal;

        [JsonIgnore]
        public T Root => _sortedArray[0];

        [JsonIgnore]
        public T[] SortedTree => _sortedArray;

        public T[] FullArray 
        { 
            get => _array;
            set
            {
                _array = (T[])value.Clone();
                _lastVal = _array.Length;
                sort();
            }
        }

        public int Left(int index=0) => index*2+1;
        public int Right(int index=0) => index*2+2;

        public MinHeap()
        {
            _array = new T[0];
            _lastVal = 0;
        }
        public MinHeap(T[] array)
        { FullArray = array; }

        public void Add(T value)
        {
            if (_lastVal == _array.Length) Array.Resize(ref _array, _lastVal+1);
            _array[_lastVal++] = value;
            sort();
        }

        private T sort()
        {
            _sortedArray = (T[])_array.Clone();
            if (_lastVal < 2) return _sortedArray[0];
            for (int i = _lastVal / 2 - 1; i >= 0; i--)
            { upHeap(i, _lastVal); }
            return _sortedArray[0];
        }

        private void upHeap(int index, int lastVal)
        {
            int min = index;
            int left = index*2 + 1;
            int right = index*2 + 2;
            if (left < lastVal && _sortedArray[left] < _sortedArray[min])
                min = left;
            if (right < lastVal && _sortedArray[right] < _sortedArray[min])
                min = right;

            if (min != index) 
            {
                swap(min, index, ref _sortedArray);
                upHeap(index, lastVal);
            }
        }


        private void swap(int i, int j, ref T[] swapArr)
        {
            T temp = swapArr[i];
            swapArr[i] = swapArr[j];
            swapArr[j] = temp;
        }

    }
}
