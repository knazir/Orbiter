using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CometExit : MonoBehaviour {

	public Vector2 exitTarget;

	private const float FLY_SPEED = 20.0f;
	private const float EXIT_TARGET_DIST = 1000.0f;
	private const float FLY_AWAY_DELAY = 0.5f;

	[SerializeField] private Transform playerTarget;

	private GameObject particleSystemGameObject;
	private PlatformerController platformerController;
	private LevelManager levelManager;
	private bool shouldFlyAway = false;

	private void Start() {
		platformerController = FindObjectOfType<PlatformerController>();
		levelManager = FindObjectOfType<LevelManager>();
		exitTarget = new Vector2(transform.position.x + EXIT_TARGET_DIST, transform.position.y);
		for (var i = 0; i < transform.childCount; i++) {
			var child = transform.GetChild(i);
			if (child.CompareTag(Constants.COMET_PARTICLE_SYSTEM)) {
				particleSystemGameObject = child.gameObject;
				break;
			}
		}
	}

	private void Update() {
		if (shouldFlyAway) {
			transform.position = Vector2.MoveTowards(transform.position, exitTarget, FLY_SPEED * Time.deltaTime);
		}
	}

	private void OnCollisionEnter2D(Collision2D col) {
		if (!col.gameObject.CompareTag(Constants.PLAYER)) return;
		GetComponent<Rigidbody2D>().mass = 1000.0f;
		platformerController.DisableMovement();
		platformerController.WalkToPosition(playerTarget);
	} 
	
	public void FlyAway() {
		Invoke("beginFlight", FLY_AWAY_DELAY);
	}

	private void beginFlight() {
		shouldFlyAway = true;
		particleSystemGameObject.SetActive(true);
		levelManager.FinishLevel();
	}
}
