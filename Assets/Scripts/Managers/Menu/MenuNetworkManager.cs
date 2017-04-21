using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
//using UnityEngine.Networking.Types;
//using UnityEngine.Networking.Match;
using Game.Networking;
using Menu;

namespace Managers.Menu
{
    /// <summary>
    /// This class handles all networking that happens in the menu scene.
    /// </summary>
    public class MenuNetworkManager : MonoBehaviour
    {
        public MenuPanelManager MenuPanelManager;
        public MenuPanel LobbyPanel;

        // Used for initialization.
        private void Start()
        {
            HostManager hostManager = HostManager.Instance;

            if (hostManager)
            {
                hostManager.OnClientStart += OnConnected;
                hostManager.OnServerStart += OnConnected;
            }
            else
                Debug.LogError("HostManager instance is null");
        }

        // Get called when the object get destroyed.
        private void OnDestroy()
        {
            HostManager hostManager = HostManager.Instance;

            if (hostManager)
            {
                hostManager.OnClientStart -= OnConnected;
                hostManager.OnServerStart -= OnConnected;
            }
            //else
            //Debug.LogError("HostManager instance is null");
        }

        // When player connects.
        private void OnConnected()
        {
            MenuPanelManager.ShowPanel(LobbyPanel);
        }

        // Start a server.
        public void StartServer()
        {
            NetworkClient.ShutdownAll();
            NetworkServer.Shutdown();

            HostManager.Instance.StopServer();
            HostManager.Instance.StartServer();
        }

        public void JoinServer(InputField ip)
        {
            NetworkClient.ShutdownAll();
            NetworkServer.Shutdown();

            HostManager.Instance.StopClient();
            HostManager.Instance.networkAddress = ip.text;
            HostManager.Instance.StartClient();
        }

        // Starts the game.
        public void StartGame()
        {
            MatchData matchData = GameManager.Instance.MatchData;

            if (matchData)
                matchData.StartGame();
            else
                Debug.LogError("Not in a game");
        }

        // When disconnecting from the game.
        public void Disconnect()
        {
            if (NetworkClient.active)
            {
                Debug.Log("Stopping Client");

                HostManager.Instance.StopClient();
            }
            else if (NetworkServer.active)
            {
                Debug.Log("Stopping Server");

                HostManager.Instance.StopClient();
                HostManager.Instance.StopServer();
                HostManager.Instance.StopHost();
                NetworkServer.DisconnectAll();
                NetworkServer.Shutdown();

                foreach (MatchData matchData in FindObjectsOfType<MatchData>())
                    Destroy(matchData.gameObject);

                foreach (PlayerData playerData in FindObjectsOfType<PlayerData>())
                    Destroy(playerData.gameObject);

                Debug.Log("Stopping Server End");
            }

            //NetworkClient.ShutdownAll();
            //NetworkServer.DisconnectAll();
            //NetworkServer.Shutdown();
        }
    }
}