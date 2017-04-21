using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Interface
{
    public class PlaceholderReactivator : MonoBehaviour
    {
        public Text PlaceholderText;
        public Text ActualText;

        // Use this for initialization
        private void Start()
        {
            GetComponent<InputField>().onEndEdit.AddListener(delegate { if (string.IsNullOrEmpty(ActualText.text)) { PlaceholderText.enabled = true; } });
        }
    }
}