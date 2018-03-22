using System;
using System.Collections;
using System.Collections.Generic;
using Exploder2D;
using UnityEngine;

public class TriggerBlackHole : MonoBehaviour {

	private const float BLACK_HOLE_DELAY = 1.5f;
	private const float REMOVE_SUN_DELAY = 1.0f;

	public GameObject blackHole;
	public GameObject targetPlanet;

	[SerializeField] private float collapseSpeed = 3.0f;
	[SerializeField] private float blackHoleMinDistance = 2.0f;

	private List<GameObject> planets;
	private GameObject sun;
	private PlatformerController characterController;
	private Exploder2DObject exploder;
	
	private static bool sunExploded = false;
	private static bool blackHoleIsActive = false;

	private void Awake() {
		sunExploded = false;
		blackHoleIsActive = false;
		blackHole.SetActive(blackHoleIsActive);
		sun = GameObject.FindGameObjectWithTag(Constants.SUN);
		planets = new List<GameObject>(GameObject.FindGameObjectsWithTag(Constants.CELESTIAL_BODY));
		characterController = FindObjectOfType<PlatformerController>();
	}

	private void Start() {
		exploder = Exploder2D.Utils.Exploder2DSingleton.Exploder2DInstance;
	}
		
	private void Update() {
		// Trigger the black hole when character reaches right planet
		if (!blackHoleIsActive && characterController.isOnPlanet(targetPlanet)) {
			activateBlackHole();
		}

		// Move planets if black hole activated
		collapsePlanets();
	}

	///////// Helper Methods ///////////

	private void activateBlackHole() {
		Invoke("startBlackHole", BLACK_HOLE_DELAY);
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
		if (other.CompareTag(Constants.PLAYER)) {
			FindObjectOfType<LevelManager>().ReloadScene();
		} else if (!other.CompareTag(Constants.SUN)) {
			if (planets.IndexOf(other.gameObject) != -1) planets.Remove(other.gameObject);
			Destroy(other.gameObject);
		}
	}

	private void startBlackHole() {
		if (sunExploded) return;
		FindObjectOfType<LevelManager>().PlayEvilBGM();
		sun.tag = Constants.EXPLODER_2D;
		Exploder2DUtils.SetActive(sun, true);
		exploder.transform.position = Exploder2DUtils.GetCentroid(sun);
		exploder.Radius = 100.0f;
		exploder.Force = 32.0f;
		exploder.TargetFragments = 420;
		sunExploded = true;
		exploder.Explode();
		blackHoleIsActive = true;
		blackHole.SetActive(true);
	}
}
