using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBlackHole : MonoBehaviour {

	public GameObject blackHole;
	public GameObject targetPlanet;

	[SerializeField] private float collapseSpeed = 3.0f;
	[SerializeField] private float blackHoleMinDistance = 2.0f;

	private List<GameObject> planets;
	private GameObject sun;
	private PlatformerController characterController;
	private bool blackHoleIsActive = false;

	private void Awake() {
		blackHole.SetActive(blackHoleIsActive);

		sun = GameObject.Find(Constants.SUN);
		planets = new List<GameObject>(GameObject.FindGameObjectsWithTag(Constants.CELESTIAL_BODY));

		var character = GameObject.FindGameObjectWithTag(Constants.PLAYER);
		characterController = character.GetComponent<PlatformerController> ();
	}
		
	private void Update () {
		// Trigger the black hole when character reaches right planet
		if (!blackHoleIsActive && characterController.isOnPlanet (targetPlanet)) {
			activateBlackHole();
		}

		// Move planets if black hole activated
		collapsePlanets();
	}

	///////// Helper Methods ///////////

	private void activateBlackHole() {
		blackHoleIsActive = true;
		blackHole.SetActive(true);
		sun.SetActive(false);
	}

	private void collapsePlanets() {
		if (!blackHoleIsActive) return;
		var step = collapseSpeed * Time.deltaTime;
		for (var i = planets.Count - 1; i >= 0; i--) {
			var planet = planets[i];
			if (planet == null) continue;
			planet.transform.position = Vector2.MoveTowards(planet.transform.position, transform.position, step);
		}
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (sun.activeSelf) return;
		if (other.CompareTag(Constants.PLAYER)) {
			FindObjectOfType<LevelManager>().ReloadScene();
		} else {
			if (planets.IndexOf(other.gameObject) != -1) planets.Remove(other.gameObject);
			Destroy(other.gameObject);
		}
	}
}
