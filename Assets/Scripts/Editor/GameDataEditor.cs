using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Game.Networking;

namespace Editor
{
    [CustomEditor(typeof(MatchData))]
    public class GameDataEditor : UnityEditor.Editor
    {
        // Display all the players in the dictionary
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MatchData script = (MatchData)target;

            GUILayout.Space(20);

            GUILayout.BeginVertical();
            GUILayout.Label("Players (" + script.GetAllPlayerData().Count + "):");

            // Show specific data for each player
            foreach (KeyValuePair<int, PlayerData> keyValuePlayerData in script.GetAllPlayerData())
            {
                GUILayout.Label("========================================");

                GUILayout.Label("Player ID: " + keyValuePlayerData.Key);

                GUILayout.Label("Local ID: " + keyValuePlayerData.Value.PlayerId);
                GUILayout.Label("Name: " + keyValuePlayerData.Value.name);
                GUILayout.Label("Score: " + keyValuePlayerData.Value.Score);

                GUILayout.Label("========================================");
            }

            GUILayout.EndVertical();
        }
    }
}