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

	private const float behindPlanetZPos = 0.1f;
	private const float frontPlanetZPos = -0.1f;
	private bool atPlanet = false;
	private bool movingToEnd = true;

	private float besidePlanetZPos;

	void Start() {
		besidePlanetZPos = transform.position.z;
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
			transform.localPosition = nextPosXY;
		}
	}

	void OnTriggerEnter2D(Collider2D col) {
		GameObject ancestorObject = transform.parent.parent.gameObject;
		if (col.gameObject.name == ancestorObject.name) {
			// Move moon behind parent planet
			atPlanet = true;
		}
	}

	void OnTriggerExit(Collider col) {
		GameObject ancestorObject = transform.parent.parent.gameObject;
		if (col.gameObject.name == ancestorObject.name) {
			// No longer behind
			atPlanet = false;
		}
	}

	bool isMovingToEnd(Vector2 prevPos, Vector2 nextPos, Vector2 start, Vector2 end) {
		return ((end - start).normalized == (nextPos - prevPos).normalized);
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
