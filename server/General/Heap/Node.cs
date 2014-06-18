using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.General.Heap
{
    class Node<T>
    {
        public int key;
        public T value;

        public Node(int key, T value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
