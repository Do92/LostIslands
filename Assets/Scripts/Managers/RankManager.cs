using System.Collections.Generic;
using Game;
using Game.Networking;
using UnityEngine;

namespace Managers
{
    public class RankManager : MonoBehaviour
    {
        // Maximum = 6 items, default index = first place, last index = last place
        public ScoreboardEntry[] ScoreboardEntries;

        // The color of the winner will be set for this
        public SkinnedMeshRenderer RotatingRobot;

        private List<PlayerData> rankedPlayerData = new List<PlayerData>();
        private MatchData matchData; // Reference for easy access and a little resource efficiency

        private void Awake()
        {
            matchData = GameManager.Instance.MatchData;
        }

        private void CalculateScores()
        {
            List<PlayerData> allPlayerData = new List<PlayerData>(matchData.GetAllPlayerData().Values);
            rankedPlayerData.Clear();

            for (int i = 0; i < allPlayerData.Count; i++)
            {
                PlayerData bestCalculatedScore = allPlayerData[i]; // Initial best

                foreach (PlayerData comparisonPlayerdata in allPlayerData)
                    if (bestCalculatedScore.Score < comparisonPlayerdata.Score)
                        bestCalculatedScore = comparisonPlayerdata; // Eventually the final best

                rankedPlayerData.Add(bestCalculatedScore);
            }

			RotatingRobot.material.color = rankedPlayerData[0].Character.EmissionColor;
        }

        public void SetRanking()
        {
            CalculateScores();

            // Set ranking for all active players
            for (int i = 0; i < rankedPlayerData.Count; i++)
            {
                ScoreboardEntries[i].EyeFadeImage.color = rankedPlayerData[i].Character.EyeFadeColor;
                ScoreboardEntries[i].PlayerName.text = rankedPlayerData[i].Character.Name;
                ScoreboardEntries[i].PlayerScore.text = rankedPlayerData[i].Score.ToString();

                ScoreboardEntries[i].EyeFadeImage.gameObject.SetActive(true);
                ScoreboardEntries[i].PlayerName.gameObject.SetActive(true);
                ScoreboardEntries[i].PlayerScore.gameObject.SetActive(true);
            }
        }
    }
}