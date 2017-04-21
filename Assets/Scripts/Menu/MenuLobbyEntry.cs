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

        public void Initialize(PlayerData playerData)
        {
            Color color = playerData.Character.MainColor;
            color.a = PlayerColor.color.a;
            PlayerColor.color = color;

            PlayerImage.sprite = playerData.Character.Image;
            PlayerName.text = playerData.Character.Name;
            SelectedImage.gameObject.SetActive(playerData.isLocalPlayer);
        }
    }
}