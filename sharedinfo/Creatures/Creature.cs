using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameSharedInfo.Creatures
{
    public class Creature
    {
        private static int newID = 0;

        // image key
        public CreatureRepresentation representation { get; set; }

        // unique ID for each creature
        public int ID { get; set; }

        public int currentHitpoints { get; set; }
        public int maxHitpoints { get; set; }

        public int currentMana { get; set; }
        public int maxMana { get; set; }

        // created with it's representation
        public Creature(CreatureRepresentation representation)
        {
            this.ID = newID++;
            this.representation = representation;

            this.currentHitpoints = 50;
            this.maxHitpoints = 100;

            this.currentMana = 75;
            this.maxMana = 100;
        }

        public String getCreatureString()
        {
            return ID + "." + representation + "." + currentHitpoints + "." + maxHitpoints + "." + currentMana + "." + maxMana;
        }
    }
}
