using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;

namespace TCPGameClient.Model
{
    class PlayerList
    {
        // list of players currently online
        private Dictionary<String, String> playersOnline;

        public PlayerList()
        {
            playersOnline = new Dictionary<string, string>();
        }

        public void updatePlayerList(String playerName, String areaName, String stateChange)
        {
            switch (stateChange)
            {
                case "IN":
                    AddPlayer(playerName);
                    break;
                case "AREA":
                    UpdatePlayerLoc(playerName, areaName);
                    break;
                case "OUT":
                    RemovePlayer(playerName);
                    break;
                case "PASSIVE_IN":
                    AddPlayer(playerName);
                    UpdatePlayerLoc(playerName, areaName);
                    break;
                default:
                    Debug.Print("unknown state change for update player list");
                    break;
            }
        }

        private void AddPlayer(string playerName)
        {
            playersOnline.Add(playerName, "");
        }

        private void UpdatePlayerLoc(string playerName, string playerLoc)
        {
            playersOnline[playerName] = playerLoc;
        }

        private void RemovePlayer(string playerName)
        {
            playersOnline.Remove(playerName);
        }

        public List<string> GetList()
        {
            List<string> lst = new List<string>();
            foreach (var tuple in playersOnline)
            {
                lst.Add(",," + tuple.Key + "," + tuple.Value);
            }
            return lst;
        }
    }
}
