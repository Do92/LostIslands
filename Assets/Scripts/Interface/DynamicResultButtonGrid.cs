using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Interface
{
    // This makes the grid layout group dynamic
    [RequireComponent(typeof(GridLayoutGroup))]
    public class DynamicResultButtonGrid : MonoBehaviour
    {
        public int Column, Row;

        public Button ActiveButton;

        public Text ChoiceInformation;

        public GameObject FeedbackButtonPrefab;

        private Button[] buttonsToSetActive;

        // Use this for initialization
        private void Start()
        {
            for (int i = 0; i < GameManager.Instance.MatchData.Descriptions.Count(); i++)
                Instantiate(FeedbackButtonPrefab, Vector3.zero, Quaternion.identity);

            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            GridLayoutGroup gridLayoutGroup = gameObject.GetComponent<GridLayoutGroup>();

            // The editable cell size should be avoided in the editor (as we're changing it below)
            gridLayoutGroup.cellSize = new Vector2(rectTransform.rect.width / Column, rectTransform.rect.height / Row) - gridLayoutGroup.spacing;

            List<Button> initialButtonsToSetActive = GetComponentsInChildren<Button>().ToList();

            // Here we're preventing the SetActiveButton method to change its interactability
            initialButtonsToSetActive.Remove(ActiveButton);

            buttonsToSetActive = initialButtonsToSetActive.ToArray();
        }

        public void SetActiveButton(int indexNumber)
        {
            // Setting all the buttons which are not the specified index number to interactable
            for (int i = 0; i < buttonsToSetActive.Count(); i++)
                buttonsToSetActive[i].interactable = i != indexNumber;

            // Setting the description component
            ChoiceInformation.text = GameManager.Instance.MatchData.Descriptions[indexNumber];

            // Setting the active choice text child component
            ActiveButton.GetComponentInChildren<Text>().text = (indexNumber + 1).ToString();
        }
    }
}