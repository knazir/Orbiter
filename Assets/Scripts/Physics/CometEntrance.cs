using System.Collections;
using System.Collections.Generic;
using Exploder2D;
using UnityEngine;

public class CometEntrance : MonoBehaviour {
	
	private const float DESTROY_DELAY = 1.0f;

	[SerializeField] private float initialVelocity;

	private Exploder2DObject exploder;
	private PlatformerController platformerController;
	private bool hopped = false;

	private void Start() {
		platformerController = FindObjectOfType<PlatformerController>();
		GetComponent<Rigidbody2D>().velocity = transform.right * initialVelocity;
		exploder = Exploder2D.Utils.Exploder2DSingleton.Exploder2DInstance;
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag(Constants.PLAYER) || hopped) return;
		// Get player off comet
		hopped = true;
		platformerController.HopOffComet();
		platformerController.EnableMovement();
	}

	private void OnCollisionEnter2D(Collision2D other) {
		if (other.gameObject.CompareTag(Constants.CELESTIAL_BODY)) deactivateComet();
	}

	//////////////// Helper Methods ///////////////

	private void deactivateComet() {
		// Remove non-particle-system children
		foreach (Transform child in transform) {
			if (child.GetComponent<ParticleSystem>() == null && !child.CompareTag(Constants.EXPLODER_2D)) {
				child.parent = null;
			}
		}
		explode();
	}

	private void explode() {
		// Blow up comet
		Exploder2DUtils.SetActive(exploder.gameObject, true);
		exploder.transform.position = Exploder2DUtils.GetCentroid(gameObject);
		GameObject.FindGameObjectWithTag(Constants.EXPLOSION_PLAYER).GetComponent<AudioSource>().Play();
		exploder.Explode();
		Invoke("destroy", DESTROY_DELAY);
	}

	private void destroy() {
		Destroy(gameObject);
		Destroy(GameObject.FindGameObjectWithTag(Constants.EXPLOSION_PLAYER).GetComponent<AudioSource>());
	}
}
