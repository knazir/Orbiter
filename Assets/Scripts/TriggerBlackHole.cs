using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBlackHole : MonoBehaviour {

	public GameObject blackHole;
	public GameObject targetPlanet;

	private GameObject[] planets;
	private PlatformerController characterController;

	void Awake() {
		blackHole.SetActive (false);

		planets = GameObject.FindGameObjectsWithTag (Constants.CELESTIAL_BODY);

		GameObject character = GameObject.FindGameObjectWithTag (Constants.PLAYER);
		characterController = character.GetComponent<PlatformerController> ();
	}
		
	void Update () {

		// Trigger the black hole when character reaches right planet
		if (characterController.isOnPlanet (targetPlanet)) {
			initBlackHoleCollapse ();
		}
	}

	///////// Helper Methods ///////////
	private void initBlackHoleCollapse() {
		blackHole.SetActive (true);

		foreach (GameObject planet in planets) {
			planet.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.None;
		}
	}
}
