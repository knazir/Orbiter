using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CometEntrance : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter2D(Collision2D col) {
		if (col.gameObject.tag == Constants.CELESTIAL_BODY) {
			deactivateComet ();
		}
	}


	//////////////// Helper Methods ///////////////
	private void deactivateComet() {
		// Get off children
		foreach (Transform child in transform) {
			child.parent = null;
		}

		Destroy (gameObject);
	}
}
