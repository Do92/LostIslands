using UnityEngine;
using System.Collections;
[RequireComponent(typeof(TextMesh))]
public class RisingText : MonoBehaviour
{
	public float duration;
	public float rise_speed;

	// private variables:
	Vector3 crds_delta;
	float   alpha;
	float   life_loss;
	Camera  cam;

	// public variables - you can change this in Inspector if you need to
	public Color color = Color.white;

	// SETUP - call this once after having created the object, to make it 
	// "points" shows the points.
	// "duration" is the lifespan of the object
	// "rise speed" is how fast it will rise over time.
	public void Setup()
	{
		//GetComponent<TextMesh>().text = points.ToString();        
		life_loss = 1f / duration;
		crds_delta = new Vector3(0f, rise_speed, 0f);        
	}

	void Start() // some default values. You still need to call "setup"
	{
		alpha = 1f;
		cam = Camera.main;
		crds_delta = new Vector3(0f, 1f, 0f);
		life_loss = 0.3f;
	}

	void Update () 
	{
		// move upwards :
		transform.Translate(crds_delta * Time.deltaTime, Space.World);

		// change alpha :
		alpha -= Time.deltaTime * life_loss;
		GetComponent<Renderer>().material.color = new Color(color.r,color.g,color.b,alpha);

		// if completely faded out, die:
		if (alpha <= 0f) Destroy(gameObject);

		// make it face the camera:
		transform.LookAt(cam.transform.position);
		transform.rotation = cam.transform.rotation;        
	}
}
