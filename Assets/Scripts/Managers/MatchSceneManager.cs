using System.Collections;
using UnityEngine;
using Game;
using Game.Networking;
using Miscellaneous;

namespace Managers
{
    // This is a API for anything related to the scene, clientUI and serverUI should never be referred directly
    public class MatchSceneManager : MonoBehaviour
    {
        public Level Level;

        // Must be assigned in the editor
        [SerializeField]
        private ServerUI serverUI;
        [SerializeField]
        private ClientUI clientUI;

        private bool isServer;
        private MatchData matchData;

        private MoviePlayer introMoviePlayer;
        private MoviePlayer outroMoviePlayer;

        private void Awake()
        {
            matchData = GameManager.Instance.MatchData;
        }

        public void SetServer(bool isServer)
        {
            this.isServer = isServer;

            serverUI.gameObject.SetActive(isServer);
            clientUI.gameObject.SetActive(!isServer);
        }

        public void SetMatchState(MatchStateType matchState)
        {
            if (isServer)
                serverUI.ShowPanel(matchState);
            else
                clientUI.ShowPanel(matchState);

            // Used to fade from and to
            Color questionBackgroundfadeColor = new Color(0, 0, 0, 185.0f / 255.0f);

            switch (matchState)
            {
                case MatchStateType.Intro:
                    introMoviePlayer = isServer ? serverUI.IntroPanel.GetComponent<MoviePlayer>() : clientUI.IntroPanel.GetComponent<MoviePlayer>();
                    break;
                case MatchStateType.FadeFromBlack:
                    // Will make for a nice camera effect
                    CameraFade.StartAlphaFade(Color.black, true, 3.0f);
                    break;
                case MatchStateType.QuestionFadeIn:
                    CameraFade.StartAlphaFade(questionBackgroundfadeColor, false, 3.5f);
                    break;
                case MatchStateType.QuestionFadeOut:
                    CameraFade.StartAlphaFade(questionBackgroundfadeColor, true, 4.0f);
                    break;
                case MatchStateType.LevelGeneration:
                    if (isServer)
                    {
                        Level.BuildLevel();
                        Level.SpawnPlayers();
                    }
                    break;
                case MatchStateType.FadeToBlack:
                    CameraFade.StartAlphaFade(Color.black, false, 3.0f);
                    break;
                case MatchStateType.Outro:
                    outroMoviePlayer = isServer ? serverUI.OutroPanel.GetComponent<MoviePlayer>() : clientUI.OutroPanel.GetComponent<MoviePlayer>();
                    break;
            }
        }

        public IEnumerator PlayIntroCutscene()
        {
            introMoviePlayer.StartCoroutine(introMoviePlayer.PlayCutscene());

            while (introMoviePlayer.IsPlayingMovie)
                yield return new WaitForEndOfFrame();
        }

        public IEnumerator PlayOutroCutscene()
        {
            outroMoviePlayer.StartCoroutine(outroMoviePlayer.PlayCutscene());

            while (outroMoviePlayer.IsPlayingMovie)
                yield return new WaitForEndOfFrame();
        }

        public void SetTurn(int playerId)
        {
            if (!isServer)
                clientUI.SetTurn(matchData.PlayerId == playerId);
            else
                serverUI.UpdatePlayerCards();
        }

        public void UpdateBuffs()
        {
            if (!isServer)
                clientUI.UpdateBuffs();
        }

        public void UpdatePlayerCards()
        {
            if (isServer)
                serverUI.UpdatePlayerCards();
        }

        public void SetTurn(int actionPoints, MoveOptionType[] moveOptions)
        {
            if (!isServer)
                clientUI.SetTurn(actionPoints, moveOptions);
        }

        public void MovePlayer(int playerId, DirectionType direction)
        {
            Level.MovePlayer(playerId, direction);
        }

        public MoveOptionType[] GetAvailableMoveOptions(PlayerData playerData)
        {
            return Level.GetAvailableMoveOptions(playerData.PlayerId);
        }
    }
}