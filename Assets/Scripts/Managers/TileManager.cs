using UnityEngine;
using Game;
using Miscellaneous;
using System.Collections.Generic;

namespace Managers
{
    public class TileManager : Singleton<TileManager>
    {
        // Array with max size of 255, if a tileId is not present it will, 
        // instead of an error, just return null
        private static Tile[] tileCollection = new Tile[byte.MaxValue];

        // References for easy access gives getter call error, while local ones don't
        //private static byte[,] levelData = GameManager.Instance.MatchData.LevelData;
        //private static Tile[,] levelTiles = GameManager.Instance.MatchData.Level.LevelTiles;

        // Will load all the grid cubes from for /Resources/GridCubes/
        private void Awake()
        {
            Tile[] gridCubes = Resources.LoadAll<Tile>("GridCubes");

            if (gridCubes.Length <= 0)
                Debug.LogError("No grid cubes have been loaded!");

            foreach (Tile gridCube in gridCubes)
                tileCollection[gridCube.TileId] = gridCube;
        }

        // Return a tile by a given tile id, will return null if tile is not present
        public static Tile GetTile(byte tileId)
        {
            return tileCollection[tileId];
        }

        // Return a position in the grid by a given tile id, will return null if tile is not found (not used)
        public static Vector2? GetTileGridPosition(byte tileId)
        {
            //GameManager.Instance.MatchData.Level
            //GameManager.Instance.MatchData.LevelData
            //GameManager.Instance.MatchData.MatchSceneManager.Level

            byte[,] levelData = GameManager.Instance.MatchData.LevelData;

            //Tile[,] levelTiles = new Tile[levelData.GetLength(0), levelData.GetLength(1)];
            //Tile[,] levelTiles = GameManager.Instance.MatchData.Level.LevelTiles;

            // Iterate over each spot in the world to find the position in the grid
            for (int x = 0; x < levelData.GetLength(0); x++)
            {
                for (int y = 0; y < levelData.GetLength(1); y++)
                {
                    // Check if we found the specified tile
                    if (levelData[x, y].Equals(tileId))
                        return new Vector2(x, y);

                    //byte tileId = levelData[x, y];
                    //Tile tile = TileManager.GetTile(tileId);
                }
            }

            return null;
        }

        // Return a position in the grid by a given tile, will return null if tile is not found
        public static Vector2? GetTileGridPosition(Tile tile)
        {
            // References for easy access
            byte[,] levelData = GameManager.Instance.MatchData.LevelData;
            Tile[,] levelTiles = GameManager.Instance.MatchData.Level.LevelTiles;

            // Iterate over each spot in the world to find the position in the grid
            for (int x = 0; x < levelData.GetLength(0); x++)
            {
                for (int y = 0; y < levelData.GetLength(1); y++)
                {
                    // Check if we found the specified tiles
                    if (levelTiles[x, y] && levelTiles[x, y].Equals(tile))
                        return new Vector2(x, y);
                }
            }

            return null;
        }

        // Returns boolean array, true = locked tile, false = everything else
        public static bool[,] GetTilesStates() // was previously called GetLockedTiles
        {
            // References for easy access
            byte[,] levelData = GameManager.Instance.MatchData.LevelData;
            Tile[,] levelTiles = GameManager.Instance.MatchData.Level.LevelTiles;

            bool[,] lockedTiles = new bool[levelTiles.GetLength(0), levelTiles.GetLength(1)];

            // Iterate over each spot in the world to find all locked tiles
            for (int x = 0; x < levelData.GetLength(0); x++)
                for (int y = 0; y < levelData.GetLength(1); y++)
                    if (levelTiles[x, y])
                        lockedTiles[x, y] = levelTiles[x, y].SteppedOnAmount == 2;

            return lockedTiles;
        }

        public static List<Tile> GetLockedTiles()
        {
            // References for easy access
            byte[,] levelData = GameManager.Instance.MatchData.LevelData;
            Tile[,] levelTiles = GameManager.Instance.MatchData.Level.LevelTiles;

            List<Tile> lockedTiles = new List<Tile>();

            // Iterate over each spot in the world to find all locked tiles
            for (int x = 0; x < levelData.GetLength(0); x++)
                for (int y = 0; y < levelData.GetLength(1); y++)
                    if (levelTiles[x, y] && levelTiles[x, y].SteppedOnAmount == 2)
                        lockedTiles.Add(levelTiles[x, y]);

            return lockedTiles;
        }

        public static List<Tile> GetLockableTiles()
        {
            // References for easy access
            byte[,] levelData = GameManager.Instance.MatchData.LevelData;
            Tile[,] levelTiles = GameManager.Instance.MatchData.Level.LevelTiles;

            List<Tile> lockableTiles = new List<Tile>();

            // Iterate over each spot in the world to find all locked tiles
            for (int x = 0; x < levelData.GetLength(0); x++)
                for (int y = 0; y < levelData.GetLength(1); y++)
                    if (levelTiles[x, y] && levelTiles[x, y].SteppedOnAmount != 2)
                        lockableTiles.Add(levelTiles[x, y]);

            return lockableTiles;
        }
    }
}