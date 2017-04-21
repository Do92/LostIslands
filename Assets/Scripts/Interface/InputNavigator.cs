using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Interface
{
    // This will affect both the attached game object with all the children
    public class InputNavigator : MonoBehaviour
    {
        private EventSystem eventSystem;

        private void Start()
        {
            eventSystem = EventSystem.current;
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                Selectable nextSelectable =
                    eventSystem.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

                // If next navigation element can be found
                if (nextSelectable != null)
                {

                    InputField inputField = nextSelectable.GetComponent<InputField>();
                    if (inputField != null)
                        inputField.OnPointerClick(new PointerEventData(eventSystem));
                    // If it's an input field, also set the text caret

                    eventSystem.SetSelectedGameObject(nextSelectable.gameObject, new BaseEventData(eventSystem));
                }
            }
        }
    }
}