using UnityEngine;
using System.Collections;

public class DestroyInTime : MonoBehaviour
{

	public float duration;

	void Start () {
		Destroy(gameObject, duration);
	}
}
