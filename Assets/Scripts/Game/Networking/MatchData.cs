using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using Game.GameModes;
using Managers;
using QuestionItem;
using Miscellaneous;

namespace Game.Networking
{
    public enum MatchStateType
    {
        Initial,
        Loading,
        Intro,
        FadeFromBlack,
        LevelGeneration,
        Start,
        QuestionFadeIn,
        QuestionPreparation,
        QuestionProcessing,
        QuestionResult,
        QuestionFadeOut,
        Player,
        FadeToBlack,
        Outro,
        GameOverMenu
    }

    public class MatchData : NetworkBehaviour
    {
        public GameMode CurrentGameMode;
        public Level Level;

        private bool Debug = true;

        [Header("Values")]
        public int GameScene;
        public byte[,] LevelData { get; private set; }

        public int PlayerId { get; private set; }

        public PlayerData PlayerData { get { return playerDataCollection[PlayerId]; } }

        public Player Player { get { return Level.GetPlayer(PlayerId); } }

        [HideInInspector]
        public MatchSceneManager MatchSceneManager;

        [SyncVar(hook = "MatchStateChanged")]
        public MatchStateType MatchState = MatchStateType.Initial;

        [SyncVar(hook = "MatchTurnChanged")]
        public int CurrentTurnId = -1;

        [SyncVar]
        public QuestionInfo CurrentQuestion;

        [SyncVar]
        public float QuestionTime = 0;

        [SyncVar]
        public float TurnTime = -1;

        [SyncVar]
        public int CurrentRound = 0;

        // SERVER ONLY, binds "connectionId" to "playerId"
        private Dictionary<int, int> playerConnections = new Dictionary<int, int>();

        // EVERYONE, binds "playerId" to "PlayerData"
        private Dictionary<int, PlayerData> playerDataCollection = new Dictionary<int, PlayerData>();

        private HashSet<int> usedCharacters = new HashSet<int>();

        public string[] Descriptions;

        // Returns a not used character.
        public CharacterInfo GetCharacter()
        {
            for (int id = 0; id < CharacterManager.Instance.GetCharacterCount(); id++)
            {
                if (!usedCharacters.Contains(id))
                {
                    usedCharacters.Add(id);
                    return CharacterManager.Instance.GetCharacter(id);
                }
            }

            return default(CharacterInfo);
        }

        // Returns character by ID.
        public void ReturnCharacter(int id)
        {
            usedCharacters.Remove(id);
        }

        // Initialization.
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        // Starts the game.
        public void StartGame()
        {
            SceneManager.LoadScene(GameScene);
            RpcStartGame();
        }

        // Changes the match state.
        public void ChangeMatchState(MatchStateType matchState)
        {
            if (Debug)
                UnityEngine.Debug.Log("Game Loop: " + matchState);

            MatchState = matchState;

            MatchStateChanged(matchState);
        }

        // Sends turn to a player.
        public void SendTurn(int playerId)
        {
            int[] availableMoveOptions = ArrayUtilities.ToIntArray(MatchSceneManager.GetAvailableMoveOptions(playerDataCollection[playerId]));
            RpcSendTurn(playerId, GetPlayerData(playerId).ActionPoints, availableMoveOptions);
        }

        // Sends RPC turn.
        [ClientRpc]
        private void RpcSendTurn(int playerId, int actionPoints, int[] newMoveOptions)
        {
            MoveOptionType[] moveOptions = ArrayUtilities.ToEnumArray<MoveOptionType>(newMoveOptions);
            MatchSceneManager.SetTurn(actionPoints, moveOptions);
        }

        // Sets a level.
        public void SetLevel(byte[,] levelData)
        {
            LevelData = levelData;
        }

        // Calls start to every player.
        [ClientRpc]
        private void RpcStartGame()
        {
            playerDataCollection[PlayerId].LoadScene(GameScene);
        }

        // Sends question to every player.
        [ClientRpc]
        private void RpcSetQuestion(QuestionInfo question)
        {
            CurrentQuestion = question;
        }

        // Hook
        private void MatchStateChanged(MatchStateType matchState)
        {
            if (Debug)
                UnityEngine.Debug.Log("Game Client: MatchState is " + matchState);

            MatchState = matchState;
            MatchSceneManager.SetMatchState(matchState);
        }

        // Sets new turn id.
        private void MatchTurnChanged(int playerId)
        {
            if (Debug)
                UnityEngine.Debug.Log("Game Client: Match turn changed to " + playerId);

            CurrentTurnId = playerId;
            Level.SetTurnIndicator(playerId);
            MatchSceneManager.SetTurn(playerId);
        }

        // When the scene is loaded.
        public void OnLevelWasLoaded(int scene)
        {
            if (scene == GameScene)
            {
                Level = FindObjectOfType<Level>();
                MatchSceneManager = FindObjectOfType<MatchSceneManager>();
                MatchSceneManager.SetServer(isServer);

                if (isServer)
                {
                    GameMode gameMode = Instantiate(CurrentGameMode);
                    gameMode.MatchData = this;
                    gameMode.MatchSceneManager = MatchSceneManager;
                }
            }
        }

        // SERVER ONLY
        public bool IsConnectionRegistered(NetworkConnection connection)
        {
            return playerConnections.ContainsKey(connection.connectionId);
        }

        // Gets player id according to its network connection.
        public int GetPlayerId(NetworkConnection connection)
        {
            return playerConnections[connection.connectionId];
        }

        // Registers a network connection.
        public void RegisterConnection(NetworkConnection connection, int playerId)
        {
            if (Debug)
                UnityEngine.Debug.Log("Server: Bound connection \"" + connection.connectionId + "\" to playerId \"" + playerId + "\"");

            playerConnections.Add(connection.connectionId, playerId);
        }

        // Unregisters a player connection.
        public void UnregisterConnection(NetworkConnection connection)
        {
            if (Debug)
                UnityEngine.Debug.Log("Server: Unregistered connection \"" + connection.connectionId + "\"");

            playerConnections.Remove(connection.connectionId);
        }

        // EVERYONE
        public Dictionary<int, PlayerData> GetAllPlayerData()
        {
            return playerDataCollection;
        }

        // Returns a playerData according to a playerID.
        public PlayerData GetPlayerData(int playerId)
        {
            return playerDataCollection[playerId];
        }

        // Registers a player.
        public void RegisterPlayer(int playerId, PlayerData playerData, bool isMine)
        {
            if (Debug)
                UnityEngine.Debug.Log("Registering Player ID: " + playerId);
            if(playerDataCollection == null)
                playerDataCollection = new Dictionary<int, PlayerData>();

            playerDataCollection.Add(playerId, playerData);

            if (isMine)
                PlayerId = playerId;

            HostManager.Instance.CallOnPlayerJoin();
        }

        // Unregisters a player.
        public void UnregisterPlayer(int playerId)
        {
            if (Debug)
                UnityEngine.Debug.Log("Unregistering Player ID: " + playerId);

            playerDataCollection.Remove(playerId);

            HostManager.Instance.CallOnPlayerLeave();
        }

        public void SendQuestionDescriptions()
        {
            int questionAmount = QuestionManager.Instance.QuestionList.Count;

            // This is required for releasing the client restriction as we can't send the complete question list
            string[] descriptions = new string[questionAmount];

            for (int i = 0; i < questionAmount; i++)
                descriptions[i] = QuestionManager.Instance.QuestionList.ElementAt(i).Description;

            RpcSendQuestionDescriptions(descriptions);
        }

        // Preparation for the feedback screen
        [ClientRpc]
        private void RpcSendQuestionDescriptions(string[] descriptions)
        {
            Descriptions = descriptions;
        }
    }
}