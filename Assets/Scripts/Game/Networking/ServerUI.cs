using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Interface;
using Managers;
using QuestionItem;

namespace Game.Networking
{
    /// <summary>
    /// This class handles everything based around the server UI.
    /// </summary>
    public class ServerUI : MonoBehaviour
    {

        // Main server canvas
        [Header("Main Canvas")]
        public RectTransform LoadPanel;
        public RectTransform IntroPanel;
        public RectTransform GamePanel;
        public RectTransform QuestionPanel;
        public RectTransform QuestionResultPanel;
        public RectTransform OutroPanel;
        public RectTransform OutroPanelFail;
        public RectTransform ScorePanel;

        // Question Panel
        [Header("Question Panel")]
        public Text QuestionText;
        public Text[] AnswerTexts;
        public Image TimeLeftBar;

        // Question Done
        [Header("Question Result")]
        public Text CurrentQuestionText;
        public Text[] AnswerKeyTexts;
        public Text AnswerKeyText;
        public GameObject CorrectPlayerResults; // Parent game object as container for correct player results
        public GameObject IncorrectPlayerResults; // Parent game object as container for incorrect player results
        public GameObject PlayerResultCardPrefab;
        public Sprite AnswerCorrectSprite;
        public Sprite AnswerIncorrectSprite;

        [Header("Player State")]
        public PlayerCard[] PlayerCards;
        public Text TurnAmountText;
        private string turnAmountString;

        [Header("Scoreboard")]
        private RankManager rankManager;
        public GameObject PersistentMenuItems;
        [Header("Other"), Range(0,1f)]
        public float succesPercentage = .8f;

        // Question Data
        private QuestionInfo question;

        private MatchData matchData;

        // Initialization.
        private void Awake()
        {
            HideAll();
            LoadPanel.gameObject.SetActive(true);
            turnAmountString = TurnAmountText.text;

            matchData = GameManager.Instance.MatchData;

            rankManager = GetComponent<RankManager>();
        }

        // Gets called every frame.
        private void Update()
        {
            if (matchData.MatchState.Equals(MatchStateType.QuestionPreparation))
                TimeLeftBar.fillAmount = matchData.QuestionTime / matchData.CurrentGameMode.QuestionMaxTime;
        }

        // Shows the panel according to the game state.
        public void ShowPanel(MatchStateType matchState)
        {
            HideAll();

            switch (matchState)
            {
                case MatchStateType.Loading:
                    LoadPanel.gameObject.SetActive(true);
                    SetLoadingState();
                    break;
                case MatchStateType.Intro:
                    IntroPanel.gameObject.SetActive(true);
                    break;
                case MatchStateType.QuestionPreparation:
                    QuestionPanel.gameObject.SetActive(true);
                    SetQuestionState();
                    break;
                case MatchStateType.QuestionResult:
                    QuestionResultPanel.gameObject.SetActive(true);
                    SetQuestionResultState();
                    break;
                case MatchStateType.Player:
                    SetPlayerState();
                    GamePanel.gameObject.SetActive(true);
                    break;
                case MatchStateType.Outro:

                    Level level = FindObjectOfType<Level>();
                    if(((float)level.healedTileCount / (float)level.tileCount) > succesPercentage)
                        OutroPanel.gameObject.SetActive(true);
                    else
                    {
                        OutroPanelFail.gameObject.SetActive(true);
                    }
                    break;
                case MatchStateType.GameOverMenu:
                    rankManager.SetRanking();
                    PersistentMenuItems.SetActive(true);
                    ScorePanel.gameObject.SetActive(true);
                    break;
            }
        }

        // Disables all panels.
        private void HideAll()
        {
            RectTransform[] panelsToHide =
            {
                LoadPanel,
                IntroPanel,
                QuestionPanel,
                QuestionResultPanel,
                GamePanel,
                ScorePanel,
                OutroPanel,
                OutroPanelFail
            };

            foreach (RectTransform panel in panelsToHide)
                panel.gameObject.SetActive(false);
        }

        // Sets the loading state.
        private void SetLoadingState()
        {
            turnAmountString = TurnAmountText.text;

            foreach (PlayerCard playerCard in PlayerCards)
                playerCard.gameObject.SetActive(false);

            //for (int i = 0; i < matchData.GetAllPlayerData().Count; i++)
            //    PlayerCards[i].gameObject.SetActive(true);

            //AssignPlayerCards();
        }

        // Assign player cards to players.
        private void AssignPlayerCards()
        {
            int i = 0;
            foreach (KeyValuePair<int, PlayerData> keyValuePlayerData in matchData.GetAllPlayerData())
            {
                CharacterInfo character = CharacterManager.Instance.GetCharacter(keyValuePlayerData.Value.CharacterId);

                PlayerCards[i].Instantiate(character);
                PlayerCards[i].PlayerId = keyValuePlayerData.Key;

                i++;
            }

            UpdatePlayerCards();
        }

        // Sets question state.
        private void SetQuestionState()
        {
            question = matchData.CurrentQuestion;
            QuestionText.text = question.QuestionText;

            int index = 0;
            foreach (AnswerInfo answerOption in question.AnswerOptions)
                AnswerTexts[index++].text = answerOption.AnswerText;
        }

        // Sets question result state.
        private void SetQuestionResultState()
        {
            CurrentQuestionText.text = question.QuestionText;
            if (question.AnswerKeyDataText is string)
            {
                Debug.Log("AnswerKeyDataText is string");
                AnswerKeyText.text = (string)question.AnswerKeyDataText;
            }
            else if (question.AnswerKeyDataText is string[])
            {
                Debug.Log("AnswerKeyDataText is string[]");
                //foreach (string answerKeyText in (string[])question.AnswerKeyDataText)
                //AnswerKeyTexts = (string[])question.AnswerKeyDataText
                for (int i = 0; i < ((string[])question.AnswerKeyDataText).Length; ++i)
                    AnswerKeyTexts[i].text = ((string[])question.AnswerKeyDataText)[i];

                //AnswerKeyText.text = ((string[])question.AnswerKeyDataText)[0];
            }

            // Deleting any previously set player results
            foreach (Transform childTransform in CorrectPlayerResults.transform)
                Destroy(childTransform.gameObject);
            foreach (Transform childTransform in IncorrectPlayerResults.transform)
                Destroy(childTransform.gameObject);

            // Repopulate the player results container game objects
            foreach (PlayerData playerData in matchData.GetAllPlayerData().Values)
            {
                PlayerResultCard playerResultCard = Instantiate(PlayerResultCardPrefab).GetComponent<PlayerResultCard>();
                CharacterInfo character = CharacterManager.Instance.GetCharacter(playerData.CharacterId);

                if (playerData.HasAnsweredCorrectly)
                {
                    playerResultCard.transform.SetParent(CorrectPlayerResults.transform);
                    playerData.AddScore(playerData.ActionPoints);
                }
                else
                    playerResultCard.transform.SetParent(IncorrectPlayerResults.transform);

                //playerResultCard.AnswerResultImage.sprite = playerData.HasAnsweredCorrectly ? AnswerCorrectSprite : AnswerIncorrectSprite;
                playerResultCard.PlayerImage.sprite = character.Image;
                //playerResultCard.PlayerName.text = character.Name;
            }
        }

        // Sets player turn state.
        private void SetPlayerState()
        {
            TurnAmountText.text = turnAmountString + matchData.CurrentRound;
        }

        // Updates the players cards.
        public void UpdatePlayerCards()
        {
            Debug.Log("UpdatePlayerCards");

            foreach (PlayerCard playerCard in PlayerCards)
            {
                PlayerData playerData = matchData.GetPlayerData(playerCard.PlayerId);

                playerCard.UpdateScore(playerData.Score.ToString());
                playerCard.UpdateTurn(playerCard.PlayerId == matchData.CurrentTurnId);
            }
        }
    }
}