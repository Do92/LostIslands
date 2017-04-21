using UnityEngine;
using UnityEngine.UI;
using Managers;
using QuestionItem;
using Miscellaneous;

namespace Game.Networking
{
    /// <summary>
    /// This class is used to manages all things based around the client UI.
    /// </summary>
    public class ClientUI : MonoBehaviour
    {
        [Header("Panels")]
        public RectTransform LoadPanel;
        public RectTransform IntroPanel;
        public RectTransform QuestionPanel;
        public RectTransform QuestionResultPanel;
        public RectTransform GamePanel;
        public RectTransform GameTurnPanel;
        public RectTransform OutroPanel;
        public RectTransform ScorePanel;

        [Header("Question Panel")]
        public Text QuestionText;
        public Text[] AnswerTexts;
        public Button[] AnswerButtons;
        public Color SelectedButtonColor;
        public Image TimeLeftBar;

        [Header("Question Result Panel Server")]
        public Text ResultQuestionText;
        public Text ResultCorrectAnswer;

        [Header("Question Result Panel Client")]
        public Text ActionPointsRewardText;
        private string actionPointsRewardString;
        public Text QuestionResultText;
        public string QuestionCorrectString;
        public string QuestionIncorrectString;

        [Header("Game Panel")]
        public Image CharacterImage;
        public Text ActionPointsText;
        public Button[] MoveOptionButtons;
        public Sprite[] MoveOptionSprites;
        public Text[] BuffMovementTexts;
        public Text[] BuffStrenghtTexts;
        public Text[] BuffRangeTexts;

        [Header("Turn Panel")]
        public Text TimeRemaining;
        private bool isMyTurn;
        private bool isMoving;

        [Header("Scoreboard")]
        private RankManager rankManager;

        public GameObject PersistentMenuItems;

        // References
        private MatchData matchData;
        private ClientResponseInfo currentResponse;

        // Reference for easy access and a little resource efficiency
        private Animation questionResultAnimation;

        // Initialization.
        private void Awake()
        {
            questionResultAnimation = QuestionResultPanel.GetComponent<Animation>();

            HideAll();

            LoadPanel.gameObject.SetActive(true);

            matchData = GameManager.Instance.MatchData;
            actionPointsRewardString = ActionPointsRewardText.text;

            rankManager = GetComponent<RankManager>();
        }

        // Initialization.
        private void Start()
        {
            CharacterImage.sprite = matchData.PlayerData.Character.Image;
        }

        // Gets called every frame.
        private void Update()
        {
            if (matchData.MatchState.Equals(MatchStateType.QuestionPreparation))
                TimeLeftBar.fillAmount = matchData.QuestionTime / matchData.CurrentGameMode.QuestionMaxTime;

            if (isMyTurn && !isMoving)
                TimeRemaining.text = ((int)matchData.TurnTime).ToString();
        }

        // Shows the panel according to the game state.
        public void ShowPanel(MatchStateType matchState)
        {
            HideAll();

            switch (matchState)
            {
                case MatchStateType.Loading:
                    LoadPanel.gameObject.SetActive(true);
                    break;
                case MatchStateType.Intro:
                    IntroPanel.gameObject.SetActive(true);
                    break;
                case MatchStateType.QuestionPreparation:
                    QuestionPanel.gameObject.SetActive(true);
                    PrepareQuestion();
                    break;
                case MatchStateType.QuestionProcessing:
                    SendAnswer();
                    break;
                case MatchStateType.QuestionResult:
                    questionResultAnimation.Play(PlayMode.StopAll);
                    QuestionResultState();
                    break;
                case MatchStateType.Player:
                    // Couldn't use this state to toggle the question background panel off!
                    break;
                case MatchStateType.Outro:
                    OutroPanel.gameObject.SetActive(true);
                    break;
                case MatchStateType.GameOverMenu:
                    rankManager.SetRanking();
                    PersistentMenuItems.SetActive(true);
                    ScorePanel.gameObject.SetActive(true);
                    break;
#if UNITY_EDITOR
                // This is mainly for clarity while debugging the client (the hierarchy would be a mess otherwise)
                case MatchStateType.LevelGeneration:
                    Tile[] levelTiles = FindObjectsOfType<Tile>();
                    foreach (Tile tile in levelTiles)
                        tile.transform.parent = matchData.Level.transform;
                    break;
#endif
            }
        }

        // Disables all panels except the question result panel (is always enabled for animation purposes)
        private void HideAll()
        {
            RectTransform[] panelsToHide =
            {
                LoadPanel,
                IntroPanel,
                QuestionPanel,
                GamePanel,
                GameTurnPanel,
                ScorePanel
            };

            foreach (RectTransform panel in panelsToHide)
                panel.gameObject.SetActive(false);
        }

        // Questions
        private void PrepareQuestion()
        {
            currentResponse = new ClientResponseInfo()
            {
                QuestionType = matchData.CurrentQuestion.QuestionType,
                IsQuestionAnswered = false
            };

            // Sets up the public client question text
            QuestionText.text = matchData.CurrentQuestion.QuestionText;

            // Sets up the public client answer text for the multiple choice question
            if (matchData.CurrentQuestion.QuestionType.Equals(QuestionType.MultipleChoiceItem))
            {
                int index = 0;
                foreach (AnswerInfo answerOption in matchData.CurrentQuestion.AnswerOptions)
                    AnswerTexts[index++].text = answerOption.AnswerText;
            }
        }

        // State for when the question is done.
        private void QuestionResultState()
        {
            ActionPointsText.text = matchData.PlayerData.ActionPoints.ToString();

            // Resetting the answer buttons for the next round
            foreach (Button answerButton in AnswerButtons)
                answerButton.GetComponent<Button>().interactable = true;

            ActionPointsRewardText.text = matchData.PlayerData.ActionPoints + " " + actionPointsRewardString;

            if (!currentResponse.IsQuestionAnswered)
                QuestionResultText.text = QuestionIncorrectString;
            else if (QuestionManager.ValidateAnswerKeyData(matchData.CurrentQuestion, currentResponse, false))
                QuestionResultText.text = QuestionCorrectString;
            else
                QuestionResultText.text = QuestionIncorrectString;
        }

        // When the user clicks an answer button.
        public void AnswerButtonPressed(int answer)
        {
            AnswerData answerData;

            switch (matchData.CurrentQuestion.QuestionType)
            {
                case QuestionType.MultipleChoiceItem:
                    answerData = (AnswerData)(1 << answer);

                    currentResponse = new ClientResponseInfo(currentResponse.QuestionType, answerData, true);

                    // Making the pressed button non-interactable so that it can't be pressed again, obviously (changes color as indication)
                    for (int i = 0; i < AnswerButtons.Length; i++)
                        AnswerButtons[i].interactable = !i.Equals(answer);
                    break;
                case QuestionType.MultipleResponseItem:
                    if (!currentResponse.IsQuestionAnswered)
                        answerData = (AnswerData)(1 << answer);
                    else
                        answerData = currentResponse.AnswerKeyData | (AnswerData)(1 << answer);

                    currentResponse = new ClientResponseInfo(currentResponse.QuestionType, answerData, true);

                    // Making the pressed button non-interactable so that it can't be pressed again, obviously (changes color as indication)
                    //for (int i = 0; i < AnswerButtons.Length; i++)
                    //AnswerButtons[i].interactable = !i.Equals(answer);
                    break;
            }
        }

        // Sends the users answer to the server.
        private void SendAnswer()
        {
            matchData.PlayerData.CmdSubmitAnswer(currentResponse);
        }

        // Updates the display of the user buffs.
        public void UpdateBuffs()
        {
            foreach (Text buffMovementText in BuffMovementTexts)
                buffMovementText.text = matchData.PlayerData.MovementBuffs.ToString();
            foreach (Text buffRangeText in BuffRangeTexts)
                buffRangeText.text = matchData.PlayerData.RangeBuffs.ToString();
            foreach (Text buffStrengthText in BuffStrenghtTexts)
                buffStrengthText.text = matchData.PlayerData.StrengthBuffs.ToString();
        }

        // Movement
        public void MovePlayer(int direction)
        {
            isMoving = true;
            matchData.PlayerData.CmdDoMove((DirectionType)direction);
            DisableMovement();
        }

        // Sets the new turn.
        public void SetTurn(bool isMyTurn)
        {
            this.isMyTurn = isMyTurn;

            HideAll();
            DisableMovement();

            GamePanel.gameObject.SetActive(!isMyTurn);
            GameTurnPanel.gameObject.SetActive(isMyTurn);
        }

        // Shows the turn for the player and shows the possible moves the player can make.
        public void SetTurn(int actionPoints, MoveOptionType[] moveOptions)
        {
            ActionPointsText.text = actionPoints.ToString();

            foreach (DirectionType direction in EnumUtilities.GetValues<DirectionType>())
            {
                Button moveOptionButton = MoveOptionButtons[(int)direction];
                MoveOptionType moveOption = moveOptions[(int)direction];

                moveOptionButton.interactable = moveOption != MoveOptionType.Disabled;
                moveOptionButton.GetComponent<Image>().sprite = MoveOptionSprites[(int)moveOption];
            }

            isMoving = false;
        }

        // Disables the movement control for the player. 
        private void DisableMovement()
        {
            foreach (Button moveOptionButton in MoveOptionButtons)
            {
                //moveButtonsText[(int)direction].text = moveText[(int)MoveType.Disabled];
                moveOptionButton.interactable = false;
            }
        }
    }
}