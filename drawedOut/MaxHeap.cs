using System.Text.Json.Serialization;
using System.Numerics;
namespace drawedOut
{
    // max heap sorts the items in ascending order
    internal class MaxHeap<T> where T : INumber<T>
    {
        private T[] _array; 

        [JsonIgnore] // do note store the length of full array in the JSON file
        /// <summary> Length of FullArray </summary>
        public int Length { get; private set; }

        public T[] FullArray 
        { 
            get => _array;
            set
            {
                _array = (T[])value.Clone(); // must clone or reference will be used (need value)
                Length = _array.Length;
                sort();
            }
        }

        // can be created empty or can pass in an existing array
        public MaxHeap()
        {
            _array = new T[0];
            Length = 0;
        }

        public MaxHeap(T[] array)
        { FullArray = array; }

        // <summary>
        // add a value into the max heap
        // </summary>
        public void Add(T value)
        {
            if (Length == _array.Length) Array.Resize(ref _array, Length+1);
            _array[Length++] = value;
            sort();
        }

        private void sort()
        {
            // if only 1 or 0 items then skip checks
            if (Length < 2) return;

            // check from largest non-leaf nodes, going up
            for (int i = Length / 2 - 1; i >= 0; i--)
            { upHeap(i, Length); }

            // starting from the last item in the array, swap this item with the root node (index 0), then re-build the heap, ignoring the swapped root node value
            for (int i = Length-1; i > 0; i--)
            { 
                swap(0, i, ref _array);
                upHeap(0, i);
            }
        }

        private void upHeap(int index, int lastVal)
        {
            int max = index;            // assume current node is max
            int left = index*2 + 1;     // left child index
            int right = index*2 + 2;    // right child index

            // check if children are bigger than current node
            if (left < lastVal && _array[left] > _array[max])
                max = left;
            if (right < lastVal && _array[right] > _array[max])
                max = right;

            // if chidlren are bigger than current node, then swap and recursively check the child node that was swapped
            if (max != index) 
            {
                swap(max, index, ref _array);
                upHeap(max, lastVal);
            }
        }


        // <summary> swap helper function to swap two values in array </summary>
        private void swap(int i, int j, ref T[] swapArr)
        {
            T temp = swapArr[i];
            swapArr[i] = swapArr[j];
            swapArr[j] = temp;
        }

    }
}
