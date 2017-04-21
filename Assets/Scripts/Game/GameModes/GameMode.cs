using UnityEngine;
using Game.Networking;
using Managers;

namespace Game.GameModes
{
    // Abstract class containing all the game mode data
    public abstract class GameMode : MonoBehaviour
    {
        [HideInInspector]
        public MatchData MatchData;
        [HideInInspector]
        public MatchSceneManager MatchSceneManager;

        public string Name;

        [Header("Points")]
        protected int amountOfTurns;
        public int KillScore = 3;
        public int DieScore = -2;
        public int CorrectAnswerPoints = 3;
        public int IncorrectAnswerPoints = 1;

        [Header("Timings")]
        public float UpdateSpeed = 0.1f;
        public float IntroTime = 3.0f;
        public float InstructionTime = 3.0f;
        public float RegularFadeTime = 1.0f;
        public float BuildTime = 2.0f;
        public float QuestionPrepareTime = 2.0f;
        public float QuestionMaxTime = 10.0f;
        public float QuestionResultTime = 6.0f;
        public float TurnTimePerMove = 5.0f;
        public float WaitForQuestionTime = 1.0f;
        public float QuestionFadeTime = 1.5f;
    }
}