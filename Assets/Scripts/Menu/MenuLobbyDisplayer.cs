using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Game.Networking;
using Managers;

namespace Menu
{
    [RequireComponent(typeof(LayoutGroup))]
    public class MenuLobbyDisplayer : MonoBehaviour
    {
        public MenuLobbyEntry EntryPrefab;

        public Button StartButton;

        // Used for initialization.
        private void Awake()
        {
            Clear();

            StartButton.gameObject.SetActive(false);

            HostManager hostManager = HostManager.singleton as HostManager;

            if (hostManager)
            {
                hostManager.OnPlayerJoin += Refresh;
                hostManager.OnPlayerLeave += Refresh;
            }
            else
                Debug.LogError("HostManager singleton is null");
        }

        // Gets called when object get destroyed.
        private void OnDestroy()
        {
            HostManager hostManager = HostManager.singleton as HostManager;

            if (hostManager)
            {
                hostManager.OnPlayerJoin -= Refresh;
                hostManager.OnPlayerLeave -= Refresh;
            }
            else
                Debug.LogError("HostManager singleton is null");
        }

        // Clears the list with entries.
        private void Clear()
        {
            MenuLobbyEntry[] menuLobbyEntries = GetComponentsInChildren<MenuLobbyEntry>();

            foreach (MenuLobbyEntry menuLobbyEntry in menuLobbyEntries)
                Destroy(menuLobbyEntry.gameObject);
        }

        // Refreshes the list with entries.
        private void Refresh()
        {
            Clear();

            int current = 0;

            MatchData matchData = GameManager.Instance.MatchData;

            if (!matchData)
                return;

            foreach (PlayerData playerData in matchData.GetAllPlayerData().Values)
            {
                MenuLobbyEntry menuLobbyEntry = Instantiate(EntryPrefab);

                menuLobbyEntry.Initialize(playerData);
                menuLobbyEntry.transform.SetParent(transform);

                current++;
            }

            if (NetworkServer.active)
                StartButton.gameObject.SetActive(current > 0);
        }
    }
}