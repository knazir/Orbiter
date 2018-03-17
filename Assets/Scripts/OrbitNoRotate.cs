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
	private bool movingToEnd = true;

//	private Renderer orbiterRenderer;

	void Start() {
		// Ignore collisions between orbitting planet and moon
		Physics2D.IgnoreCollision(GetComponent<Collider2D>(), transform.parent.parent.gameObject.GetComponent<Collider2D>());

//		orbiterRenderer = GetComponent<Renderer> ();
	}

	void Update() {
		Vector2 nextPosXY = Vector2.Lerp (start.localPosition, end.localPosition, Mathf.PingPong(Time.time*orbitSpeed, 1.0f));

		bool shouldBeBehindPlanet = 
			atPlanet && isMovingToEnd (transform.localPosition, nextPosXY, start.localPosition, end.localPosition);
		
		if (shouldBeBehindPlanet) {
			transform.localPosition = new Vector3 (nextPosXY.x, nextPosXY.y, behindPlanetZPos);
//			orbiterRenderer.enabled = false;
		} else if (atPlanet && !shouldBeBehindPlanet) {
			transform.localPosition = new Vector3 (nextPosXY.x, nextPosXY.y, frontPlanetZPos);
//			orbiterRenderer.enabled = true;
		} else {
			transform.localPosition = new Vector3 (nextPosXY.x, nextPosXY.y, besidePlanetZPos);
//			orbiterRenderer.enabled = true;
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
//		GameObject ancestorObject = transform.parent.parent.gameObject;
		if (col.gameObject.name == TRIGGER_NAME) {
			// No longer behind
			atPlanet = false;
		}
	}

	bool isMovingToEnd(Vector2 prevPos, Vector2 nextPos, Vector2 start, Vector2 end) {
		// Checks if two vectors are close to parallel
//		print ("End to start normalized, " + (end - start).normalized);
//		print ("Next to prev normalized, " + (nextPos - prevPos).normalized);
//		Vector2 epsilon = new Vector2 (0.1f, 0.1f);

		Vector3 movementVec = new Vector3((nextPos - prevPos).x, (nextPos - prevPos).y, 0);
		Vector3 forwardVec = new Vector3((end - start).x, (end - start).y, 0);

		float angle = Vector3.Angle (movementVec, forwardVec);

		return (angle < ANGLE_EPSILON);
	}

//	private float radius;
//	private float angle = 0.0f;
//
////	[SerializeField] private bool 
//
//	// Use this for initialization
//	void Start () {
//		radius = transform.localPosition.x;
//	}
//	
//	// Update is called once per frame
//	void Update () {
//		angle += RotateSpeed * Time.deltaTime;
//
//		Vector2 realOffset = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * radius;
//		Vector3	totalOffset = new Vector3 (realOffset.x, 0, realOffset.y);
//		transform.position = pivot.position + totalOffset;
//	}
}
