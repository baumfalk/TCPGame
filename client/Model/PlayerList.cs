﻿using System;
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
            Debug.Print("state change " + stateChange + " for update player list (" + playerName + ", " + areaName + ")");

            switch (stateChange)
            {
                case "IN":
                case "AREA":
                    UpdatePlayerLoc(playerName, areaName);
                    break;
                case "OUT":
                    RemovePlayer(playerName);
                    break;
                default:
                    Debug.Print("unknown");
                    break;
            }
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
