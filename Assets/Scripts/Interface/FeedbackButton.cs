using UnityEngine;
using UnityEngine.UI;

namespace Interface
{
    public class FeedbackButton : MonoBehaviour
    {
        private DynamicResultButtonGrid dynamicResultButtonGrid;
        private static int nextButtonId;
        private int myButtonId;

        // Use this for initialization
        private void Start()
        {
            // Setting this id and incrementing for the next feedback button
            myButtonId = nextButtonId++;

            GetComponentInChildren<Text>().text = (myButtonId + 1).ToString();

            dynamicResultButtonGrid = FindObjectOfType(typeof(DynamicResultButtonGrid)) as DynamicResultButtonGrid;

            transform.SetParent(dynamicResultButtonGrid.transform, false);

            GetComponent<Button>().onClick.AddListener(ActivateThisButton);
        }

        private void ActivateThisButton()
        {
            dynamicResultButtonGrid.SetActiveButton(myButtonId);
        }
    }
}