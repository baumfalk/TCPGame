using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPGameServer.World.Players.Commands
{
    class WholistUpdateCommand : PlayerCommand
    {
        public enum StateChange { Logged_In, Changed_Area, Logged_Out };

        private Model model;
        private String playerName;
        private String areaName;
        private String stateChange;

        public WholistUpdateCommand(Model model, Player player, StateChange stateChange)
        {
            this.model = model;
            this.playerName = player.GetName();

            if (stateChange == StateChange.Changed_Area) this.areaName = player.GetBody().GetPosition().GetArea().GetName();
            else this.areaName = "Limbo";

            this.stateChange = StateChangeToString(stateChange);
        }

        private String StateChangeToString(StateChange stateChange)
        {
            switch (stateChange)
            {
                case StateChange.Logged_In: return "IN";
                case StateChange.Changed_Area: return "AREA";
                case StateChange.Logged_Out: return "OUT";
                default: return "NULL";
            }
        }

        public void Handle(int tick)
        {
            List<Player> playerList = model.getCopyOfPlayerList();

            foreach (Player player in playerList)
            {
                player.AddMessage("WHOLIST," + playerName + "," + areaName + "," + stateChange, tick);
            }
        }
    }
}
