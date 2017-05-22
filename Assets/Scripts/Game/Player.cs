using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Game.Networking;
using Managers;
using Miscellaneous;

namespace Game
{
	public enum DirectionType
	{
		Up,
		Right,
		Down,
		Left
	}

	public enum ExtendedDirectionType
	{
		TopLeft,
		Top,
		TopRight,
		Right,
		BottomRight,
		Bottom,
		BottomLeft,
		Left
	}

	public class Player : NetworkBehaviour
	{
		// Structure data for an animation, will contain positional data and a move type in a direction
		private struct MoveAnimationInfo
		{
			public DirectionType Direction;
			public Vector2 Position;
			public MoveActionType Action;

			public MoveAnimationInfo(DirectionType direction, Vector2 position, MoveActionType action)
			{
				Direction = direction;
				Position = position;
				Action = action;
			}
		}

		//public bool Debug = false;

		[HideInInspector]
		public int PlayerId { get; private set; }

		public PlayerData PlayerData { get; private set; }

		public float LevelBuildTime = 3.0f;
		public float MoveSpeed = 1.0f;
		public float RotateSpeed = 1.0f;
		public float FallSpeed = 1.0f;
		public float SpawnHeight = 10.0f;
		public float DieDepth = -10.0f;
		public Renderer[] Renderers;
		public SpriteRenderer Indicator;
		public GameObject turnIndicator;        
		public GameObject pushParticle;

		private MatchData matchData;
		private float positionY;
		private DirectionType currentDirection = DirectionType.Up;
		private Queue<MoveAnimationInfo> moveAnimations = new Queue<MoveAnimationInfo>();
		private Animator characterAvatar;

		private int lastPusher = -1;

		public Vector2 Position { get; set; }

		private void Awake()
		{
			positionY = transform.position.y;

			StartCoroutine(MoveAnimator());

			characterAvatar = gameObject.GetComponentInChildren<Animator>();
		}

		// Will register the player in the active level
		// If called on a server it will automatically do this on all clients
		public void Register(int playerId)
		{
			if (isServer)
				RpcRegister(playerId);

			PlayerId = playerId;

			matchData = GameManager.Instance.MatchData;
			PlayerData = matchData.GetPlayerData(playerId);

			matchData.MatchSceneManager.Level.RegisterPlayer(this);
		}

		[ClientRpc]
		private void RpcRegister(int playerId)
		{
			Register(playerId);
		}

		// Set the player's color
		// If called on a server it will automatically do this on all clients
		public void SetColor(Color mainColor)
		{
			foreach (Renderer renderer in Renderers)
			{
				//renderer.material.color = mainColor; // Sets the main color

				//renderer.material.SetFloat("_Metallic", 1.0f); // Makes it more visible and shiny like a robot

				renderer.materials[0].SetColor("_Color", mainColor);

			}

			Indicator.color = mainColor;

			if (isServer)
				RpcSetColor(mainColor);
		}

		[ClientRpc]
		private void RpcSetColor(Color mainColor)
		{
			SetColor(mainColor);
		}

		public void SetPushedBy(int playerId)
		{
			lastPusher = playerId;
		}

		// Movement & Animation 
		//----------------------------------------------------------
		//ALL INACTIVE!!!
		//----------------------------------------------------------
		// Teleport with first spawn animation
		public void DoInitialMove(Vector2 position)
		{
			Move(DirectionType.Right, position, MoveActionType.Initial);
		}

		// Teleport with respawn animation
		public void Spawn(Vector2 position)
		{
			Move(DirectionType.Right, position, MoveActionType.Spawn);
		}

		// Die animation
		public void Die(Vector2 position)
		{
			if (isServer)
			{
				PlayerData.AddScore(matchData.CurrentGameMode.DieScore);
				matchData.GetPlayerData(lastPusher).AddScore(matchData.CurrentGameMode.KillScore);
			}

			Move(DirectionType.Up, position, MoveActionType.Die);
		}

		// Teleport, no specific animation
		public void Teleport(Vector2 newPosition)
		{
			Move(DirectionType.Up, newPosition, MoveActionType.Teleport);
		}

		// Push animation
		public void Push(DirectionType direction)
		{
			//characterAvatar.SetBool ("bumpCheck", true);
			//GameObject particle = (GameObject)GameObject.Instantiate(pushParticle, new Vector3 (transform.position.x, (transform.position.y + 2.0f), transform.position.z), transform.rotation);
			//Move(direction, Position, MoveActionType.Pushing);
			//characterAvatar.SetBool ("bumpCheck", false);

		}

		// Move animation
		public void Move(DirectionType newDirection)
		{
			Vector3 newPosition = Position.GetRelative(newDirection);

			Move(newDirection, newPosition, MoveActionType.Move);
		}

		// Pushed animation
		public void Pushed(DirectionType direction, int playerId, int distance)
		{
			for (int i = 0; i < distance; i++)
			{
				Vector3 position = Position.GetRelative(direction);
				Move(direction, position, MoveActionType.Pushed);

				// Will apply a death check of the pushed player (for a valid push result position)
				if (!matchData.MatchSceneManager.Level.IsValidPositionForPlayer(this, position))
				{
					Die(position);
					Spawn(matchData.MatchSceneManager.Level.GetSpawnPoint());

					matchData.MatchSceneManager.Level.OnDeath(this);
					break;
				}
			}
		}

		// End animations and movement

		// Internal move method, will require a few parameters but will handle each possible move
		// If called on a server it will automatically do this on all clients
		private void Move(DirectionType newDirection, Vector2 newPosition, MoveActionType moveAction)
		{
			//if (Debug)
				//UnityEngine.Debug.Log("Move: " + newDirection + ":" + newPosition + ":" + moveAction);

			// Prepare a rotational move behaviour
			if (moveAction != MoveActionType.Pushed && moveAction != MoveActionType.Die && moveAction != MoveActionType.Spawn)
				moveAnimations.Enqueue(new MoveAnimationInfo(newDirection, newPosition, MoveActionType.Rotate));

			// Prepare a regular move behaviour
			moveAnimations.Enqueue(new MoveAnimationInfo(newDirection, newPosition, moveAction));

			Position = newPosition;

			// Regular buff and tile color fading behaviour along with a remote procedure call to all clients
			if (isServer)
			{
				Tile tile = matchData.MatchSceneManager.Level.GetTile(newPosition);

				if (tile != null && moveAction != MoveActionType.Pushing)
					tile.OnTileEnter(this);

				RpcMove(newDirection, newPosition, moveAction);
			}
		}

		// Animation enumerator, will automatically play animations when added to the moves list
		//----------------------------------------------------------------------------
		//NOTHING OF THE FOLLOWING IS EVER CALLED IN THE CURRENT VERSION OF THE GAME refer to Level.cs
		//---------------------------------------------------------------------------
		private IEnumerator MoveAnimator()
		{
			while (true)
			{
				while (moveAnimations.Count > 0)
				{
					MoveAnimationInfo moveAnimation = moveAnimations.Dequeue();
					float amount = 0.0f;

					Vector3 startPosition;
					Vector3 targetPosition;

					switch (moveAnimation.Action)
					{
					case MoveActionType.Move:
					case MoveActionType.Pushing:
					case MoveActionType.Pushed:						

						startPosition = transform.position;
						targetPosition = moveAnimation.Position.ToVector3(positionY);

						while (transform.position != targetPosition)
						{
							amount += MoveSpeed * Time.deltaTime;
							transform.position = Vector3.Lerp(startPosition, targetPosition, amount);

							yield return null;
						}

						break;
					
					case MoveActionType.Rotate:

						if (currentDirection == moveAnimation.Direction)
							continue;

						int rotateDistance = currentDirection.GetShortestRotateDistance(moveAnimation.Direction);
						float rotateAngle = moveAnimation.Direction.GetAngle();
						Quaternion startRotation = transform.rotation;
						Quaternion targetRotation = Quaternion.AngleAxis(rotateAngle, Vector3.up);

						while ((int)transform.eulerAngles.y != (int)rotateAngle)
						{
							amount += (RotateSpeed / rotateDistance) * Time.deltaTime;
							transform.rotation = Quaternion.Lerp(startRotation, targetRotation, amount);

							yield return null;
						}

						currentDirection = moveAnimation.Direction;

						break;
					case MoveActionType.Initial:
					case MoveActionType.Spawn:
					case MoveActionType.Die:
						targetPosition = moveAnimation.Position.ToVector3(positionY);
						startPosition = targetPosition;

						startPosition.y = (moveAnimation.Action == MoveActionType.Die) ? DieDepth : SpawnHeight;
						transform.position = startPosition;

						if (moveAnimation.Action == MoveActionType.Initial)
						{
							yield return new WaitForSeconds(LevelBuildTime);
							GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
						}

						while (transform.position.y > targetPosition.y)
						{
							amount += MoveSpeed * Time.deltaTime;
							transform.position = Vector3.Lerp(startPosition, targetPosition, amount);

							yield return null;
						}

						break;
					case MoveActionType.Teleport:
						transform.position = moveAnimation.Position.ToVector3(positionY);

						break;
					}
					yield return null;
				}
				yield return null;
			}
		}

		[ClientRpc]
		private void RpcMove(DirectionType direction, Vector2 position, MoveActionType moveAction)
		{
			Move(direction, position, moveAction);
		}
	}
}