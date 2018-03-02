using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour {

	private const KeyCode JUMP = KeyCode.Space;
	private const float MOVE_EPSILON = 0.001f;

	[SerializeField] private float jumpForce = 2.0f;
	[SerializeField] private float maxSpeed = 5.0f;
	
	// TODO: Figure this out
	// [SerializeField] private LayerMask whatIsGround;

	private bool facingRight = true;
	private Rigidbody2D myRigidBody;

	private void Start () {
		myRigidBody = GetComponent<Rigidbody2D>();
	}

	private void Update() {
		if (Input.GetKeyDown(JUMP)) {
			myRigidBody.AddForce(new Vector2(0, jumpForce));
		}
	}
	
	private void FixedUpdate () {
		var move = Input.GetAxis("Horizontal");
		if (Math.Abs(move) > MOVE_EPSILON || Math.Abs(move) < -MOVE_EPSILON) {
			myRigidBody.velocity = new Vector2(move * maxSpeed, myRigidBody.velocity.y);
		}
	}

	private void flip() {
		facingRight = !facingRight;
	}
}
