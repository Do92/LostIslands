using UnityEngine;
using System.Collections;
using Game;
using UnityEngine.UI;

public class HealedTileBar : MonoBehaviour
{

	public Image image;

	void Update ()
	{
		Level level = FindObjectOfType<Level>();
		image.fillAmount = ((float)level.healedTileCount / (float)level.tileCount);
	}
}
