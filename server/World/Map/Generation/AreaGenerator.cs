using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.IO;

namespace TCPGameServer.World.Map.Generation
{
    class AreaGenerator
    {
        public static AreaData Generate(GeneratorData generatorData)
        {
            DateTime start = DateTime.Now;

            AreaData toReturn;

            if (generatorData.areaType.Equals("Small Cave"))
            {
                toReturn = new Cave_SmallCaveGenerator(generatorData).Generate();
            }
            else if (generatorData.areaType.Equals("Tunnel Cave"))
            {
                toReturn = new Cave_TunnelGenerator(generatorData).Generate();
            }
            else
            {
                Output.Print("nonexistent map type " + generatorData.areaType + ", returning small cave");

                toReturn = new Cave_SmallCaveGenerator(generatorData).Generate();
            }

            Output.Print("generation took " + (DateTime.Now - start).TotalMilliseconds + " milliseconds");

            return toReturn;
        }
    }
}
