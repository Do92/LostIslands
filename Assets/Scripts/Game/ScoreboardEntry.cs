using UnityEngine;
using UnityEngine.UI;
using Game.Networking;
using Managers;
using CharacterInfo = Game.Networking.CharacterInfo;

namespace Game
{
    /// <summary>
    /// This class contains the required references for the scoreboard entries.
    /// </summary>
    public class ScoreboardEntry : MonoBehaviour
    {
        public Image EyeFadeImage;
        public Text PlayerName;
        public Text PlayerScore;
    }
}