using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Game;
using Game.Networking;
using Miscellaneous;

namespace Managers
{
    /// <summary>
    /// This class manages the camera that is directed at the player.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraManager : MonoBehaviour
    {
        private Vector3 startPosition;

        public float LerpSpeed = 2;
        public Vector3 OffsetPosition;

        // Initialization
        private void Start()
        {
            // Here we remember the initial position for the rollback code in Update()
            startPosition = transform.position;
        }

        // Gets called every frame update.
        private void Update()
        {
            MatchData matchData = FindObjectOfType<MatchData>();

            if (matchData.CurrentTurnId > -1 && matchData.isClient)
            {
                // Zooms in on a player that has the game turn and is a client (otherwise the main server screen would also zoom in)
                Player playerWithTurn = matchData.Level.GetPlayer(matchData.CurrentTurnId);
                Vector3 targetPosition = playerWithTurn.Position.ToVector3(0.0f);
                targetPosition += OffsetPosition;

                transform.position = Vector3.Lerp(transform.position, targetPosition, LerpSpeed * Time.deltaTime);
            }
            else
                // Zooms out back to the initial camera position (this happens on CurrentTurnId value reset at the very end of the game loop)
                transform.position = Vector3.Lerp(transform.position, startPosition, LerpSpeed * Time.deltaTime);
        }
    }
}