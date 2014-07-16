using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TCPGameServer.World.Map;
using TCPGameServer.World.Players;

using TCPGameSharedInfo;

namespace TCPGameServer.World.Creatures
{
    // a creature is any mobile object, including players and such which may extend it
    public class Creature
    {
        private Player player;

        // image key
        private CreatureRepresentation representation;
        // position in the world
        private Tile position;

        private VisionSystem vision;
        public enum RegisterMode { None, Outer, All };
        
        // created with it's representation
        public Creature(CreatureRepresentation representation)
        {
            this.representation = representation;

            vision = new NPCVisionSystem();
        }

        // a creature can be a player. Methods to check if this is the case, and make
        // it the case. (Or not).
        public bool IsPlayer()
        {
            return (player != null);
        }
        public void SetPlayer(Player player)
        {
            this.player = player;

            if (player == null) vision = new NPCVisionSystem();
            else vision = new PlayerVisionSystem();
        }
        public Player GetPlayer()
        {
            return player;
        }

        // the creature sees something
        public void VisionEvent(Tile changedTile)
        {
            vision.DoVisionEvent(changedTile);
        }
        public void UpdateVision(int tick)
        {
            vision.HandleVisionEvents(this, tick);
        }

        public void VisionDeregister(RegisterMode registerMode)
        {
            switch (registerMode)
            {
                case RegisterMode.None:
                    break;
                case RegisterMode.Outer:
                    VisionDeregister(GetPosition().GetTilesAtRange(vision.GetVisionRange()));
                    break;
                case RegisterMode.All:
                    VisionDeregister(GetPosition().GetTilesInRange(vision.GetVisionRange()));
                    break;
            }
        }
        public void VisionDeregister(List<Tile> tilesToDeregister)
        {
            foreach (Tile tile in tilesToDeregister)
            {
                tile.DeregisterAsViewing(this);
            }
        }

        public void VisionRegister(RegisterMode registerMode)
        {
            switch (registerMode)
            {
                case RegisterMode.None:
                    break;
                case RegisterMode.Outer:
                    VisionRegister(GetPosition().GetTilesAtRange(vision.GetVisionRange()));
                    VisionRegister(GetPosition().GetTilesAtRange(vision.GetVisionRange() - 1));
                    break;
                case RegisterMode.All:
                    VisionRegister(GetPosition().GetTilesInRange(vision.GetVisionRange()));
                    break;
            }
        }
        public void VisionRegister(List<Tile> tilesToRegister)
        {
            foreach (Tile tile in tilesToRegister)
            {
                tile.RegisterAsViewing(this);
            }
        }

        // position can be changed and requested freely
        public void SetPosition(Tile position)
        {
            this.position = position;
        }
        public Tile GetPosition()
        {
            return position;
        }

        // image key can be requested and changed freely
        public void SetRepresentation(CreatureRepresentation representation)
        {
            this.representation = representation;
        }
        public CreatureRepresentation GetRepresentation()
        {
            return representation;
        }
    }
}
