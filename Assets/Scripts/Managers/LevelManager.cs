using System;
using System.Collections.Generic;
using UnityEngine;
using Miscellaneous;
using Random = UnityEngine.Random;

namespace Managers
{
    public class LevelManager : Singleton<LevelManager>
    {
        private string[] rowSplitter = { "\r\n", "\n" };
        private string[] columnSplitter = { " " };

        private static List<byte[,]> generatedLevels = new List<byte[,]>();

        // Will load all levels from for /Resources/Levels/
        // Levels must be in a text file format
        private void Awake()
        {
            TextAsset[] levels = Resources.LoadAll<TextAsset>("Levels");

            if (levels.Length <= 0)
                Debug.LogError("No levels were loaded!");

            // Loop over all found levels
            foreach (TextAsset level in levels)
            {
                byte[,] data;
                string[] rows = level.text.Split(rowSplitter, StringSplitOptions.None);

                int height = rows.Length;
                int width = rows[0].Split(columnSplitter, StringSplitOptions.None).Length;

                data = new byte[height, width];

                for (int y = 0; y < height; y++)
                {
                    string[] row = rows[y].Split(columnSplitter, StringSplitOptions.None);

                    for (int x = 0; x < width; x++)
                    {
                        byte id = 0;

                        byte.TryParse(row[x], out id);

                        data[y, x] = id;
                    }
                }

                generatedLevels.Add(data);
            }
        }

        public static byte[,] GetRandomLevel()
        {
            return generatedLevels[Random.Range(0, generatedLevels.Count)];
        }
    }
}