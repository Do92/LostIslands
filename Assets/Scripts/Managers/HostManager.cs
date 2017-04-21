using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using Game.Networking;
using Miscellaneous;

// Temporary fix explained
// =================================================================
// This script is a temporary workaround for a bug within the UNET code.
// It is a replacement that we are using instead of one of unity's own components.
// The task of that component was to let players connect to the host and spawn their player object accordingly.
// This component however was bugged. It didn't do what it had to do within the UNET code.
// This is because the UNET was just released and this was one of the first UNET builds.
// Therefore this is our temporary replacement for the unity component "LobbyManager".
// If this bug will be fixed in the future, you can replace this script back with the LobbyManager.
// All you really need to change is to reapply the hooks on the Methods.
// 12-10-2015
// ==================================================================

namespace Managers
{
    public class HostManager : NetworkManager
    {
        public bool Debug = false;

        public static HostManager Instance { get { return singleton as HostManager; } }

        public int DisconnectScene = 1;

        //      =======================================
        //                      Server
        //      =======================================

        public PlayerData PlayerData;
        public GameObject MatchData;

        // Events
        public delegate void VoidEvent();

        public event VoidEvent OnPlayerJoin = delegate { };
        public event VoidEvent OnPlayerLeave = delegate { };
        public event VoidEvent OnClientStart = delegate { };
        public event VoidEvent OnServerStart = delegate { };

        // This hook is called when a server is stopped - including when a host is stopped.
        public override void OnStartServer()
        {
            base.OnStartServer();

            if (Debug)
                UnityEngine.Debug.Log("Host Successfully Started");

            // Has to be set 1 frame later
            StartCoroutine(CreateServerObject());
        }

        // Create server object.
        private IEnumerator CreateServerObject()
        {
            yield return new WaitForEndOfFrame();

            GameObject serverObject = Instantiate<GameObject>(MatchData);
            NetworkServer.Spawn(serverObject);

            OnServerStart();
        }

        // When a player joins
        public void CallOnPlayerJoin()
        {
            OnPlayerJoin();
        }

        // When a players leaves.
        public void CallOnPlayerLeave()
        {
            OnPlayerJoin();
        }

        // Called on the server when a new client connects
        public override void OnServerConnect(NetworkConnection connection)
        {
            base.OnServerConnect(connection);

            if (Debug)
                UnityEngine.Debug.Log("Adding Player: " + connection.connectionId);

            MatchData matchData = GameManager.Instance.MatchData;

            if (!matchData.IsConnectionRegistered(connection))
            {
                PlayerData playerData = Instantiate(PlayerData);
                NetworkServer.AddPlayerForConnection(connection, playerData.gameObject, 0);

                matchData.RegisterConnection(connection, playerData.PlayerId);
            }
            else
                UnityEngine.Debug.LogError("Player is already connected");
        }

        // Called on the server when a client disconnects.
        public override void OnServerDisconnect(NetworkConnection connection)
        {
            base.OnServerDisconnect(connection);

            if (Debug)
                UnityEngine.Debug.Log("Removing Player: " + connection.connectionId);

            MatchData matchData = GameManager.Instance.MatchData;

            if (matchData.IsConnectionRegistered(connection))
                matchData.UnregisterConnection(connection);
            else
                UnityEngine.Debug.LogError("Connection is already unregistered");

        }

        // Called on the server when a client adds a new player with ClientScene.AddPlayer.
        public override void OnServerAddPlayer(NetworkConnection connection, short playerControllerId)
        {
            if (Debug)
                UnityEngine.Debug.Log("OnServerAddPlayer");
            // Don't do anything
        }

        // Called on the server when a client removes a player.
        public override void OnServerRemovePlayer(NetworkConnection connection, PlayerController playerController)
        {
            if (Debug)
                UnityEngine.Debug.Log("OnServerRemovePlayer");
            // Don't do anything
        }

        // Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
        public override void OnServerSceneChanged(string sceneName)
        {
            if (Debug)
                UnityEngine.Debug.Log("OnServerSceneChanged");
            // Do nothing
        }

        //      =======================================
        //                      Client
        //      =======================================

        // Called on the client when connected to a server.
        public override void OnClientConnect(NetworkConnection connection)
        {
            base.OnClientConnect(connection);

            if (Debug)
                UnityEngine.Debug.Log("OnClientConnect");

            OnClientStart();
        }

        // Called on clients when disconnected from a server.
        public override void OnClientDisconnect(NetworkConnection connection)
        {
            base.OnClientDisconnect(connection);

            if (Debug)
                UnityEngine.Debug.Log("OnClientDisconnect");

            SceneManager.LoadScene(DisconnectScene);
            //NetworkServer.Reset();
        }

        // When the client encounters an error.
        public override void OnClientError(NetworkConnection connection, int errorCode)
        {
            UnityEngine.Debug.LogError("OnClientError: " + ((NetworkError)errorCode).ToString());

            MenuUtilities.ErrorText = ((NetworkError)errorCode).ToString();
        }
    }
}