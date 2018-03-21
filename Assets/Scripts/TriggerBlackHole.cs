using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBlackHole : MonoBehaviour {

	public GameObject blackHole;
	public GameObject targetPlanet;

	[SerializeField] private const float COLLAPSE_SPEED = 3.0f;

	private List<GameObject> planets;
	private GameObject sun;
	private PlatformerController characterController;
	private bool blackHoleIsActive = false;

	void Awake() {
		blackHole.SetActive (blackHoleIsActive);

		sun = GameObject.Find (Constants.SUN);
		planets = new List<GameObject>(GameObject.FindGameObjectsWithTag (Constants.CELESTIAL_BODY));

		GameObject character = GameObject.FindGameObjectWithTag (Constants.PLAYER);
		characterController = character.GetComponent<PlatformerController> ();
	}
		
	void Update () {

		// Trigger the black hole when character reaches right planet
		if (!blackHoleIsActive && characterController.isOnPlanet (targetPlanet)) {
			activateBlackHole ();
		}

		// Move planets if black hole activated
		collapsePlanets();
	}

	///////// Helper Methods ///////////
	private void activateBlackHole() {
		blackHoleIsActive = true;
		blackHole.SetActive (true);
		sun.SetActive (false);
	}

	private void collapsePlanets() {
		if (!blackHoleIsActive)
			return;

		float step = COLLAPSE_SPEED * Time.deltaTime;
		Vector3 blackHolePos = transform.position;

		for (int i = planets.Count - 1; i >= 0; i--) {
			GameObject planet = planets [i];

			planet.transform.position = Vector2.MoveTowards (planet.transform.position, blackHolePos, step);

			// Planet has been absorbed by black hole
			if (planet.transform.position == blackHolePos) {
				// Remove planet from list and scene
				planets.Remove(planet);
				Destroy (planet);
			}
		}
	}
}
