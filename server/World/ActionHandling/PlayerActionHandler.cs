using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TCPGameServer.World.Map;

namespace TCPGameServer.World.ActionHandling
{
    class PlayerActionHandler
    {
        private Model world;

        public PlayerActionHandler(Model world)
        {
            this.world = world;
        }

        public void Handle(Player player, String[] splitCommand)
        {
            switch (splitCommand[1])
            {
                case "PLACE":
                    String area = splitCommand[2];
                    int ID = int.Parse(splitCommand[3]);

                    Tile position = world.GetTile(area, ID);

                    position.SetOccupant(player.GetBody());

                    return;
            }
        }
    }
}
