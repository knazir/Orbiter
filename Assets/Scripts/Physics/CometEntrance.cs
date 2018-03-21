using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CometEntrance : MonoBehaviour {

	private void OnCollisionEnter2D(Collision2D col) {
		if (col.gameObject.CompareTag(Constants.CELESTIAL_BODY)) deactivateComet ();
	}

	//////////////// Helper Methods ///////////////

	private void deactivateComet() {
		// Get off children
		foreach (Transform child in transform) {
			child.parent = null;
		}
		Destroy(gameObject);
	}
}
