using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.Map.Generation.LowLevel.Connections
{
    class Partition
    {
        private Partition identity;
        private bool isFixed;
        private int index;
        
        public Partition(int index, bool isFixed)
        {
            identity = this;
            this.index = index;
            this.isFixed = isFixed;
        }

        public int GetIndex()
        {
            return GetTop().GetIndex();
        }

        public bool IsFixed()
        {
            return isFixed;
        }

        private Partition GetTop()
        {
            if (identity == this) return this;

            return identity.GetTop();
        }
        public void SetParent(Partition identity)
        {
            GetTop().identity = identity;
        }
    }
}
