using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using CharacterInfo = Game.Networking.CharacterInfo;

namespace Interface
{
    public class PlayerCard : MonoBehaviour
    {
        public Sprite TurnImage;
        public Sprite NormalImage;

        public Image PlayerImage;
        public Image CardImage;
        public Text PlayerName;

        [HideInInspector]
        public int PlayerId;

        public Text ScoreText;
        private string scoreString;

        public void Instantiate(CharacterInfo character)
        {
            PlayerImage.sprite = character.Image;
            PlayerName.text = character.Name;
        }

        public void UpdateScore(string score)
        {
            if (scoreString == null)
                scoreString = ScoreText.text;

            ScoreText.text = scoreString + score;
            //StartCoroutine(ChangeScoreFontSize()); // not done yet
        }

        // This is to make the font size first big then back to small, as indication
        //private IEnumerator ChangeScoreFontSize()
        //{
        //    while (ScoreText.fontSize < 30)
        //    {
        //        ScoreText.fontSize++;
        //        yield return new WaitForEndOfFrame();
        //    }
        //    while (ScoreText.fontSize > 25)
        //    {
        //        ScoreText.fontSize--;
        //        yield return new WaitForEndOfFrame();
        //    }
        //}

        public void UpdateTurn(bool isMyTurn)
        {
            CardImage.sprite = isMyTurn ? TurnImage : NormalImage;
        }
    }
}