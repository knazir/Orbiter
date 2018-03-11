using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class PlatformerController : MonoBehaviour {

	private const KeyCode SPRINT = KeyCode.LeftShift;
	private const KeyCode JUMP = KeyCode.Space;
	private const float MOVE_EPSILON = 0.001f;

	[SerializeField] private float jumpForce = 500.0f;
	[SerializeField] private float jumpFactor = 1.0f;
	[SerializeField] private float moveSpeed = 5.0f;
	[SerializeField] private float rotateSpeed = 5.0f;
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private float groundDetectRadius = 1.0f;
	[SerializeField] private float groundRayLength = 1.0f;

	private float curJumpForce = 0.0f;
	private bool facingRight = true;
	private Rigidbody2D myRigidBody;
	private Animator myAnimator;
	private CameraController cameraController;

	//////////////////// Unity Event Handlers ////////////////////
	
	private void Start () {
		myRigidBody = GetComponent<Rigidbody2D>();
		myAnimator = GetComponent<Animator>();
		cameraController = FindObjectOfType<CameraController>();
	}

	private void Update() {
		if (getInputJump()) applyJump();
	}
	
	private void FixedUpdate () {
		// forceJump();
		if (isGrounded()) move();
		else spin();
		if (getIncomingGround() != null) {
			reorientToLandOn();
			// cameraController.ZoomOut();
		} else {
			// cameraController.ZoomIn();
		}
	}
	
	//////////////////// Helper Methods ////////////////////////
	
	private bool getInputJump() {
		// check keyboard input or if there's at least one finger touching
		return Input.GetKeyDown(JUMP) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
	}

	private void reorientToLandOn() {
		rotateByRaycastFrom(-transform.up);		// down
		rotateByRaycastFrom(transform.up);		// up
		rotateByRaycastFrom(transform.right);	// right
		rotateByRaycastFrom(-transform.right);	// left
	}

	private void rotateByRaycastFrom(Vector2 direction) {
		var raycast = Physics2D.Raycast(transform.position, direction, groundDetectRadius, groundLayer);
		if (raycast.collider == null) return;
		transform.rotation = Quaternion.FromToRotation(Vector2.up, raycast.normal);
	}

	private void forceJump() {
		bool fingerTouching = Input.touchCount > 0;
		print ("Fingers touching" + fingerTouching);
		if (fingerTouching) {
			Touch touch = Input.GetTouch(0);

			print ("Touching..." + touch.phase);
			switch (touch.phase) {
			case TouchPhase.Began:
				// Init jump force
				curJumpForce = 0.0f;
				break;

			case TouchPhase.Ended:
				// Apply jump force
				applyJump(curJumpForce);
				break;

			default:
				// Add to jump force
				curJumpForce += jumpFactor;
				print("Jump is " + curJumpForce);
				break;
			}
		}
	}

	private void applyJump() {
		myAnimator.SetTrigger("Jump");
		var jumpDirection = transform.up * jumpForce;
		myRigidBody.AddForce(jumpDirection);
		myRigidBody.angularVelocity = 0.0f;
	}

	private void applyJump(float jumpForce) {
		myAnimator.SetTrigger("Jump");
		var jumpDirection = transform.up * jumpForce;
		myRigidBody.AddForce(jumpDirection);
	}

//	/**
//	 * forceJump
//	 * Jumps based on how long user held down on screen
//	 */
//	private void forceJump() {
//		
//	}

	private void move() {
		var move = Input.GetAxis("Horizontal");
		var moving = Math.Abs(move) > MOVE_EPSILON || Math.Abs(move) < -MOVE_EPSILON;
		myAnimator.SetBool("Running", moving);
		if (!moving) return;
		
		// flip orientation if we're reversing directions
		if (move > 0 && !facingRight || move < 0 && facingRight) flip();

		var moveDirection = transform.right * move * moveSpeed;
		moveDirection.y = myRigidBody.velocity.y;
		moveDirection.z = 0.0f;
		myRigidBody.velocity = moveDirection;
	}

	private void spin() {
		var move = Input.GetAxis("Horizontal");
		var moving = Math.Abs(move) > MOVE_EPSILON || Math.Abs(move) < -MOVE_EPSILON;
		if (!moving) return;
		
		// negate rotate force to align with movement directions
		myRigidBody.angularVelocity = 0.0f;
		transform.Rotate(0, 0, move * -rotateSpeed);
	}

	private void flip() {
		facingRight = !facingRight;
		var localScale = transform.localScale;
		var xScale = Mathf.Abs(localScale.x);
		if (!facingRight) xScale *= -1;
		transform.localScale = new Vector3(xScale, localScale.y, localScale.z);
		myRigidBody.velocity = Vector2.zero;
	}

	private Collider2D getIncomingGround() {
		return Physics2D.OverlapCircle(transform.position, groundDetectRadius, groundLayer);
	}

	private bool isGrounded() {
		var direction = -transform.up;
		var raycastHit = Physics2D.Raycast(transform.position, direction, groundRayLength, groundLayer);
		return raycastHit.collider != null;
	}
}
