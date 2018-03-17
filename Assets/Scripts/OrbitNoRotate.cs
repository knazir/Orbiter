using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitNoRotate : MonoBehaviour {

	/// <summary>
	/// This assumes that the orbiting body is the grandchild of the body it is orbitting around.
	/// </summary>

	[SerializeField] private Transform start;
	[SerializeField] private Transform end;
	[SerializeField] private float orbitSpeed = 0.01f;

	private const string TRIGGER_NAME = "Planet Trigger";

	private const float ANGLE_EPSILON = 10.0f;
	private const float behindPlanetZPos = 1f;
	private const float besidePlanetZPos = 0.0f;
	private const float frontPlanetZPos = -1f;

	private bool atPlanet = false;

	void Start() {
		// Ignore collisions between orbitting planet and moon
		Physics2D.IgnoreCollision(GetComponent<Collider2D>(), transform.parent.parent.gameObject.GetComponent<Collider2D>());
	}

	void Update() {
		Vector2 nextPosXY = Vector2.Lerp (start.localPosition, end.localPosition, Mathf.PingPong(Time.time*orbitSpeed, 1.0f));

		bool shouldBeBehindPlanet = 
			atPlanet && isMovingToEnd (transform.localPosition, nextPosXY, start.localPosition, end.localPosition);
		
		if (shouldBeBehindPlanet) {
			transform.localPosition = new Vector3 (nextPosXY.x, nextPosXY.y, behindPlanetZPos);
		} else if (atPlanet && !shouldBeBehindPlanet) {
			transform.localPosition = new Vector3 (nextPosXY.x, nextPosXY.y, frontPlanetZPos);
		} else {
			transform.localPosition = new Vector3 (nextPosXY.x, nextPosXY.y, besidePlanetZPos);
		}
	}

	void OnTriggerEnter2D(Collider2D col) {
		GameObject ancestorObject = transform.parent.parent.gameObject;
		if (col.gameObject.name == TRIGGER_NAME) {
			// Move moon behind parent planet
			atPlanet = true;
		}
	}

	void OnTriggerExit2D(Collider2D col) {
		if (col.gameObject.name == TRIGGER_NAME) {
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
