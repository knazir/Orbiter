using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitMoon : MonoBehaviour {

	/// <summary>
	/// This assumes that the orbiting body is the grandchild of the body it is orbitting around.
	/// </summary>

	[SerializeField] private Transform start;
	[SerializeField] private Transform end;
	[SerializeField] private float orbitSpeed = 0.01f;

	private const float ANGLE_EPSILON = 10.0f;
	private const float behindPlanetZPos = 1f;
	private const float besidePlanetZPos = 0.0f;
	private const float frontPlanetZPos = -1f;

	private bool atPlanet = false;
	private Vector3 originalStartPos;
	private Vector3 originalEndPos;

	void Awake() {
		originalStartPos = start.position;
		originalEndPos = end.position;

//		GameObject planetTrigger = GameObject.Find("../../" + Constants.PLANET_TRIGGER);
//		bool planetTriggerExists = (planetTrigger != null);
//		if (!planetTriggerExists)
//			throw new UnityException ("Missing planet trigger for" + gameObject.name);
	}

	void Start() {
		// Ignore collisions between orbitting planet and moon
		Physics2D.IgnoreCollision(GetComponent<Collider2D>(), transform.parent.parent.gameObject.GetComponent<Collider2D>());
	}

	void FixedUpdate() {
		Vector2 nextPosXY = Vector2.Lerp (originalStartPos, originalEndPos, Mathf.PingPong(Time.time*orbitSpeed, 1.0f));

		bool shouldBeBehindPlanet = 
			atPlanet && isMovingToEnd (transform.position, nextPosXY, originalStartPos, originalEndPos);
		
		if (shouldBeBehindPlanet) {
			transform.position = new Vector3 (nextPosXY.x, nextPosXY.y, behindPlanetZPos);
		} else if (atPlanet && !shouldBeBehindPlanet) {
			transform.position = new Vector3 (nextPosXY.x, nextPosXY.y, frontPlanetZPos);
		} else {
			transform.position = new Vector3 (nextPosXY.x, nextPosXY.y, besidePlanetZPos);
		}
	}

	void OnTriggerEnter2D(Collider2D col) {
		if (col.gameObject.name == Constants.PLANET_TRIGGER) {
			// Move moon behind parent planet
			atPlanet = true;
		}
	}

	void OnTriggerExit2D(Collider2D col) {
		if (col.gameObject.name == Constants.PLANET_TRIGGER) {
			// No longer behind
			atPlanet = false;
		}
	}

	bool isMovingToEnd(Vector2 prevPos, Vector2 nextPos, Vector2 start, Vector2 end) {
		// Checks if two vectors are close to parallel

		Vector3 movementVec = new Vector3((nextPos - prevPos).x, (nextPos - prevPos).y, 0);
		Vector3 forwardVec = new Vector3((end - start).x, (end - start).y, 0);

		float angle = Vector3.Angle (movementVec, forwardVec);

		return (angle < ANGLE_EPSILON);
	}
}
