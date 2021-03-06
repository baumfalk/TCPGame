﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameServer.Control.Output;

using TCPGameServer.World.Map.Generation;
using TCPGameServer.World.Map.IO.MapFile;

namespace TCPGameServer.World.Map.IO
{
    class AreaReader
    {
        // the area being loaded, and the world it's in
        private static Area area;
        private static World world;

        // load an area from file, based on its name
        public static AreaData Load(String name, Area area, World world)
        {
            AreaReader.area = area;
            AreaReader.world = world;

            AreaFileData fileData = AreaFile.Read(name);

            GeneratorData generatorData = CreateGeneratorData(fileData);

            Log.Print("Generating area " + name + " of type " + fileData.header.areaType);

            AreaData toReturn = AreaGenerator.Generate(generatorData);
            toReturn.areaType = fileData.header.areaType;

            return toReturn;
        }

        private static GeneratorData CreateGeneratorData(AreaFileData fileData)
        {
            GeneratorData generatorData = new GeneratorData();

            generatorData.area = area;
            generatorData.world = world;

            generatorData.fileData = fileData;

            return generatorData;
        }
    }
}
