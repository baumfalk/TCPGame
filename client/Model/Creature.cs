using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TCPGameSharedInfo;

namespace TCPGameClient.Model
{
    public class Creature : TCPGameSharedInfo.Creatures.Creature
    {
        private static Creature NO_CREATURE = new Creature(CreatureRepresentation.None);

        private static Dictionary<int, Creature> knownCreatures = new Dictionary<int,Creature>();

        public Creature(CreatureRepresentation creatureRepresentation) :
            base(creatureRepresentation)
        {

        }

        public static Creature parse(String creatureString)
        {
            if (creatureString.Equals("")) return NO_CREATURE;

            String[] creatureSplit = creatureString.Split('.');

            int ID = int.Parse(creatureSplit[0]);

            CreatureRepresentation representation = (CreatureRepresentation)Enum.Parse(typeof(CreatureRepresentation), creatureSplit[1]);

            Creature editing;
            if (knownCreatures.ContainsKey(ID))
            {
                knownCreatures.TryGetValue(ID, out editing);
                editing.representation = representation;
            }
            else editing = new Creature(representation);

            editing.currentHitpoints = int.Parse(creatureSplit[2]);
            editing.maxHitpoints = int.Parse(creatureSplit[3]);
            editing.currentMana = int.Parse(creatureSplit[4]);
            editing.maxMana = int.Parse(creatureSplit[5]);

            return editing;
        }
    }
}
