using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCelestialBody : MonoBehaviour {

	[SerializeField] private float angularVelocity = 1.0f;
	[SerializeField] private bool rotateRight = true;

	private Rigidbody2D myRigidbody;
	private float actualAngularVelocity;

	private void Start() {
		myRigidbody = GetComponent<Rigidbody2D>();
		actualAngularVelocity = angularVelocity;
		if (rotateRight) actualAngularVelocity *= -1;
	}
	
	private void FixedUpdate () {
		myRigidbody.angularVelocity =  actualAngularVelocity;
	}
}
