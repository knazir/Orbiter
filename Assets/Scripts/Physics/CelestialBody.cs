using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : MonoBehaviour {
	
	private const float MASS_SCALAR = 30.0f;

	[SerializeField] private float angularVelocity = 1.0f;
	[SerializeField] private bool rotateRight = true;
	[SerializeField] private bool autoSetMass = false;

	private Rigidbody2D myRigidbody;
	private float actualAngularVelocity;

	private void Start() {
		myRigidbody = GetComponent<Rigidbody2D>();
		Renderer myRenderer = GetComponent<Renderer> ();

		actualAngularVelocity = angularVelocity;
		if (rotateRight) actualAngularVelocity *= -1;
		if (autoSetMass) myRigidbody.mass = MASS_SCALAR * myRenderer.bounds.size.x;
	}
	
	private void FixedUpdate () {
		myRigidbody.angularVelocity =  actualAngularVelocity;
	}
}
