using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.Networking;
//using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System;

namespace Menu
{
    /// <summary>
    /// This class is for every entry in the server list when searching for existing games.
    /// </summary>
    public class MenuServerEntry : MonoBehaviour
    {
        public Text ServerName;
        public Text ServerPlayers;

        private Action connectAction;

        // Initialization.
        public void Initialize(MatchDesc match, Action connectAction)
        {
            ServerName.text = match.name;
            ServerPlayers.text = match.currentSize + "/" + match.maxSize;

            this.connectAction = connectAction;
        }

        // When player chooses to connect to the game.
        public void Connect()
        {
            connectAction.Invoke();
        }
    }
}