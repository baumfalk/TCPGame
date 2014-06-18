using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.IO;
using TCPGameServer.General.Heap;

namespace TCPGameServer.General
{
    // heap priority queue based on an arraylist
    class PriorityQueue<T>
    {
        static int ID;

        private int id;

        private PriorityQueue<T> parent;

        private List<NodeHeap<T>> heapList;
        private int smallestHeap;
        private int smallestHeapSize;

        private int heapWithSmallestMin;
        private int smallestMin;

        private int count;

        public PriorityQueue()
        {
            this.id = ID++;

            heapList = new List<NodeHeap<T>>();
            heapList.Add(new NodeHeap<T>());

            smallestHeapSize = int.MaxValue;
            smallestHeap = 0;
            smallestMin = int.MaxValue;
            heapWithSmallestMin = 0;
        }

        public static void ResetID()
        {
            ID = 0;
        }

        public void Add(int key, T value)
        {
            if (key < smallestMin)
            {
                smallestMin = key;
                heapWithSmallestMin = smallestHeap;
            }

            heapList[smallestHeap].Add(key, value);

            smallestHeapSize = heapList[smallestHeap].Count();
            for (int n = 0; n < heapList.Count; n++)
            {
                if (heapList[n].Count() < smallestHeapSize)
                {
                    smallestHeap = n;
                    smallestHeapSize--;
                }
            }

            count++;
        }

        public T RemoveMin()
        {
            T toReturn = heapList[heapWithSmallestMin].RemoveMin();

            int lowest = int.MaxValue;
            for (int n = 0; n < heapList.Count; n++)
            {
                if (heapList[n].Count() > 0 && heapList[n].minKey() <= lowest)
                {
                    lowest = heapList[n].minKey();
                    heapWithSmallestMin = n;
                }
            }

            count--;

            return toReturn;
        }

        public int Count()
        {
            return count;
        }

        private List<NodeHeap<T>> getHeaps()
        {
            return heapList;
        }

        public int Merge(PriorityQueue<T> toAdd)
        {
            if (toAdd.parent != null)
            {
                return Merge(toAdd.parent);
            }

            List<NodeHeap<T>> mergeHeaps = toAdd.getHeaps();

            toAdd.parent = this;

            for (int n = 0; n < mergeHeaps.Count; n++) {
                heapList.Add(mergeHeaps[n]);
            }

            for (int n = 0; n < heapList.Count; n++) {
                if (heapList[n].Count() < smallestHeap)
                {
                    smallestHeapSize = heapList[n].Count();
                    smallestHeap = n;
                }

                if (heapList[n].Count() > 0 && heapList[n].minKey() <= smallestMin)
                {
                    smallestMin = heapList[n].minKey();
                    heapWithSmallestMin = n;
                }
            }

            count += toAdd.count;

            return toAdd.id;
        }

        public override string ToString()
        {
            return "Priorityqueue " + id + ", Count = " + count;
        }
    }
}
