using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace TCPGameServer.World.Players.PlayerFiles
{
    class PlayerFile
    {
        private static String GetFileName(String name)
        {
            // path, based on where the files are in the git repository
            String playerPath = TCPGameSharedInfo.FileLocations.playerPath;

            // create the folder if it doesn't exist
            if (!Directory.Exists(playerPath)) Directory.CreateDirectory(playerPath);

            return playerPath + name + ".pla";
        }

        public static bool Exists(String name)
        {
            return (File.Exists(GetFileName(name)));
        }

        public static bool IsStub(String name)
        {
            HeaderData header = ReadHeader(name);

            return header.password == "";
        }

        public static PlayerFileData Read(String name)
        {
            StreamReader fileReader = new StreamReader(GetFileName(name));

            PlayerFileData data = Read(fileReader);

            fileReader.Close();

            return data;
        }

        private static PlayerFileData Read(StreamReader fileReader)
        {
            PlayerFileData toReturn = new PlayerFileData();

            toReturn.header = Header.Read(fileReader);

            return toReturn;
        }

        public static HeaderData ReadHeader(String name)
        {
            StreamReader fileReader = new StreamReader(GetFileName(name));

            HeaderData data = Header.Read(fileReader);

            fileReader.Close();

            return data;
        }

        public static void Write(PlayerFileData toWrite, String name)
        {
            StreamWriter fileWriter = new StreamWriter(GetFileName(name));

            Write(toWrite, fileWriter);

            fileWriter.Close();
        }

        private static void Write(PlayerFileData toWrite, StreamWriter fileWriter)
        {
            Header.Write(toWrite.header, fileWriter);
        }
    }
}
