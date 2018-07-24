using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Mathematics;

namespace PathFinder.Release.Matusevich {

    //TODO: тут всё плохо
    public class SortedNodesHeap : Heap<Node> {

        public SortedNodesHeap() : base(HeapType.MinHeap) { }

        internal Node Shift() {
            return PopRoot();
        }

        internal Node Find(Vector2 point) {
            //TODO: не надо
            return items.FirstOrDefault(item => item.Point.Equals(point));
        }

        internal void Add(Node node) {
            Insert(node);
        }

        internal void UpdateNode(Node existingNode, Node node) {
            //TODO: ещё хуже
            items.Remove(existingNode);
            Insert(node);
        }
    }


    public enum HeapType {
        MinHeap,
        MaxHeap
    }

    public class Heap<T> where T : IComparable<T> {
        protected readonly List<T> items;

        private HeapType MinOrMax { get; set; }

        public T Root {
            get { return items[0]; }
        }

        public Heap(HeapType type) {
            items = new List<T>();
            this.MinOrMax = type;
        }

        public void Insert(T item) {
            items.Add(item);

            int i = items.Count - 1;

            bool flag = true;
            if (MinOrMax == HeapType.MaxHeap)
                flag = false;

            while (i > 0) {
                if ((items[i].CompareTo(items[(i - 1) / 2]) > 0) ^ flag) {
                    T temp = items[i];
                    items[i] = items[(i - 1) / 2];
                    items[(i - 1) / 2] = temp;
                    i = (i - 1) / 2;
                } else
                    break;
            }
        }

        private void DeleteRoot() {
            int i = items.Count - 1;

            items[0] = items[i];
            items.RemoveAt(i);

            i = 0;

            bool flag = true;
            if (MinOrMax == HeapType.MaxHeap)
                flag = false;

            while (true) {
                int leftInd = 2 * i + 1;
                int rightInd = 2 * i + 2;
                int largest = i;

                if (leftInd < items.Count) {
                    if ((items[leftInd].CompareTo(items[largest]) > 0) ^ flag)
                        largest = leftInd;
                }

                if (rightInd < items.Count) {
                    if ((items[rightInd].CompareTo(items[largest]) > 0) ^ flag)
                        largest = rightInd;
                }

                if (largest != i) {
                    T temp = items[largest];
                    items[largest] = items[i];
                    items[i] = temp;
                    i = largest;
                } else
                    break;
            }
        }

        public T PopRoot() {
            T result = items[0];

            DeleteRoot();

            return result;
        }

        public void Clear() {
            items.Clear();
        }

        internal bool Empty() {
            return items.Count == 0;
        }
    }
}