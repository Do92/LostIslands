using System.Linq;
using UnityEngine;
using Menu;
using Miscellaneous;

namespace Managers.Menu
{
    [RequireComponent(typeof(Canvas))]
    public class MenuPanelManager : MonoBehaviour
    {
        private MenuPanel[] menuPanels;
        private MenuPanel[] nestedMenuPanels;

        // Used for initialization
        private void Awake()
        {
            menuPanels = GetComponentsInChildren<MenuPanel>(true);

            foreach (MenuPanel menuPanel in menuPanels)
            {
                menuPanel.gameObject.SetActive(false);

                if (menuPanel.ContainsNestedPanels)
                    if (nestedMenuPanels.IsNullOrEmpty())
                        nestedMenuPanels = menuPanel.GetComponentsInChildren<MenuPanel>(true);
                    else
                        nestedMenuPanels = nestedMenuPanels.Concat(menuPanel.GetComponentsInChildren<MenuPanel>(true)).ToArray();
            }

            // Merging the nested menu panels with the top-level menu panels into a single array
            if (!nestedMenuPanels.IsNullOrEmpty())
                menuPanels = menuPanels.Concat(nestedMenuPanels).ToArray();

            //menuPanels[0].gameObject.SetActive(true);
        }

        // HACK: For some odd reason the main panel gets disabled
        public void EnableTheMagicallyDisabledPanel()
        {
            if (menuPanels != null)
            {
                foreach (MenuPanel menuPanel in menuPanels)
                    if (menuPanel.name == "Panel - Main")
                        menuPanel.gameObject.SetActive(true);
            }
        }

        // Shows a new panel and disables others (top-level).
        public void ShowPanel(MenuPanel specifiedMenuPanel)
        {
            foreach (MenuPanel menuPanel in menuPanels)
            {
                // Shows either the main panel or the specified panel
                if (menuPanel.name != "Panel - Main")
                    menuPanel.gameObject.SetActive(specifiedMenuPanel.Equals(menuPanel));
                else
                    menuPanel.gameObject.SetActive(true);

                //if (menuPanel.ContainsNestedPanels && specifiedMenuPanel.Equals(menuPanel))
                // The first nested menu panel will be treated as the main nested menu panel
                //menuPanel.GetComponentsInChildren<MenuPanel>(true)[0].gameObject.SetActive(true);
            }
        }

        // Shows a new panel without disabling other panels.
        public void ShowPopupPanel(MenuPanel specifiedMenuPanel)
        {
            foreach (MenuPanel menuPanel in menuPanels)
                if (menuPanel.Equals(specifiedMenuPanel))
                    specifiedMenuPanel.gameObject.SetActive(true);
        }

        // Disables a specific panel.
        public void HidePanel(MenuPanel menuPanel)
        {
            menuPanel.gameObject.SetActive(false);
        }

        // Exits the game.
        public void QuitGame()
        {
            Application.Quit();
        }
    }
}