using System;
using System.Collections;
using System.Collections.Generic;
using Game.Networking;
using Managers;
using UnityEngine;
using UnityEngine.Networking;
using Miscellaneous;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Game
{
    public enum BuffType
    {
        Movement,
        Strength,
        Range
    }

    [Serializable]
    public struct BuffInfo
    {
        public BuffType Type;
        public int Power;
    }

    [RequireComponent(typeof(NetworkIdentity))]
    [RequireComponent(typeof(Animator))]
    public class Tile : NetworkBehaviour
    {
        public byte TileId = 0;
        public bool CanSpawnOn = true;
        public string TileName = "Default";
        public Vector2 SpawnAnimationTime = new Vector2(0, 2);
        public BuffInfo[] BuffCollection;
        public Renderer Renderer;
        public int RendererMaterialIndex;
        public Material[] StepMaterials;
        public GameObject onEnterParticle;
		public GameObject onScoreParticle;
		public GameObject pointPopup;

        public UnityAction OnTileHealed;

        private PlayerData ownerData;
        private Animator animator;
        private bool healed;

        public int SteppedOnAmount;
        public bool IsCorner; // is edge if false

        private void Awake()
        {
            SteppedOnAmount = 1;
            IsCorner = false;
            animator = GetComponent<Animator>();

            #region Fade animation curve initialization with SetFadeAnimationCurve (remove if this becomes unnecessary)
            //foreach (AnimationClip animationClip in animator.runtimeAnimatorController.animationClips)
            //{
            //Debug.Log("test_1: " + animationClip);
            //Debug.Log("test_2: " + animationClip.name);
            //SetFadeAnimationCurve(animationClip, "Cube");
            //SetFadeAnimationCurve(animationClip, "Cube/Buff");

            //new AnimationCurve().preWrapMode = WrapMode.Once;
            //}


            //SetFadeAnimationCurve(animator.runtimeAnimatorController.animationClips[0], "Cube");
            //SetFadeAnimationCurve(animator.runtimeAnimatorController.animationClips[0], "Cube/Buff");
            #endregion

            Invoke("PlaySpawnAnimation", Random.Range(SpawnAnimationTime.x, SpawnAnimationTime.y));
        }

        /*private void LateUpdate()
        {
            // When spawning this will set the color with the new fade alpha that has just been updated by the animator
            if (!animator.GetBool("IsDoneSpawning"))
            {
                SetFadeAlpha("Cube", animator.GetFloat("NewFadeAlpha"));
                SetFadeAlpha("Cube/Buff", animator.GetFloat("NewFadeAlpha"));
            }
            // If it was done spawning but did not completely finish the fading, then we simply set the final fade alpha
            else if (animator.GetFloat("NewFadeAlpha") < 0.996f) // 0.004 tolerance for the float imprecision
            {
                SetFadeAlpha("Cube", 1.0f);
                SetFadeAlpha("Cube/Buff", 1.0f);
            }
        }*/

        // This will set the new fade alpha value for the initial spawning
        private void SetFadeAlpha(string childGameObject, float newFadeAlpha)
        {
            // Finds the specified child game object and saves the material as a reference
            Material myMaterial = transform.FindChild(childGameObject).GetComponent<MeshRenderer>().material;

            myMaterial.color = new Color
            (
                myMaterial.color.r,
                myMaterial.color.g,
                myMaterial.color.b,
                newFadeAlpha
            );
        }

        #region SetFadeAnimationCurve (remove this if it will not become necessary in the near-future)
        // This will either add a new fade-in animation curve or replace an existing one for the specified child game object
        // It will not affect the original color as it includes the r,g,b values as non-changing animation curves
        // Those are merged with the changing alpha animation curve for the complete r,g,b,a color property of the animation clip
        // So it is independent of the existing values and solely made for fading, thus fully functional (otherwise r,g,b becomes black)
        /*private void SetFadeAnimationCurve(AnimationClip animationClip, string childGameObject)
        {
            Color originalColor = transform.FindChild(childGameObject).GetComponent<MeshRenderer>().material.color;

            // I assume that those values will not and do not need to be changed, however they can be exposed to the inspector for easy editing
            animationClip.SetCurve(childGameObject, typeof(MeshRenderer), "material._Color.r", AnimationCurve.Linear(0.0f, originalColor.r, 1.0f, originalColor.r));
            animationClip.SetCurve(childGameObject, typeof(MeshRenderer), "material._Color.g", AnimationCurve.Linear(0.0f, originalColor.g, 1.0f, originalColor.g));
            animationClip.SetCurve(childGameObject, typeof(MeshRenderer), "material._Color.b", AnimationCurve.Linear(0.0f, originalColor.b, 1.0f, originalColor.b));
            animationClip.SetCurve(childGameObject, typeof(MeshRenderer), "material._Color.a", AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f));

            //animationClip.wrapMode = WrapMode.ClampForever;
        }*/
        #endregion

        private void PlaySpawnAnimation()
        {
            animator.Play("GridCubeSpawn");
        }

        public void StopSpawnAnimation()
        {
            animator.SetBool("IsDoneSpawning", true);
        }

        // Will be called on the server when a player moves on this tile
        // This will handle setting and removing any existing buffs as well as the tile conquering and controlling functionality
        public void OnTileEnter(Player player)
        {
            // Handle buffs if there are any and not already taken by the same player
            if (BuffCollection.Length > 0 && !player.PlayerData.Equals(ownerData))
            {
                // Removing buffs of previous owner
                if (ownerData)
                    GiveBuffs(ownerData, false);

                // Setting buffs and color of the new owner
                GiveBuffs(player.PlayerData, true);
                StartCoroutine(LerpToColor(player.PlayerData.Character.EmissionColor));
            }
            SpawnOnEnterParticles();

            // Set the new tile owner if the tile isn't already locked
//            if (!player.PlayerData.Equals(ownerData) && SteppedOnAmount == 1)
//                ownerData = player.PlayerData;
//            // Lock the tile if the same owner stepped on this tile for the second time
//            else if (player.PlayerData.Equals(ownerData) && SteppedOnAmount == 1)
//                SteppedOnAmount++;

            // Set the new tile color based on the stepped-on amount, excluding any buffed tiles
            if (BuffCollection.Length <= 0 && !healed)
            {
                // Here we fade to the tile color partially for the first step
                if (SteppedOnAmount == 1)
                {
                    if(StepMaterials.Length > (SteppedOnAmount -1))
                        SetMaterial(SteppedOnAmount - 1);
					//switch (player.PlayerData.Character.EmissionColor.ToName())
     //               {
     //                   case "red":
     //                       StartCoroutine(LerpToColor(new Color(0.5f, 0.0f, 0.0f, 1.0f)));
     //                       break;
     //                   case "green":
     //                       StartCoroutine(LerpToColor(new Color(0.0f, 0.5f, 0.0f, 1.0f)));
     //                       break;
     //                   case "blue":
     //                       StartCoroutine(LerpToColor(new Color(0.0f, 0.0f, 0.5f, 1.0f)));
     //                       break;
     //                   case "yellow":
     //                       StartCoroutine(LerpToColor(new Color(0.5f, 0.5f, 0.0f, 1.0f)));
     //                       break;
     //                   case "purple":
     //                       StartCoroutine(LerpToColor(new Color(0.5f, 0.0f, 0.5f, 1.0f)));
     //                       break;
     //                   case "orange":
					//	    StartCoroutine(LerpToColor(new Color(0.5f, (64.0f / 255.0f), 0.0f, 1.0f)));
     //                       break;
     //               }
                    SteppedOnAmount++;
                }
                // Here we fade to the tile color completely for the second step
                // As well as rewarding the score points to the player for locking a single tile and/or completing a link loop
                // The lockable tiles from within the link loops of default or partial color will become the same color as the locked tiles of the loop
                else if (SteppedOnAmount == 2)
                {
                    SpawnOnHealParticles();
                    player.PlayerData.AddScore(1);
                    healed = true;

                    HealTile();

                    //ownerData.AddScore(1);
                    SetMaterial(SteppedOnAmount - 1);
                    //StartCoroutine(LerpToColor(player.PlayerData.Character.EmissionColor));

                    return; // TODO: finish this tile conquer & control functionality
                    #region NewCode
#pragma warning disable 0162 // Made it unreachable on purpose, so we don't care about this warning
                    // References for easy access
                    //byte[,] levelData = GameManager.Instance.MatchData.LevelData;
                    Tile[,] levelTiles = GameManager.Instance.MatchData.Level.LevelTiles;

                    List<Tile> lockableTiles = TileManager.GetLockableTiles();

                    Tile currentTarget = this; // We're using this locked tile as start point and neighbor targets will be set afterwards

                    // Points at the surrounding neighbor targets which will be processed one by one with the queue
                    List<Tile> neighborTargets = new List<Tile>
                    {
                        levelTiles[(int)currentTarget.transform.position.x - 1, (int)currentTarget.transform.position.y - 1], // Top-left tile
                        levelTiles[(int)currentTarget.transform.position.x - 1, (int)currentTarget.transform.position.y],     // Top tile
                        levelTiles[(int)currentTarget.transform.position.x - 1, (int)currentTarget.transform.position.y + 1], // Top-right tile
                        levelTiles[(int)currentTarget.transform.position.x, (int)currentTarget.transform.position.y + 1],     // Right tile
                        levelTiles[(int)currentTarget.transform.position.x + 1, (int)currentTarget.transform.position.y + 1], // Bottom-right tile
                        levelTiles[(int)currentTarget.transform.position.x + 1, (int)currentTarget.transform.position.y],     // Bottom tile
                        levelTiles[(int)currentTarget.transform.position.x + 1, (int)currentTarget.transform.position.y - 1], // Bottom-left tile
                        levelTiles[(int)currentTarget.transform.position.x, (int)currentTarget.transform.position.y - 1]      // Left tile
                    };
                    Queue<Tile> neighborTargetQueue = new Queue<Tile>(neighborTargets);

                    List<Tile> affectedTiles = new List<Tile>(); // Will contain tiles that can and will be locked

                    while (!lockableTiles.IsNullOrEmpty())
                    {
                        // We start searching from every enqueued neighbor target
                        // Until a complete path has been found
                        // or when there is nothing left to dequeue
                        // or when we've searched through all lockable tiles and no path had been found

                        if (neighborTargetQueue.Count != 0)
                        {
                            // Evaluate if we haven't found an empty spot
                            if (neighborTargetQueue.Peek())
                                currentTarget = neighborTargetQueue.Dequeue();
                            else
                                continue; // TODO: if it wasn't a tile from the outermost border, otherwise there is no complete link
                        }
                        else
                        {
                            // Check if we have any open spots on the outermost border? Although they do need to be saved somewhere first..

                            foreach (Tile affectedTile in affectedTiles)
                                SetLocked(affectedTile, player.PlayerData.Character.EmissionColor);

                            break; // out of this loop, because there are no more targets left
                            // there must be 1..* tile sets with and/or without border
                        }

                        lockableTiles.Remove(currentTarget);
                        affectedTiles.Add(currentTarget);

                        // If we immediately find a square shaped link loop around the current target as the center tile, we'll lock it and continue with the next target
						if (FindSquareLinkLoop(currentTarget, player.PlayerData.Character.EmissionColor)) // skipOtherAffectedTiles?
                        {
							SetLocked(currentTarget, player.PlayerData.Character.EmissionColor);
                            continue;
                        }

                        // Adds all the affected lockable tiles and updates the specified lockableTiles reference (as in removing the involved tiles from recursively continuing the search)
						affectedTiles.AddRange(FindAffectedTiles(currentTarget, ref lockableTiles, player.PlayerData.Character.EmissionColor));

                        //keep setting lockable surrounding neighbor targets (maybe with a separate queue or list)
                        //foreach neighbor target set above
                        //{
                        //    remove neighbor target from list of lockable tiles (save as linked tile)
                        //    if list of lockable tiles is not empty
                        //    {
                        //        // set next linked tile
                        //        
                        //    }
                        //}
                        //until there is nothing left to target
                        //and if the amount wasn't completely iterated over
                        //then there must either be 1..* tile sets with and/or without border
                    }
#pragma warning restore 0162
                    #endregion


                    #region oldCode
                    //Debug.Log("Tile(" + TileId + ") grid position: " + TileManager.GetTileGridPosition(TileId)); // TileId is always 1 ?? o_0
                    Debug.Log("Tile(" + TileName + ") grid position: " + TileManager.GetTileGridPosition(this));

                    // Tile linking here
                    //GameManager.Instance.MatchData.Level
                    //GameManager.Instance.MatchData.LevelData
                    //GameManager.Instance.MatchData.MatchSceneManager.Level

                    // TODO: Uncomment or delete the code below
                    //byte[,] levelData = GameManager.Instance.MatchData.LevelData;
                    //Tile[,] levelTiles = GameManager.Instance.MatchData.Level.LevelTiles;

                    ////int lockedTilesAmount = 0;
                    ////foreach (bool lockedTile in TileManager.GetLockedTiles())
                    ////    if (lockedTile)
                    ////        lockedTilesAmount++;

                    //Vector2? tileGridPosition;

                    //// Loop over each spot in the world
                    //for (int x = 0; x < levelData.GetLength(0); x++)
                    //{
                    //    for (int y = 0; y < levelData.GetLength(1); y++)
                    //    {
                    //        // Will be reused for all recursively found targets
                    //        Tile currentTarget = this;

                    //        // Points at the remaining targets which will be processed one by one
                    //        Queue<Tile> targetQueue = new Queue<Tile>();

                    //        // currentTarget is our start point and more targets will be added when and if those are found
                    //        //targetQueue.Enqueue(currentTarget);

                    //        // Our initial target counts as one and this becomes higher when and if more targets will be found
                    //        int remainingLockedTiles = 1;

                    //        // Everything that has already been checked or can't be checked at all, goes in here
                    //        List<Tile> ignoredTiles = new List<Tile>();

                    //        // Every locked tile must be marked as either edge or corner piece
                    //        while (remainingLockedTiles-- > 0)
                    //        {
                    //            #region oldCode
                    //            //Tile lockableTile;

                    //            // Start checking from our newly locked tile
                    //            // And keep checking while there are still lockable tiles
                    //            //while (lockableTile = FindLockedEdgeOrCorner(this))
                    //            //if (lockableTile = FindLockedEdgeOrCorner(this))
                    //            //{


                    //            //loop over elke level tile
                    //            // voor elke ongemarkeerde locked tile
                    //            //  als FindLockedEdgeOrCorner(this) een corner vind met een vrije tile
                    //            //   markeer this tile als corner
                    //            //   voor alle tiles(top,right,bottom,left) die deel uitmaken van een corner behalve de center tile zelf
                    //            //    check of het een edge of inversed corner tile is

                    //            //    tijdens het rechecken moet hij een al gecheckte terug vinden of niks terug vinden
                    //            //    om zo bij het einde te komen.. obviously


                    //            //}

                    //            //Tile currentTarget = this; // volgensmij moet dit 1 scope dieper
                    //            //List<Tile> ignoredTiles = new List<Tile>();
                    //            #endregion

                    //            // The initial target isn't enqueued, so we skip that one
                    //            if (currentTarget != this)
                    //                currentTarget = targetQueue.Dequeue();

                    //            // if current tile target is the same one
                    //            if (levelTiles[x, y] && levelTiles[x, y].Equals(currentTarget))
                    //                if ((tileGridPosition = TileManager.GetTileGridPosition(currentTarget)) != null)
                    //                {
                    //                    // Saving all the surrounding tiles that potentially contain a lockable tile
                    //                    Tile[] cornersWithLockableTile =
                    //                    {
                    //                        levelTiles[(int)tileGridPosition.Value.x - 1, (int)tileGridPosition.Value.y],      // Top tile
                    //                        levelTiles[(int)tileGridPosition.Value.x - 1, (int)tileGridPosition.Value.y + 1],  // Top-right tile (potentially lockable)
                    //                        levelTiles[(int)tileGridPosition.Value.x, (int)tileGridPosition.Value.y + 1],      // Right tile
                    //                        levelTiles[(int)tileGridPosition.Value.x + 1, (int)tileGridPosition.Value.y + 1],  // Bottom-right tile (potentially lockable)
                    //                        levelTiles[(int)tileGridPosition.Value.x + 1, (int)tileGridPosition.Value.y],      // Bottom tile
                    //                        levelTiles[(int)tileGridPosition.Value.x + 1, (int)tileGridPosition.Value.y - 1],  // Bottom-left tile (potentially lockable)
                    //                        levelTiles[(int)tileGridPosition.Value.x, (int)tileGridPosition.Value.y - 1],      // Left tile
                    //                        levelTiles[(int)tileGridPosition.Value.x - 1, (int)tileGridPosition.Value.y - 1]   // Top-left tile (potentially lockable)
                    //                    };
                    //                    //List<KeyValuePair<Tile, bool>> cornersWithLockableTile = new List<KeyValuePair<Tile, bool>>
                    //                    //{ 
                    //                    //    new KeyValuePair<Tile, bool>(levelTiles[(int)tileGridPosition.Value.x - 1, (int)tileGridPosition.Value.y], false),      // Top tile
                    //                    //    new KeyValuePair<Tile, bool>(levelTiles[(int)tileGridPosition.Value.x - 1, (int)tileGridPosition.Value.y + 1], false),  // Top-right tile
                    //                    //    new KeyValuePair<Tile, bool>(levelTiles[(int)tileGridPosition.Value.x, (int)tileGridPosition.Value.y + 1], false),      // Right tile
                    //                    //    new KeyValuePair<Tile, bool>(levelTiles[(int)tileGridPosition.Value.x + 1, (int)tileGridPosition.Value.y + 1], false),  // Bottom-right tile
                    //                    //    new KeyValuePair<Tile, bool>(levelTiles[(int)tileGridPosition.Value.x + 1, (int)tileGridPosition.Value.y], false),      // Bottom tile
                    //                    //    new KeyValuePair<Tile, bool>(levelTiles[(int)tileGridPosition.Value.x + 1, (int)tileGridPosition.Value.y - 1], false),  // Bottom-left tile
                    //                    //    new KeyValuePair<Tile, bool>(levelTiles[(int)tileGridPosition.Value.x, (int)tileGridPosition.Value.y - 1], false),      // Left tile
                    //                    //    new KeyValuePair<Tile, bool>(levelTiles[(int)tileGridPosition.Value.x - 1, (int)tileGridPosition.Value.y - 1], false)   // Top-left tile
                    //                    //};

                    //                    // Check until there can't be checked anymore
                    //                    for (int i = 0; i < (cornersWithLockableTile.Length / 2); i++)
                    //                    {
                    //                        // First iteration: [0], [1], [2] / [top], [top-right], [right]
                    //                        // Second iteration: [2], [3], [4] / [right], [bottom-right], [bottom]
                    //                        // Third iteration: [4], [5], [6] / [bottom], [bottom-left], [left]
                    //                        // Fourth iteration: [6], [7], [0] / [left], [top-left], [top]
                    //                        //
                    //                        // Here we're finding the corner pieces and modulo is just bringing the non-existing index back to the first and usable one
                    //                        if
                    //                        (
                    //                            // If they exist at all and not empty spots
                    //                            (
                    //                                cornersWithLockableTile[i == 0 ? 0 : i * 2].Key &&
                    //                                cornersWithLockableTile[((i == 0 ? 0 : i * 2) + 1) % cornersWithLockableTile.Length].Key &&
                    //                                cornersWithLockableTile[((i == 0 ? 0 : i * 2) + 2) % cornersWithLockableTile.Length].Key
                    //                            )
                    //                            && // If they do not contain ignored tiles from previous executions
                    //                            (
                    //                                !ignoredTiles.Contains(cornersWithLockableTile[i == 0 ? 0 : i * 2].Key) &&
                    //                                !ignoredTiles.Contains(cornersWithLockableTile[(i == 0 ? 0 : i * 2) + 2].Key)
                    //                            )
                    //                            && // If they are both locked
                    //                            (
                    //                                cornersWithLockableTile[i == 0 ? 0 : i * 2].Key.SteppedOnAmount == 2 &&
                    //                                cornersWithLockableTile[((i == 0 ? 0 : i * 2) + 1) % cornersWithLockableTile.Length].Key.SteppedOnAmount != 2 &&
                    //                                cornersWithLockableTile[((i == 0 ? 0 : i * 2) + 2) % cornersWithLockableTile.Length].Key.SteppedOnAmount == 2
                    //                            )
                    //                        )
                    //                        {
                    //                            //list[i].Key.IsCorner = true;
                    //                            //list[(i + 1) % list.Count].Key.IsCorner = true;
                    //                            ignoredTiles.Add(currentTarget); // previous target does not need to be checked again
                    //                            //cornersWithLockableTile[((i == 0 ? 0 : i * 2) + 1) % cornersWithLockableTile.Length].Key.IsCorner = true;
                    //                            ignoredTiles.Add(cornersWithLockableTile[((i == 0 ? 0 : i * 2) + 1) % cornersWithLockableTile.Length].Key); // free corner spot

                    //                            // Enqueue the next two corner targets
                    //                            targetQueue.Enqueue(cornersWithLockableTile[i == 0 ? 0 : i * 2].Key);
                    //                            targetQueue.Enqueue(cornersWithLockableTile[((i == 0 ? 0 : i * 2) + 2) % cornersWithLockableTile.Length].Key);

                    //                            //currentTarget = cornersWithLockableTile[i == 0 ? 0 : i * 2].Key;
                    //                            remainingLockedTiles += 2;

                    //                            //list[i].Value = true; // The corner contains a lockable tile
                    //                        }
                    //                    }

                    //                    #region oldCode
                    //                    // breaks out of the first loop when we're done, second loop should also break though (currently not that important)
                    //                    //break;

                    //                    /*Debug.Log(string.Format
                    //                (
                    //                    "TopLeft: {0}, Top: {1}, TopRight: {2}, Right: {3}, BottomRight {4}, Bottom: {5}, BottomLeft: {6} & Left: {7}.",
                    //                    tileGridPosition - Vector2.one,                 // Top-left
                    //                    tileGridPosition - new Vector2(1.0f, 0.0f),     // Top
                    //                    tileGridPosition + new Vector2(-1.0f, 1.0f),    // Top-right
                    //                    tileGridPosition + new Vector2(0.0f, 1.0f),     // Right
                    //                    tileGridPosition + Vector2.one,                 // Bottom-right
                    //                    tileGridPosition + new Vector2(1.0f, 0.0f),     // Bottom
                    //                    tileGridPosition + new Vector2(1.0f, -1.0f),    // Bottom-left
                    //                    tileGridPosition - new Vector2(0.0f, 1.0f)      // Left
                    //                ));*/

                    //                    // Saving all the surrounding tiles
                    //                    //Tile[] tileLoop =
                    //                    //{
                    //                    //    levelTiles[(int)tileGridPosition.Value.x - 1, (int)tileGridPosition.Value.y - 1], // Top-left tile
                    //                    //    levelTiles[(int)tileGridPosition.Value.x - 1, (int)tileGridPosition.Value.y],     // Top tile
                    //                    //    levelTiles[(int)tileGridPosition.Value.x - 1, (int)tileGridPosition.Value.y + 1], // Top-right tile
                    //                    //    levelTiles[(int)tileGridPosition.Value.x, (int)tileGridPosition.Value.y + 1],     // Right tile
                    //                    //    levelTiles[(int)tileGridPosition.Value.x + 1, (int)tileGridPosition.Value.y + 1], // Bottom-right tile
                    //                    //    levelTiles[(int)tileGridPosition.Value.x + 1, (int)tileGridPosition.Value.y],     // Bottom tile
                    //                    //    levelTiles[(int)tileGridPosition.Value.x + 1, (int)tileGridPosition.Value.y - 1], // Bottom-left tile
                    //                    //    levelTiles[(int)tileGridPosition.Value.x, (int)tileGridPosition.Value.y - 1]      // Left tile
                    //                    //};

                    //                    // Find out whether the whole loop exists out of only locked tiles
                    //                    //bool complete8LinkLoop = true;
                    //                    //foreach (Tile tile in tileLoop)
                    //                    //    if (tile) // Could be an empty spot I think
                    //                    //        complete8LinkLoop &= (tile.steppedOnAmount == 2);

                    //                    //Debug.Log("Complete 8 link loop = " + complete8LinkLoop);

                    //                    //int lockedTilesAmount = 0;
                    //                    //foreach (bool lockedTile in TileManager.GetLockedTiles())
                    //                    //    if (lockedTile)
                    //                    //        lockedTilesAmount++;

                    //                    //Debug.Log("Locked tiles: " + lockedTilesAmount);
                    //                    #endregion
                    //                }

                    //            //currentTarget = ;
                    //        }
                    //        //byte tileId = levelData[x, y];
                    //        //Tile tile = TileManager.GetTile(tileId);
                    //    }
                    //}
                    #endregion
                }
            }
        }

        public void SpawnOnEnterParticles()
        {
            if (onEnterParticle != null)
            {
                GameObject particle1 =
                    (GameObject)GameObject.Instantiate(onEnterParticle,
                                                       new Vector3(transform.position.x, transform.position.y + 1,
                                                                   transform.position.z), Quaternion.identity);
                if (isServer)
                    RpcSpawnOnEnterParticles();
            }
        }

        [ClientRpc]
        public void RpcSpawnOnEnterParticles()
        {
            SpawnOnEnterParticles();
        }

        public void SpawnOnHealParticles()
        {
            if (onScoreParticle != null)
            {
                GameObject particle2 =
                    (GameObject)GameObject.Instantiate(onScoreParticle,
                                                       new Vector3(transform.position.x, transform.position.y + 1,
                                                                   transform.position.z), Quaternion.identity);
				GameObject points =
					(GameObject)GameObject.Instantiate(pointPopup,
						new Vector3(transform.position.x, transform.position.y + 2,
							transform.position.z), Quaternion.identity);
				points.GetComponent<RisingText>().Setup (3.0f, 2f);

                if (isServer)
                    RpcSpawnOnHealParticles();
            }
        }

        [ClientRpc]
        public void RpcSpawnOnHealParticles()
        {
            SpawnOnHealParticles();
        }

        // Validates the lockable center tile and then finds the square shaped link around it
        private bool FindSquareLinkLoop(Tile lockableCenterTile, Color squareLoopColor)
        {
            // We return false immediately if the specified lockableCenterTile is in fact locked
            if (lockableCenterTile.SteppedOnAmount == 2)
                return false;

            // Reference for easy access
            Tile[,] levelTiles = GameManager.Instance.MatchData.Level.LevelTiles;

            // Saving all the neighbor tiles surrounding the specified center tile
            List<Tile> centerNeighborTiles = new List<Tile>
            {
                levelTiles[(int)lockableCenterTile.transform.position.x - 1,  (int)lockableCenterTile.transform.position.y - 1],    // Top-left tile
                levelTiles[(int)lockableCenterTile.transform.position.x - 1,  (int)lockableCenterTile.transform.position.y],        // Top tile
                levelTiles[(int)lockableCenterTile.transform.position.x - 1,  (int)lockableCenterTile.transform.position.y + 1],    // Top-right tile
                levelTiles[(int)lockableCenterTile.transform.position.x,      (int)lockableCenterTile.transform.position.y + 1],    // Right tile
                levelTiles[(int)lockableCenterTile.transform.position.x + 1,  (int)lockableCenterTile.transform.position.y + 1],    // Bottom-right tile
                levelTiles[(int)lockableCenterTile.transform.position.x + 1,  (int)lockableCenterTile.transform.position.y],        // Bottom tile
                levelTiles[(int)lockableCenterTile.transform.position.x + 1,  (int)lockableCenterTile.transform.position.y - 1],    // Bottom-left tile
                levelTiles[(int)lockableCenterTile.transform.position.x,      (int)lockableCenterTile.transform.position.y - 1]     // Left tile
            };

            // Find out whether the whole loop exists out of only locked tiles with the specified color
            bool complete8LinkLoop = true;
            foreach (Tile tile in centerNeighborTiles)
                if (tile) // Could be an empty spot I think
                    complete8LinkLoop &= (tile.SteppedOnAmount == 2) && tile.Renderer.material.color.Equals(squareLoopColor);
                else
                    complete8LinkLoop = false;

            return complete8LinkLoop;
        }

        //only search for lockable tiles in a + pattern, but include the other tiles & empty spots of the x pattern
        // Recursive method that finds and returns all the affected tiles including the startPoint
        private List<Tile> FindAffectedTiles(Tile lockableStartTile, ref List<Tile> lockableTiles, Color lastLockedTileColor)//, List<Tile> ignoredTiles)
        {
            // Reference for easy access
            Tile[,] levelTiles = GameManager.Instance.MatchData.Level.LevelTiles;

            Vector2? startPointGridPosition = TileManager.GetTileGridPosition(lockableStartTile);
            List<Tile> affectedTiles = new List<Tile> { lockableStartTile };
            lockableTiles.Remove(lockableStartTile);

            // Saving all the surrounding tiles of startPoint
            List<Tile> startPointNeighborTiles = new List<Tile>
            {
                levelTiles[(int)startPointGridPosition.Value.x - 1, (int)startPointGridPosition.Value.y - 1],   // Top-left tile
                levelTiles[(int)startPointGridPosition.Value.x - 1, (int)startPointGridPosition.Value.y],       // Top tile
                levelTiles[(int)startPointGridPosition.Value.x - 1, (int)startPointGridPosition.Value.y + 1],   // Top-right tile
                levelTiles[(int)startPointGridPosition.Value.x,     (int)startPointGridPosition.Value.y + 1],   // Right tile
                levelTiles[(int)startPointGridPosition.Value.x + 1, (int)startPointGridPosition.Value.y + 1],   // Bottom-right tile
                levelTiles[(int)startPointGridPosition.Value.x + 1, (int)startPointGridPosition.Value.y],       // Bottom tile
                levelTiles[(int)startPointGridPosition.Value.x + 1, (int)startPointGridPosition.Value.y - 1],   // Bottom-left tile
                levelTiles[(int)startPointGridPosition.Value.x,     (int)startPointGridPosition.Value.y - 1]    // Left tile
            };
            Queue<Tile> startPointNeighborTileQueue = new Queue<Tile>(startPointNeighborTiles);

            //if (any empty spots, then there is an opening in the border.. so no link)
            // although we do need to exclude the empty spots inside, as those don't matter
            // only the empty spots on the outermost border matters 
            // including the null values (empty spots)

            // Find out whether the whole loop exists out of only locked tiles of the same color as
            bool complete8LinkLoop = true;
            foreach (Tile tile in startPointNeighborTileQueue)
                if (tile) // Could be an empty spot I think
                    complete8LinkLoop &= (tile.SteppedOnAmount == 2) && tile.Renderer.material.color.Equals(lastLockedTileColor);
                else
                    // not sure how to handle empty spots inside
                    complete8LinkLoop = false;

            //Debug.Log("Complete 8 link loop = " + complete8LinkLoop);

            if (complete8LinkLoop)
                return affectedTiles;

            Tile neighborTargetSuccessor = startPointNeighborTileQueue.Dequeue();
            Vector2? successorGridPosition = TileManager.GetTileGridPosition(neighborTargetSuccessor);
            affectedTiles.Add(neighborTargetSuccessor);
            lockableTiles.Remove(neighborTargetSuccessor);

            // Setting the initial neighbor tiles of our neighbor target successor
            List<Tile> successorNeighborTiles = new List<Tile>
            {
                levelTiles[(int)successorGridPosition.Value.x - 1,  (int)successorGridPosition.Value.y - 1],    // Top-left tile
                levelTiles[(int)successorGridPosition.Value.x - 1,  (int)successorGridPosition.Value.y],        // Top tile
                levelTiles[(int)successorGridPosition.Value.x - 1,  (int)successorGridPosition.Value.y + 1],    // Top-right tile
                levelTiles[(int)successorGridPosition.Value.x,      (int)successorGridPosition.Value.y + 1],    // Right tile
                levelTiles[(int)successorGridPosition.Value.x + 1,  (int)successorGridPosition.Value.y + 1],    // Bottom-right tile
                levelTiles[(int)successorGridPosition.Value.x + 1,  (int)successorGridPosition.Value.y],        // Bottom tile
                levelTiles[(int)successorGridPosition.Value.x + 1,  (int)successorGridPosition.Value.y - 1],    // Bottom-left tile
                levelTiles[(int)successorGridPosition.Value.x,      (int)successorGridPosition.Value.y - 1]     // Left tile
            };
            Stack<Tile> successorNeighborTileStack = new Stack<Tile>(successorNeighborTiles);

            while (neighborTargetSuccessor) // is not null
            {
                neighborTargetSuccessor = successorNeighborTileStack.Pop();

                // If neighbor target successor is not an empty spot and neither locked
                if (neighborTargetSuccessor && neighborTargetSuccessor.SteppedOnAmount != 2)
                {
                    successorNeighborTileStack.Push(levelTiles[(int)successorGridPosition.Value.x - 1, (int)successorGridPosition.Value.y - 1]);    // Top-left tile
                    successorNeighborTileStack.Push(levelTiles[(int)successorGridPosition.Value.x - 1, (int)successorGridPosition.Value.y]);        // Top tile
                    successorNeighborTileStack.Push(levelTiles[(int)successorGridPosition.Value.x - 1, (int)successorGridPosition.Value.y + 1]);    // Top-right tile
                    successorNeighborTileStack.Push(levelTiles[(int)successorGridPosition.Value.x, (int)successorGridPosition.Value.y + 1]);        // Right tile
                    successorNeighborTileStack.Push(levelTiles[(int)successorGridPosition.Value.x + 1, (int)successorGridPosition.Value.y + 1]);    // Bottom-right tile
                    successorNeighborTileStack.Push(levelTiles[(int)successorGridPosition.Value.x + 1, (int)successorGridPosition.Value.y]);        // Bottom tile
                    successorNeighborTileStack.Push(levelTiles[(int)successorGridPosition.Value.x + 1, (int)successorGridPosition.Value.y - 1]);    // Bottom-left tile
                    successorNeighborTileStack.Push(levelTiles[(int)successorGridPosition.Value.x, (int)successorGridPosition.Value.y - 1]);        // Left tile

                    continue;
                }

                // Set the next startPoint successor for the following iteration
                neighborTargetSuccessor = startPointNeighborTileQueue.Dequeue();

                //if (lockableTiles is not empty) // is this condition even necessary
                //{
                // A path through all lockableTiles was not found
                // Both the empty spots & locked tiles must be saved as well (the surrounding neighbors), we just follow the lockable tiles
                // Then if there are any empty spots, it means that there is an opening in the border.. so no link
                // And all the locked tiles from the border have to be the same color (of the startPoint tile)
                //}
            }

            return affectedTiles;
        }

        // Made for locking other tiles within a link loop
        private void SetLocked(Tile lockableTile, Color fadeTargetColor)
        {
            lockableTile.SteppedOnAmount = 2;
            ownerData.AddScore(1);
            StartCoroutine(lockableTile.LerpToColor(fadeTargetColor));
        }

        // Finds out if the specified center tile is part of either a corner piece or edge piece
        // Returns lockable tile if found, otherwise null.
        private Tile FindLockedEdgeOrCorner(Tile centerTile)
        {
            return null;
        }

        // Lerp this tile to the specified target color
        private IEnumerator LerpToColor(Color targetColor, float totalLerpTime = 0.5f)
        {
            float currentLerpTime = 0; // Initial start value

            // Saving a copy of the current color because it will change while lerping
            Color startColor = Renderer.material.color;

            while (!Mathf.Approximately(currentLerpTime, totalLerpTime))
            {
                // Increment timer once per frame
                currentLerpTime += Time.deltaTime;

                // Clamping to totalLerpTime if higher
                if (currentLerpTime > totalLerpTime)
                    currentLerpTime = totalLerpTime;

                // The actual lerping
                float currentProgress = currentLerpTime / totalLerpTime;
                SetColor(Color.Lerp(startColor, targetColor, currentProgress));

                yield return new WaitForEndOfFrame();
            }
        }

        // Remove this tile's buff of a player
        public void RemoveBuffs(Player player)
        {
            if (BuffCollection.Length <= 0)
                return;

            PlayerData playerData = player.PlayerData;

            if (ownerData == null || !playerData.Equals(ownerData))
                return;

            SetColor(Color.white);
            GiveBuffs(ownerData, false);

            ownerData = null;
        }

        // Change tile's color
        // If called on a server it will automatically do this on all clients
        public void SetColor(Color color)
        {
            Renderer.materials[RendererMaterialIndex].color = color;

            if (isServer)
                RpcSetColor(color);
        }

        [ClientRpc]
        private void RpcSetColor(Color color)
        {
            SetColor(color);
        }

        public void SetMaterial(int index)
        {
            if (StepMaterials.Length > index)
            {
                Material[] mats = Renderer.materials;
                mats[RendererMaterialIndex] = StepMaterials[index];
                Renderer.materials = mats;

                if (isServer)
                    RpcSetMaterial(index);
            }
        }

        [ClientRpc]
        public void RpcSetMaterial(int index)
        {
            SetMaterial(index);
        }

        public void HealTile()
        {
            if (OnTileHealed != null)
            {
                OnTileHealed.Invoke();

                if (isServer)
                    RpcHealTile();
            }
        }

        [ClientRpc]
        public void RpcHealTile()
        {
            HealTile();
        }


        // Give all tile's buffs to a player
        public void GiveBuffs(PlayerData playerData, bool addBuffPower)
        {
            foreach (BuffInfo buff in BuffCollection)
                playerData.ChangeBuff(buff.Type, addBuffPower ? buff.Power : -buff.Power);
        }
    }
}