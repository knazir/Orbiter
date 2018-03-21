using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CometExit : MonoBehaviour {

	public Vector2 exitTarget;

	private const float FLY_SPEED = 20.0f;
	private const float EXIT_TARGET_DIST = 1000.0f;

	private bool shouldFlyAway = false;

	// Use this for initialization
	void Start () {
		exitTarget = new Vector2 (transform.position.x + EXIT_TARGET_DIST, transform.position.y);
	}
	
	// Update is called once per frame
	void Update () {
		if (shouldFlyAway) {
			transform.position = Vector2.MoveTowards (transform.position, exitTarget, FLY_SPEED * Time.deltaTime);
		}
	}

	void OnCollisionEnter2D(Collision2D col) {
		if (col.gameObject.tag == Constants.PLAYER) {
			shouldFlyAway = true;
		}
	} 
}
