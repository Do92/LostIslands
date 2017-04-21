using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Managers.Menu;
using Menu;

namespace Miscellaneous
{
    /// <summary>
    /// This class handles small utilities that can be used by UGui.
    /// </summary>
    [RequireComponent(typeof(MenuPanelManager))]
    public class MenuUtilities : MonoBehaviour
    {
        public static string ErrorText = "";

        public MenuPanel ErrorPopupPanel;
        public Text ErrorPopupText;
        public Dropdown GameModesDropdown;
        public Dropdown QuestionListsDropdown;

        private MenuPanelManager menuControl;

        // Initialization
        private void Awake()
        {
            menuControl = GetComponent<MenuPanelManager>();

            if (!GameModesDropdown || !QuestionListsDropdown)
            {
                Debug.LogWarning("MenuUtilities is missing one of the drop-down lists!");
                return;
            }

            // This puts the game mode options in the drop-down list which then becomes available in the GUI
            InitializeDropDownList(GameModesDropdown, GameManager.Instance.GameModes.Select(gameMode => gameMode.Name).ToList());
            // This initializes the game mode of the default selected drop-down list option for actual usage
            GameManager.Instance.ChangeGameMode(GameModesDropdown.value);
            // This executes the ChangeGameMode method when a game mode option has been chosen from the drop-down list
            GameModesDropdown.onValueChanged.AddListener(delegate { GameManager.Instance.ChangeGameMode(GameModesDropdown.value); });

            // This puts the question list options in the drop-down list, which then becomes available in the GUI
            InitializeDropDownList(QuestionListsDropdown, QuestionManager.Instance.XmlQuestionLists.Select(xmlQuestionList => xmlQuestionList.name).ToList());
            // This initializes the question list of the default selected drop-down list option for actual usage
            QuestionManager.Instance.ChangeQuestionList(QuestionListsDropdown.value);
            // This executes the ChangeQuestionList method when a question list option has been chosen from the drop-down list
            QuestionListsDropdown.onValueChanged.AddListener(delegate { QuestionManager.Instance.ChangeQuestionList(QuestionListsDropdown.value); });
        }

        // Initialization
        private void Start()
        {
            if (!ErrorText.IsEmpty())
            {
                ErrorPopupText.text = ErrorText;
                ErrorText = "";

                menuControl.ShowPopupPanel(ErrorPopupPanel);
            }
        }

        // (re-)Initializes available option names for the drop-down lists
        private void InitializeDropDownList(Dropdown dropDownList, List<string> availableOptionNames)
        {
            dropDownList.ClearOptions();
            dropDownList.AddOptions(availableOptionNames);
        }

        // Exits the game.
        public void Exit()
        {
            Application.Quit();
        }
    }
}