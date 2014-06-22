using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.Output;

namespace TCPGameServer.World.Map.Generation.LowLevel.Cave
{
    public class Cave_SmallCaveGenerator : CaveGenerator
    {
        public Cave_SmallCaveGenerator(GeneratorData generatorData)
            : base(generatorData)
        {
            
        }

        protected override string GetAreaType()
        {
            return "Small Cave";
        }
    }
}
