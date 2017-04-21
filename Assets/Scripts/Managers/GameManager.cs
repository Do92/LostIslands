using UnityEngine;
using System.Collections.Generic;
using Game;
using Game.GameModes;
using Game.Networking;
using UnityEngine.SceneManagement;
using Miscellaneous;

namespace Managers
{
    public class GameManager : Singleton<GameManager>
    {
        public static readonly string PermissionKey = "permission";
        public Object MenuScene;

        // Database specific information, can be adjusted in the Unity editor
        public string DatabaseUrl;
        public string EncryptionSalt;

        public Player PlayerPrefab;

        [HideInInspector]
        public List<GameMode> GameModes = new List<GameMode>();

        private MatchData matchData;

        public MatchData MatchData
        {
            get
            {
                if (!matchData)
                    matchData = FindObjectOfType<MatchData>();

                return matchData;
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            LoadResources();

            SceneManager.LoadScene("Menu");
        }

        // Load all game modes out of /Resources/GameModes/
        public void LoadResources()
        {
            foreach (GameObject gameMode in Resources.LoadAll<GameObject>("GameModes"))
                GameModes.Add(gameMode.GetComponent<GameMode>());
        }

        /// <summary>
        /// This uses the existing collection "GameModes" with the specified index to get the desired game mode.
        /// The current game mode will then get changed to that game mode.
        /// </summary>
        /// <param name="desiredGameModeIndex">The desired game mode index of the game mode collection.</param>
        public void ChangeGameMode(int desiredGameModeIndex)
        {
            HostManager.Instance.MatchData.GetComponent<MatchData>().CurrentGameMode = GameModes[desiredGameModeIndex];
        }

        // Small debug
        private void OnLevelWasLoaded(int level)
        {
            Debug.Log("OnLevelWasLoaded: " + level);
        }
    }
}