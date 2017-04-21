using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Game.Networking;
using Managers;
using Miscellaneous;

namespace Game
{
    public class Level : MonoBehaviour
    {
        public bool Debug = false;

        public Tile[,] LevelTiles;
        private Dictionary<int, Player> players = new Dictionary<int, Player>();

        private MatchData matchData;

        // Build the physical level with the prefab data from the TileManager
        public void BuildLevel()
        {
            matchData = GameManager.Instance.MatchData;

            byte[,] levelData = matchData.LevelData;

            LevelTiles = new Tile[levelData.GetLength(0), levelData.GetLength(1)];

            // Loop over each spot in the world
            for (int x = 0; x < levelData.GetLength(0); x++)
            {
                for (int y = 0; y < levelData.GetLength(1); y++)
                {
                    byte tileId = levelData[x, y];
                    Tile tile = TileManager.GetTile(tileId);

                    if (tile == null)
                        continue;

                    GameObject tileObject = Instantiate(tile.gameObject, new Vector3(x, tile.transform.position.y, y), Quaternion.identity) as GameObject;
                    tileObject.transform.parent = transform;

                    NetworkServer.Spawn(tileObject);

                    LevelTiles[x, y] = tileObject.GetComponent<Tile>();
                }
            }
        }

        // TODO Optional functionality: An average spawn distance from the other players
        // Get an valid spawn point within the world that's not occupied by another player and is spawnable on
        public Vector2 GetSpawnPoint()
        {
            int maxX = LevelTiles.GetLength(0);
            int maxY = LevelTiles.GetLength(1);

            Vector2 position = Vector2.zero;

            do
            {
                position.x = Random.Range(0, maxX);
                position.y = Random.Range(0, maxY);
                bool continueLooping = false;

                foreach (Player player in players.Values)
                {
                    if (player && player.Position == position)
                    {
                        continueLooping = true;
                        break;
                    }
                }
                if (continueLooping) continue; // Skips to next iteration

                Tile tile = GetTile(position);
                if (tile != null && tile.CanSpawnOn)
                    return position;
            } while (true);
        }

        // Register a player
        public void RegisterPlayer(Player player)
        {
            if (Debug)
                UnityEngine.Debug.Log("Registering player for \"" + player.PlayerId + "\"");

            players.Add(player.PlayerId, player);
        }

        // Spawn all physical players, this gets the player prefab and spawns it with bound PlayerData
        // Also set the player's characteristics such as image and color
        public void SpawnPlayers()
        {
            foreach (PlayerData playerData in matchData.GetAllPlayerData().Values)
            {
                GameObject playerObject = Instantiate(GameManager.Instance.PlayerPrefab.gameObject);

                NetworkServer.Spawn(playerObject);
                Player player = playerObject.GetComponent<Player>();

                if (Debug)
                    UnityEngine.Debug.Log("Spawning player for \"" + playerData.PlayerId + "\"");

                player.Register(playerData.PlayerId);
                player.SetColor(playerData.Character.MainColor, playerData.Character.EmissionColor);
                player.DoInitialMove(GetSpawnPoint());
            }
        }

        // Move the selected Player by playerId in a certain direction
        // Will automatically attempt to push a other player and apply the given buffs
        public void MovePlayer(int playerId, DirectionType direction)
        {
            Player player = players[playerId];
            PlayerData playerData = player.PlayerData;

            Vector2 newPlayerPosition = player.Position.GetRelative(direction);

            bool hasPushedOpponent;
            bool hasPushed = false;
            int range = 0;

            List<Player> pushedPlayers = new List<Player>();

            // Range push check
            do
            {
                hasPushedOpponent = false;

                foreach (Player otherPlayer in players.Values)
                {
                    if (otherPlayer.Position == newPlayerPosition)
                    {
                        pushedPlayers.Add(otherPlayer);
                        newPlayerPosition = newPlayerPosition.GetRelative(direction);

                        hasPushed = true;
                        hasPushedOpponent = true;
                        break;
                    }
                }

                // Update the newPlayerPosition by another moveStep if there is more available range (this repeats itself until there is no more)
                if (!hasPushed && range < playerData.RangeBuffs)
                {
                    hasPushedOpponent = true;
                    newPlayerPosition = newPlayerPosition.GetRelative(direction);
                    range += 1;
                }
            } while (hasPushedOpponent);

            for (int i = pushedPlayers.Count - 1; i >= 0; i--)
            {
                pushedPlayers[i].SetPushedBy(playerId);
                pushedPlayers[i].Pushed(direction, playerId, playerData.StrengthBuffs + 1);
            }

            if (hasPushed)
                player.Push(direction);
            else
                player.Move(direction);
        }

        // Must be called upon death of a player, will release any claimed buffs from the field
        public void OnDeath(Player player)
        {
            for (int x = 0; x < LevelTiles.GetLength(0); x++)
                for (int y = 0; y < LevelTiles.GetLength(1); y++)
                    if (LevelTiles[x, y] != null)
                        LevelTiles[x, y].RemoveBuffs(player);
        }

        // Returns all available moves for the given playerId
        // Array will be sorted by Direction enum index
        public MoveOptionType[] GetAvailableMoveOptions(int playerId)
        {
            MoveOptionType[] moveOptions = new MoveOptionType[4];
            Player specifiedPlayer = players[playerId];
            PlayerData playerData = specifiedPlayer.PlayerData;

            moveOptions[(int)DirectionType.Up] = specifiedPlayer.Position.y + 1 >= LevelTiles.GetLength(1)
                ? MoveOptionType.Disabled
                : LevelTiles[(int)specifiedPlayer.Position.x, (int)specifiedPlayer.Position.y + 1] == null
                    ? MoveOptionType.Disabled
                    : MoveOptionType.Move;
            moveOptions[(int)DirectionType.Right] = specifiedPlayer.Position.x + 1 >= LevelTiles.GetLength(0)
                ? MoveOptionType.Disabled
                : LevelTiles[(int)specifiedPlayer.Position.x + 1, (int)specifiedPlayer.Position.y] == null
                    ? MoveOptionType.Disabled
                    : MoveOptionType.Move;
            moveOptions[(int)DirectionType.Down] = specifiedPlayer.Position.y - 1 < 0
                ? MoveOptionType.Disabled
                : LevelTiles[(int)specifiedPlayer.Position.x, (int)specifiedPlayer.Position.y - 1] == null
                    ? MoveOptionType.Disabled
                    : MoveOptionType.Move;
            moveOptions[(int)DirectionType.Left] = specifiedPlayer.Position.x - 1 < 0
                ? MoveOptionType.Disabled
                : LevelTiles[(int)specifiedPlayer.Position.x - 1, (int)specifiedPlayer.Position.y] == null
                    ? MoveOptionType.Disabled
                    : MoveOptionType.Move;

            foreach (Player otherPlayer in players.Values)
            {
                if (otherPlayer.Equals(specifiedPlayer))
                    continue;

                foreach (DirectionType direction in EnumUtilities.GetValues<DirectionType>())
                {
                    for (int i = 0; i <= playerData.RangeBuffs; i++)
                    {
                        if (specifiedPlayer.Position.GetRelative(direction, i + 1) == otherPlayer.Position)
                        {
                            moveOptions[(int)direction] = MoveOptionType.Push;
                            break;
                        }
                    }
                }
            }

            if (Debug)
            {
                UnityEngine.Debug.Log("Moves for: " + playerId);
                foreach (DirectionType direction in EnumUtilities.GetValues<DirectionType>())
                    UnityEngine.Debug.Log(direction + ": " + moveOptions[(int)direction]);
            }

            return moveOptions;
        }

        // Gets a specific tile, will return null if out of range
        public Tile GetTile(Vector2 position)
        {
            if (position.x < 0 || position.x >= LevelTiles.GetLength(0) || position.y < 0 || position.y >= LevelTiles.GetLength(1))
                return null;

            return LevelTiles[(int)position.x, (int)position.y];
        }

        // Check if specified player can stand on a specific spot
        public bool IsValidPositionForPlayer(Player specifiedPlayer, Vector2 position)
        {
            foreach (Player player in players.Values)
                if (!player.Equals(specifiedPlayer) && player.Position == position)
                    return false;

            return GetTile(position) != null;
        }

        public Player GetPlayer(int playerId)
        {
            return players[playerId];
        }

        public Dictionary<int, Player> GetPlayers()
        {
            return players;
        }
    }
}