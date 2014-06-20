using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.Map.Generation.LowLevel.Connections
{
    class Partition
    {
        private Partition identity;
        public bool isFixed;
        public int index;
        
        public Partition()
        {
            identity = this;
        }

        public Partition Get()
        {
            if (identity == this) return this;

            return identity.Get();
        }
        public void Set(Partition identity)
        {
            this.identity = identity;
        }
    }
}
