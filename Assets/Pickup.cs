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

		// Use this for initialization
		void Start () {
			
			matchData = GameObject.Find ("MatchData(Clone)").GetComponent<MatchData> ();				

		}
	
		// Update is called once per frame
		void Update () {
	
		}

		void OnTriggerExit(Collider other){

			if (pickupID == 0) {			

				GameObject points = (GameObject)GameObject.Instantiate(pointPickup5, this.gameObject.transform.position, Quaternion.identity);
				points.GetComponent<RisingText>().Setup (3.0f, 2f);

				//GameObject.Find("PlayerData").GetComponent<PlayerData>().AddScore (5);
				//Debug.Log(other.gameObject.GetComponent<Player>().PlayerId);
				matchData.GetPlayerData (other.gameObject.GetComponent<Player>().PlayerId).AddScore (5);
			
			}

			else if (pickupID == 1) {			

				GameObject points = (GameObject)GameObject.Instantiate(pointPickup10, this.gameObject.transform.position, Quaternion.identity);
				points.GetComponent<RisingText>().Setup (3.0f, 2f);

				matchData.GetPlayerData (other.gameObject.GetComponent<Player>().PlayerId).AddScore (10);

			}

			Destroy (this.gameObject);

		}
	
			
	}

}
