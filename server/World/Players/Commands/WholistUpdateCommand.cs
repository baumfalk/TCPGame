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
        private Player player;
        private String playerName;
        private String areaName;
        private StateChange stateChange;

        public WholistUpdateCommand(Model model, Player player, StateChange stateChange)
        {
            this.model = model;
            this.player = player;
            this.playerName = player.GetName();

            if (stateChange == StateChange.Changed_Area) this.areaName = player.GetBody().GetPosition().GetArea().GetName();
            else this.areaName = "Limbo";

            this.stateChange = stateChange;
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

            String stateString = StateChangeToString(stateChange);

            foreach (Player otherPlayer in playerList)
            {
                if (stateChange == StateChange.Logged_In)
                {
                    player.AddMessage("WHOLIST," + otherPlayer.GetName() + "," + otherPlayer.GetBody().GetPosition().GetArea().GetName() + "," + "PASSIVE_IN", tick);
                }

                otherPlayer.AddMessage("WHOLIST," + playerName + "," + areaName + "," + stateChange, tick);
            }
        }
    }
}
