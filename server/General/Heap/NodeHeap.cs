using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.General.Heap
{
    class NodeHeap<T>
    {
        private List<Node<T>> nodeList = new List<Node<T>>();

        public void Add(int priority, T value)
        {
            Node<T> toAdd = new Node<T>(priority, value);

            Add(toAdd);
        }

        private void Add(Node<T> toAdd)
        {
            nodeList.Add(toAdd);

            DoBubbleUp(nodeList.Count - 1);
        }

        public int minKey()
        {
            return nodeList[0].key;
        }

        private void DoBubbleUp(int index)
        {
            if (IsRoot(index)) return;

            Node<T> parent = GetParent(index);

            if (parent.key > nodeList[index].key)
            {
                nodeList[(index - 1) / 2] = nodeList[index];

                nodeList[index] = parent;

                DoBubbleUp((index - 1) / 2);
            }
        }

        public T RemoveMin()
        {
            if (nodeList.Count == 0) return default(T);

            T toReturn = nodeList[0].value;

            nodeList[0] = nodeList[nodeList.Count - 1];

            nodeList.RemoveAt(nodeList.Count - 1);

            DoBubbleDown(0);

            return toReturn;
        }

        protected List<Node<T>> GetNodes()
        {
            return nodeList;
        }

        private void DoBubbleDown(int index)
        {
            Node<T> lowest = null;
            int lowestIndex = 0;

            if (HasRight(index))
            {
                lowest = GetRight(index);
                lowestIndex = 2 * (index + 1);
            }
            if (HasLeft(index))
            {
                if (lowest == null || GetLeft(index).key < lowest.key)
                {
                    lowest = GetLeft(index);
                    lowestIndex = 2 * (index + 1) - 1;
                }
            }

            if (lowest == null || lowest.key > nodeList[index].key) return;

            nodeList[lowestIndex] = nodeList[index];

            nodeList[index] = lowest;

            DoBubbleDown(lowestIndex);
        }

        private bool IsRoot(int index)
        {
            return index == 0;
        }

        private Node<T> GetParent(int index)
        {
            return nodeList[(index - 1) / 2];
        }

        private bool HasLeft(int index)
        {
            return 2 * (index + 1) - 1 < nodeList.Count;
        }

        private Node<T> GetLeft(int index)
        {
            return nodeList[2 * (index + 1) - 1];
        }

        private bool HasRight(int index)
        {

            return 2 * (index + 1) < nodeList.Count;
        }

        private Node<T> GetRight(int index)
        {
            return nodeList[2 * (index + 1)];
        }

        public int Count()
        {
            return nodeList.Count;
        }

        public void Clear()
        {
            nodeList = new List<Node<T>>();
        }
    }
}
