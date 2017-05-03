using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Menu;
using Miscellaneous;

namespace Managers.Menu
{
    /// <summary>
    /// This class handles the login for accounts.
    /// </summary>
    public class MenuLoginManager : MonoBehaviour
    {
        public MenuPanelManager MenuPanelManager;
        public MenuPanel PopupPanel;

        [Header("Fields")]
        public InputField UsernameField;
        public InputField PasswordField;
        public Button EscapeButton;
        public Text StatusText;

        public string LoadingText;
        public string ErrorText;

        public string username;
        public string password;

        [Header("Buttons")]
        public GameObject[] LoginButtons;
        public GameObject[] LogoutButtons;
        public Button StartServerButton;
        public GameObject startServerPanel;

        private GameManager gameManager;

        // Initialization.
        private void Awake()
        {
            gameManager = GameManager.Instance;
        }

        private void Start()
        {
            // For giving me permission without actually logging in
            //PlayerPrefs.SetInt(GameManager.PermissionKey, 1);
            //PlayerPrefs.Save();

            //SetLoggedIn(PlayerPrefs.HasKey(GameManager.PermissionKey));

            // Temporarily preventing access to the server-making for the players of the upcoming classroom play test (client build)
            Logout();
        }

        // Tries to log in the user.
        public void Login()
        {
            // Temporarily giving myself direct permission on button click (don't forget to change the event of Button - Login back to ShowPanel from Canvas - Menu)
            if (UsernameField.text == username && PasswordField.text == password)
            {
                PlayerPrefs.SetInt(GameManager.PermissionKey, 1);
                PlayerPrefs.Save();
                SetLoggedIn(true);
            }
            //string md5Hash = MD5.Encrypt(UsernameField.text + gameManager.EncryptionSalt + PasswordField.text);

            //Debug.Log("MD5: " + md5Hash);

            //EscapeButton.interactable = false;
            //StatusText.text = LoadingText;

            //StartCoroutine(AttemptLogin(md5Hash));
        }

        // Logs the user out.
        public void Logout()
        {
            PlayerPrefs.DeleteKey(GameManager.PermissionKey);
            PlayerPrefs.Save();

            SetLoggedIn(false);
        }

        private void OnLoginResponse(WWW response)
        {
            EscapeButton.interactable = true;

            if (response.error != null)
            {
                StatusText.text = ErrorText + response.error;

                Debug.LogError("Error Response: " + response.error);
            }
            else
            {
                int permission = 0;

                if (int.TryParse(response.text, out permission))
                {
                    StatusText.text = "";
                    MenuPanelManager.HidePanel(PopupPanel);

                    PlayerPrefs.SetInt(GameManager.PermissionKey, permission);
                    PlayerPrefs.Save();

                    SetLoggedIn(true);
                }
                else
                    StatusText.text = response.text;

                Debug.Log("Response: " + response.text);
            }
        }

        // Login attempt.
        private IEnumerator AttemptLogin(string md5Hash)
        {
            Debug.Log("Attempting login...");

            string url = gameManager.DatabaseUrl + "hash=" + md5Hash;

            WWW response = new WWW(url);
            yield return response;

            Debug.Log("Got response!");

            OnLoginResponse(response);
        }

        // When the user is logged in.
        private void SetLoggedIn(bool isLoggedIn)
        {
            foreach (GameObject loginButton in LoginButtons)
                loginButton.SetActive(!isLoggedIn);
            foreach (GameObject logoutButton in LogoutButtons)
                logoutButton.SetActive(isLoggedIn);
            StartServerButton.gameObject.SetActive(isLoggedIn);
            startServerPanel.SetActive(isLoggedIn);
        }
    }
}