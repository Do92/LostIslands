using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Managers;
using QuestionItem;

namespace Game.Networking
{
    // Structure for a character
    // Will be serializable for the Unity editor so you can adjust this in the editor
    [Serializable]
    public struct CharacterInfo
    {
        [HideInInspector]
        public int Id;

        //public Color MainColor;
        public Color EmissionColor; // This makes the main color for the character itself lighter
        public Color EyeFadeColor; // Used for the scoreboard entry
        public Sprite Image;
        public string Name;
    }

    public class PlayerData : NetworkBehaviour
    {
        public static int GlobalPlayerId;

        [SyncVar]
        public int PlayerId;

        [SyncVar]
        public int CharacterId;

        public int Score;
        [SyncVar]
        public int ActionPoints;
        [SyncVar]
        public ClientResponseInfo LastResponse;

        public CharacterInfo Character { get { return CharacterManager.Instance.GetCharacter(CharacterId); } }

        public int MovementBuffs;
        public int StrengthBuffs;
        public int RangeBuffs;

        public bool IsLoaded;
        public bool HasAnsweredCorrectly;
        public bool HasAnswered;
        //public List<bool> AnswerResults; // Question list is shuffled and so will this -_-

        private MatchData matchData;
        private int sceneToLoad;

        // Initialization.
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            matchData = FindObjectOfType<MatchData>();
        }

        // Initialization.
        private void Start()
        {
            if (isServer)
            {
                PlayerId = GlobalPlayerId++;
                CharacterId = matchData.GetCharacter().Id;
            }

            Invoke("Register", 0.1f);
        }

        // When the object get destroyed.
        private void OnDestroy()
        {
            if (isServer)
                matchData.ReturnCharacter(CharacterId);

            matchData.UnregisterPlayer(PlayerId);
        }

        // Registers a player.
        private void Register()
        {
            matchData.RegisterPlayer(PlayerId, this, isLocalPlayer);
        }

        // Adds a score.
        public void AddScore(int score)
        {
            if (isServer)
                RpcAddScore(score);

            Score += score;
            matchData.MatchSceneManager.UpdatePlayerCards();
        }

        // Updates score over RPC.
        [ClientRpc]
        private void RpcAddScore(int score)
        {
            AddScore(score);
        }

        // Changes a buff for the player.
        public void ChangeBuff(BuffType buff, int amount)
        {
            if (isServer)
                RpcChangeBuff(buff, amount);

            switch (buff)
            {
                case BuffType.Movement:
                    MovementBuffs += amount;
                    break;
                case BuffType.Range:
                    RangeBuffs += amount;
                    break;
                case BuffType.Strength:
                    StrengthBuffs += amount;
                    break;
            }

            if (isLocalPlayer)
                matchData.MatchSceneManager.UpdateBuffs();
        }

        // Updates the buff over RPC.
        [ClientRpc]
        private void RpcChangeBuff(BuffType buff, int amount)
        {
            ChangeBuff(buff, amount);
        }

        // Commands the player move.
        [Command]
        public void CmdDoMove(DirectionType direction)
        {
            if (ActionPoints > 0)
            {
                ActionPoints--;

                matchData.MatchSceneManager.MovePlayer(PlayerId, direction);
                matchData.SendTurn(PlayerId);
            }
        }

        // Scene Switching
        [Command]
        private void CmdSetLoaded()
        {
            IsLoaded = true;
        }

        // Sends the player answer.
        [Command]
        public void CmdSubmitAnswer(ClientResponseInfo clientResponse)
        {
            HasAnswered = true;
            LastResponse = clientResponse;

            GiveActionPoints();
        }

        // Assigns action points.
        public void GiveActionPoints()
        {
            // Answered
            if (!LastResponse.IsQuestionAnswered || !QuestionManager.ValidateAnswerKeyData(matchData.CurrentQuestion, LastResponse, isServer))
            {
                ActionPoints = matchData.CurrentGameMode.IncorrectAnswerPoints + MovementBuffs;
                HasAnsweredCorrectly = false;
                //AnswerResults.Add(false);
            }
            else
            {
                ActionPoints = matchData.CurrentGameMode.CorrectAnswerPoints + MovementBuffs;
                HasAnsweredCorrectly = true;
                //AnswerResults.Add(true);
            }
        }

        // Loads a scene.
        public void LoadScene(int scene)
        {
            sceneToLoad = scene;
            SceneManager.LoadScene(scene);
        }

        // When level is loaded.
        [ClientCallback]
        public void OnLevelWasLoaded(int scene)
        {
            if (sceneToLoad == scene)
                CmdSetLoaded();
        }
    }
}