﻿using System;
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
        
        // created with it's representation
        public Creature(CreatureRepresentation representation)
        {
            this.representation = representation;
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
        }
        public Player GetPlayer()
        {
            return player;
        }

        public void VisionEvent(Tile changedTile, int tick)
        {
            vision.DoEvent(changedTile, this, tick);
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
