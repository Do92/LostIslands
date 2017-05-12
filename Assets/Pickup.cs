using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Game.Networking;
using Managers;
using Miscellaneous;

namespace Game
{

	public class Pickup : MonoBehaviour {

		public int pickupID;
		public GameObject pointPickup5;
		public GameObject pointPickup10;

		private MatchData matchData;
		private bool isColliding;

		// Use this for initialization
		void Start () {
			
			matchData = GameObject.Find ("MatchData(Clone)").GetComponent<MatchData> ();				

		}
	
		// Update is called once per frame
		void Update () {

			isColliding = false;
		}

		void OnTriggerEnter(Collider other){

			if(isColliding) return;

			if (pickupID == 0) {			

				GameObject points = (GameObject)GameObject.Instantiate(pointPickup5, this.gameObject.transform.position, Quaternion.identity);
				points.GetComponent<RisingText>().Setup ();
				isColliding = true;

				matchData.GetPlayerData (other.gameObject.GetComponent<Player>().PlayerId).AddScore (3);
			
			}

			else if (pickupID == 1) {			

				GameObject points = (GameObject)GameObject.Instantiate(pointPickup10, this.gameObject.transform.position, Quaternion.identity);
				points.GetComponent<RisingText>().Setup ();
				isColliding = true;

				matchData.GetPlayerData (other.gameObject.GetComponent<Player>().PlayerId).AddScore (5);

			}

			Destroy (this.gameObject);

		}
	
			
	}

}
