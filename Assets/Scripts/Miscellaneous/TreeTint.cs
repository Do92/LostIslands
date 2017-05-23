using UnityEngine;
using System.Collections;
using Game;
using UnityEngine.UI;

public class TreeTint : MonoBehaviour {

	public Material startMat;
	public Material endMat;
	private Renderer treeRenderer;
	private Level level;
	private float clamp;

	void Start(){
		treeRenderer = GetComponent<Renderer>();
		treeRenderer.material = startMat;
		level = FindObjectOfType<Level>();
	}

	void Update ()
	{
		treeRenderer.material.Lerp (startMat, endMat, ((float)level.healedTileCount / (float)level.tileCount));
	}
}
