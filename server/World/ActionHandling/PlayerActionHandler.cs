using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    int x = int.Parse(splitCommand[2]);
                    int y = int.Parse(splitCommand[3]);
                    int z = int.Parse(splitCommand[4]);

                    Tile position = world.FindTileAt(x, y, z);

                    position.setOccupant(player.getBody());

                    LookActionHandler.Look(player, world);
                    return;
            }
        }
    }
}
