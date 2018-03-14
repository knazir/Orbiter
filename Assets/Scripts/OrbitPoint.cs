using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitPoint : MonoBehaviour {

	[SerializeField] private float orbitSpeed = 20.0f;
//	[SerializeField] private bool useLocalPos;
	[SerializeField] private bool rotateClockwise = true;
	private Vector3 orbitCenter;

	void Start () {
		orbitCenter = transform.parent.transform.position;
	}
	
	void Update () {
		Vector3 rotAxis = Vector3.forward;
		if (rotateClockwise) rotAxis = Vector3.back;

		transform.RotateAround (orbitCenter, rotAxis, orbitSpeed * Time.deltaTime);
	}
}
