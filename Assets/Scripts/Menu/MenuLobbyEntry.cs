using UnityEngine;
using UnityEngine.UI;
using Game.Networking;

namespace Menu
{
    /// <summary>
    /// This is the Entry is the lobby screen in the menu scene.
    /// </summary>
    public class MenuLobbyEntry : MonoBehaviour
    {
        public Image PlayerColor;
        public Image PlayerImage;
        public Image SelectedImage;
        public Text PlayerName;
		public GameObject Character;

        public void Initialize(PlayerData playerData)
        {
			Color color = playerData.Character.EmissionColor;
            color.a = PlayerColor.color.a;
            PlayerColor.color = color;
			Character = GameObject.FindGameObjectWithTag ("CharacterInMenu");

			Character.GetComponent<Renderer> ().materials[0].color = new Color(color.r, color.g, color.b, 1.0f);
            PlayerImage.sprite = playerData.Character.Image;
            PlayerName.text = playerData.Character.Name;
            SelectedImage.gameObject.SetActive(playerData.isLocalPlayer);
        }
    }
}