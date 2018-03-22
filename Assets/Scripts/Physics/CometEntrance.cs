using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CometEntrance : MonoBehaviour {

	[SerializeField] private float initialVelocity;

	private void Start() {
		GetComponent<Rigidbody2D>().velocity = Vector2.right * initialVelocity;
	}

	private void OnCollisionEnter2D(Collision2D col) {
		if (col.gameObject.CompareTag(Constants.CELESTIAL_BODY)) deactivateComet();
	}

	//////////////// Helper Methods ///////////////

	private void deactivateComet() {
		// Remove non-particle-system children
		foreach (Transform child in transform) {
			if (child.GetComponent<ParticleSystem>() == null) child.parent = null;
		}
		Destroy(gameObject);
	}
}
