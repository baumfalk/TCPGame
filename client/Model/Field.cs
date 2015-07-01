using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TCPGameSharedInfo;

namespace TCPGameClient.Model
{
    // simple class containing a representation for a tile
    public class Field
    {
        // the image key for this tile
        private TileRepresentation representation;
        private Creature creature;

        // field is created with the image key
        public Field(TileRepresentation representation, Creature creature)
        {
            this.representation = representation;
            this.creature = creature;
        }

        // retrieve the image key
        public TileRepresentation GetRepresentation()
        {
            return representation;
        }

        public Creature GetCreature()
        {
            return creature;
        }
    }
}
