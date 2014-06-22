using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.IO;

namespace TCPGameServer.World.Map.Generation.LowLevel.Connections
{
    public class Partition
    {
        private static int ID;
        private int id;

        private Partition identity;
        private bool isFixed;
        private int index;
        
        public Partition(int index, bool isFixed)
        {
            identity = this;
            this.index = index;
            this.isFixed = isFixed;

            id = ID++;
        }

        public int GetIndex()
        {
            return GetTop().index;
        }

        public bool IsFixed()
        {
            return isFixed;
        }

        private Partition GetTop()
        {
            if (identity.id == id) return this;

            return identity.GetTop();
        }
        public void SetParent(Partition identity)
        {
            GetTop().identity = identity;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            Partition partition = obj as Partition;
            if ((System.Object)partition == null) return false;

            return (partition.GetTop() == this.GetTop());
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
