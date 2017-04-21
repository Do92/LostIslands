using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Networking;
using Managers;

namespace Game.GameModes
{
    public class GameModeNormal : GameMode
    {
        private IEnumerator Start()
        {
            MatchData.ChangeMatchState(MatchStateType.Loading);

            amountOfTurns = QuestionManager.Instance.QuestionList.Count;

            bool allPlayersLoaded = false;

            // Check if all players are loaded
            while (!allPlayersLoaded)
            {
                allPlayersLoaded = true;

                foreach (PlayerData playerData in MatchData.GetAllPlayerData().Values)
                {
                    if (!playerData.IsLoaded)
                    {
                        allPlayersLoaded = false;
                        break;
                    }
                }

                yield return new WaitForSeconds(UpdateSpeed);
            }

            byte[,] levelData = LevelManager.GetRandomLevel();
            MatchData.SetLevel(levelData);

            MatchData.ChangeMatchState(MatchStateType.Intro);

            yield return StartCoroutine(MatchData.MatchSceneManager.PlayIntroCutscene());

            MatchData.ChangeMatchState(MatchStateType.FadeFromBlack);
            yield return new WaitForSeconds(RegularFadeTime);

            MatchData.ChangeMatchState(MatchStateType.LevelGeneration);

            // Wait for building of the level
            yield return new WaitForSeconds(BuildTime);

            // Making the descriptions available to the client before we shuffle the question list below
            // Although it sends 10753 bytes with "Sportvragen.xml" while the maximum is 1400,
            // so I've changed the channel from Reliable Sequenced to Reliable Fragmented to allow this behaviour for now
            MatchData.SendQuestionDescriptions();

            // Preparing the question list
            QuestionManager.Instance.ShuffleQuestionList();

            while (true)
            {
                MatchData.CurrentRound++;

                // Clear all answer flags
                foreach (PlayerData playerData in MatchData.GetAllPlayerData().Values)
                    playerData.HasAnswered = false;

                yield return new WaitForSeconds(QuestionPrepareTime);

                // Sync the question
                MatchData.CurrentQuestion = QuestionManager.Instance.GetNextQuestion(MatchData.CurrentRound);
                yield return new WaitForSeconds(WaitForQuestionTime);

                MatchData.ChangeMatchState(MatchStateType.QuestionFadeIn);
                yield return new WaitForSeconds(QuestionFadeTime);

                MatchData.ChangeMatchState(MatchStateType.QuestionPreparation);

                // Start question timer
                MatchData.QuestionTime = QuestionMaxTime;
                while (MatchData.QuestionTime > 0)
                {
                    MatchData.QuestionTime -= UpdateSpeed;
                    yield return new WaitForSeconds(UpdateSpeed);
                }

                MatchData.ChangeMatchState(MatchStateType.QuestionProcessing);

                // Wait for all answers or timeout if takes too long
                bool allAnswersReceived = false;

                while (!allAnswersReceived)
                {
                    allAnswersReceived = true;

                    foreach (PlayerData playerData in MatchData.GetAllPlayerData().Values)
                    {
                        if (!playerData.HasAnswered)
                        {
                            allAnswersReceived = false;
                            break;
                        }
                    }
                    yield return new WaitForSeconds(UpdateSpeed);
                }

                yield return new WaitForEndOfFrame();

                MatchData.ChangeMatchState(MatchStateType.QuestionResult);
                yield return new WaitForSeconds(QuestionResultTime);

                MatchData.ChangeMatchState(MatchStateType.QuestionFadeOut);
                yield return new WaitForSeconds(QuestionFadeTime);

                MatchData.ChangeMatchState(MatchStateType.Player);

                // Loop over to give each player their turn
                foreach (KeyValuePair<int, PlayerData> keyValuePlayerData in MatchData.GetAllPlayerData())
                {
                    MatchData.CurrentTurnId = keyValuePlayerData.Key;
                    MatchData.TurnTime = TurnTimePerMove * keyValuePlayerData.Value.ActionPoints;

                    yield return new WaitForEndOfFrame();

                    MatchSceneManager.SetTurn(keyValuePlayerData.Key);
                    MatchData.SendTurn(keyValuePlayerData.Key);

                    // Wait until turn times out or player uses all action points
                    while (MatchData.TurnTime > 0 && keyValuePlayerData.Value.ActionPoints > 0)
                    {
                        MatchData.TurnTime -= UpdateSpeed;
                        yield return new WaitForSeconds(UpdateSpeed);
                    }
                }

                MatchData.CurrentTurnId = -1;

                // End of game
                if (MatchData.CurrentRound >= amountOfTurns)
                {
                    MatchData.ChangeMatchState(MatchStateType.FadeToBlack);
                    yield return new WaitForSeconds(RegularFadeTime);

                    MatchData.ChangeMatchState(MatchStateType.Outro);

                    yield return StartCoroutine(MatchData.MatchSceneManager.PlayOutroCutscene());

                    MatchData.ChangeMatchState(MatchStateType.GameOverMenu);

                    yield break;
                }
            }
        }
    }

    // 34 63 36 66 37 32 36 35 36 65 37 61 36 66 32 30 37 37 36 31 37 33 32 30 36 38 36 35 37 32 36 35 35 65 35 65  2xH
}