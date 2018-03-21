using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CometExit : MonoBehaviour {

	public Vector2 exitTarget;

	private const float FLY_SPEED = 20.0f;
	private const float EXIT_TARGET_DIST = 1000.0f;

	private bool shouldFlyAway = false;

	private void Start () {
		exitTarget = new Vector2(transform.position.x + EXIT_TARGET_DIST, transform.position.y);
	}
	
	private void Update () {
		if (shouldFlyAway) {
			transform.position = Vector2.MoveTowards(transform.position, exitTarget, FLY_SPEED * Time.deltaTime);
		}
	}

	private void OnCollisionEnter2D(Collision2D col) {
		if (col.gameObject.CompareTag(Constants.PLAYER)) shouldFlyAway = true;
	} 
}
