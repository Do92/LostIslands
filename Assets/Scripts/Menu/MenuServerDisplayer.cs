using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking.Match;
using Managers;

namespace Menu
{
    // This class handles the display of a server in the server list when searching for existing games.
    [RequireComponent(typeof(LayoutGroup))]
    public class MenuServerDisplayer : MonoBehaviour
    {
        public MenuServerEntry EntryPrefab;

        // Initialization.
        private void Awake()
        {
            Clear();
        }

        // Initialization.
        private void Start()
        {
            //MatchMakerManager matchMaker = MatchMakerManager.instance;

            //matchMaker.OnMatchList += Refresh;
        }

        // When the object gets destroyed.
        private void OnDestroy()
        {
            //MatchMakerManager matchMaker = MatchMakerManager.instance;

            //matchMaker.OnMatchList -= Refresh;
        }

        // Clears the list.
        private void Clear()
        {
            MenuServerEntry[] menuServerEntries = GetComponentsInChildren<MenuServerEntry>();

            foreach (MenuServerEntry menuServerEntry in menuServerEntries)
                Destroy(menuServerEntry.gameObject);
        }

        // Refreshes the list.
        public void Refresh()
        {
            Debug.Log("MenuServerDisplayer Refresh!");

            Clear();

            foreach (MatchDesc match in MatchMakerManager.Instance.MatchList)
            {
                MenuServerEntry menuServerEntry = Instantiate(EntryPrefab);

                menuServerEntry.Initialize(match, () => MatchMakerManager.Instance.JoinMatch(match));
                menuServerEntry.transform.SetParent(transform);
            }
        }
    }
}