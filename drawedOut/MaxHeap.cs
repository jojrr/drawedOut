using System.Text.Json.Serialization;
using System.Numerics;
namespace drawedOut
{
    internal class MaxHeap<T> where T : INumber<T>
    {
        private T[] _array;

        [JsonIgnore]
        /// <summary> Length of FullArray </summary>
        public int Length { get; private set; }
        public T[] FullArray 
        { 
            get => _array;
            set
            {
                _array = (T[])value.Clone();
                Length = _array.Length;
                sort();
            }
        }

        public MaxHeap()
        {
            _array = new T[0];
            Length = 0;
        }
        public MaxHeap(T[] array)
        { FullArray = array; }

        public void Add(T value)
        {
            if (Length == _array.Length) Array.Resize(ref _array, Length+1);
            _array[Length++] = value;
            sort();
        }

        private void sort()
        {
            if (Length < 2) return;

            for (int i = Length / 2 - 1; i >= 0; i--)
            { upHeap(i, Length); }

            for (int i = Length-1; i > 0; i--)
            { 
                swap(0, i, ref _array);
                upHeap(0, i);
            }
        }

        private void upHeap(int index, int lastVal)
        {
            int max = index;
            int left = index*2 + 1;
            int right = index*2 + 2;

            if (left < lastVal && _array[left] > _array[max])
                max = left;
            if (right < lastVal && _array[right] > _array[max])
                max = right;

            if (max != index) 
            {
                swap(max, index, ref _array);
                upHeap(max, lastVal);
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
