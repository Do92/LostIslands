using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Game;

namespace Editor
{
    [CustomEditor(typeof(Level))]
    public class LevelEditor : UnityEditor.Editor
    {
        // Show all spawned players in the level
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Level script = (Level)target;

            GUILayout.Space(20);

            GUILayout.BeginVertical();
            GUILayout.Label("Players (" + script.GetPlayers().Count + "):");

            // Display each player
            foreach (KeyValuePair<int, Player> keyValuePlayer in script.GetPlayers())
            {
                GUILayout.Label("========================================");

                GUILayout.Label("Player ID: " + keyValuePlayer.Key);

                GUILayout.Label("Local ID: " + keyValuePlayer.Value.PlayerId);

                GUILayout.Label("========================================");
            }

            GUILayout.EndVertical();
        }
    }
}