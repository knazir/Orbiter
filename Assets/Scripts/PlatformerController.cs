using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class PlatformerController : MonoBehaviour {

	private const KeyCode JUMP = KeyCode.Space;
	private const float MOVE_EPSILON = 0.001f;

	private const float MOVE_EPSILON_TOUCH = 100.0f; // subject to change
	private const float ALIGNMENT_EPSILON = 1.0f;
	private const float GROUNDED_DIST = 0.8f;
	private const float MAX_JUMP_FORCE = 1000.0f;

	[SerializeField] private float defaultJumpForce = 500.0f;
	[SerializeField] private float defaultBoostForce = 250.0f;
	[SerializeField] private float jumpFactor = 1.0f;
	[SerializeField] private float moveSpeed = 5.0f;
	[SerializeField] private float rotateSpeed = 5.0f;
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private float groundDetectRadius = 1.0f;
	[SerializeField] private float groundRayLength = 1.0f;

	private Rigidbody2D myRigidBody;
	private Animator myAnimator;
	private TouchButtonHandler touchButtonHandler;
	private bool facingRight = true;
	private float curJumpForce = 0.0f;
	private float curTotalTouchDelta = 0.0f;
	private bool moving = false;
	private bool movingRight = true;

	//////////////////// Unity Event Handlers ////////////////////
	
	private void Start () {
		myRigidBody = GetComponent<Rigidbody2D>();
		myAnimator = GetComponent<Animator>();
		touchButtonHandler = FindObjectOfType<TouchButtonHandler>();
	}

	private void Update() {
		if (getInputJump()) applyJump(defaultJumpForce);
		//if (getInputJump() && !touchButtonHandler.ButtonPressed()) applyJump(defaultJumpForce);
	}
	
	private void FixedUpdate () {
//		if (moving) {
//			if (isGrounded()) {
//				if (movingRight) moveRight();
//				else moveLeft();
//			} else {
//				if (movingRight) rotateRight();
//				else rotateLeft();
//			}
//		}
		
		if (isGrounded()) handleKeyboardPlanetMovement();
		else handleKeyboardSpaceMovement();
		
		myRigidBody.angularVelocity = 0.0f;
		
		if (getIncomingGround() != null) reorientToLandOn();
	}
	
	//////////////////// Touch Input ////////////////////////

	public void touchRight() {
		if (isGrounded()) moveRight();
		else rotateRight();
	}

	public void touchLeft() {
		if (isGrounded()) moveLeft();
		else rotateLeft();
	}
	
	private void handleTouchPlanetMovement() {
		var fingerTouching = Input.touchCount > 0;
		if (!fingerTouching) return; // action can only happen if there is a finger touching screen
	
		var touch = Input.GetTouch (0); // TODO: tag by finger ID

		// Add to the total touch delta
		if (touch.phase == TouchPhase.Moved) {
			curTotalTouchDelta += touch.deltaPosition.x;
		}

		var isDraggingRight = (touch.phase == TouchPhase.Moved && curTotalTouchDelta > MOVE_EPSILON_TOUCH);
		var isDraggingLeft = (touch.phase == TouchPhase.Moved && curTotalTouchDelta < -MOVE_EPSILON_TOUCH);
		var isHolding = (Math.Abs(curTotalTouchDelta) < MOVE_EPSILON_TOUCH);
			
		if (touch.phase == TouchPhase.Began) {
			// Reinit jump force and move delta
			curJumpForce = defaultJumpForce;
			curTotalTouchDelta = 0.0f;
		} else if (touch.phase == TouchPhase.Ended) {
			// Apply jump force
			applyJump(curJumpForce);
		} else if (isDraggingRight) {
			// Touched and dragged right --> move right
			curJumpForce = 0.0f;
			moveRight();
		} else if (isDraggingLeft) {
			// Touched and dragged left --> move left
			curJumpForce = 0.0f;
			moveLeft();
		} else if (isHolding) {
			// Add to jump force
			curJumpForce = Mathf.Min(curJumpForce + jumpFactor, MAX_JUMP_FORCE);
		}
	}
	
	private void moveRight() {
		setTouchPlanetVelocity(true);
	}

	private void moveLeft() {
		setTouchPlanetVelocity(false);
	}
	
	private void setTouchPlanetVelocity(bool moveRight) {
		// flip orientation if we're reversing directions
		if (moveRight && !facingRight || !moveRight && facingRight) flip();
		var moveDirection = moveRight ? 1 : -1;
		var moveVelocity = transform.right * moveDirection * moveSpeed;
		myRigidBody.velocity = moveVelocity;
	}

	private void handleTouchSpaceMovement() {
		var fingerTouching = Input.touchCount > 0;
		if (!fingerTouching) return; // action can only happen if there is a finger touching screen

		var touch = Input.GetTouch(0); // TODO: tag by finger ID

		// Add to the total touch delta
		if (touch.phase == TouchPhase.Moved) {
			curTotalTouchDelta += touch.deltaPosition.x;
		}

		var isDraggingRight = (touch.phase == TouchPhase.Moved && curTotalTouchDelta > MOVE_EPSILON_TOUCH);
		var isDraggingLeft = (touch.phase == TouchPhase.Moved && curTotalTouchDelta < -MOVE_EPSILON_TOUCH);
		var isHolding = (Math.Abs(curTotalTouchDelta) < MOVE_EPSILON_TOUCH);

		if (touch.phase == TouchPhase.Began) {
			// Reinit move delta
			curJumpForce = defaultBoostForce;
			curTotalTouchDelta = 0.0f;
		} else if (touch.phase == TouchPhase.Ended) {
			// Apply jump force
			applyJump(curJumpForce);
		} else if (isDraggingRight) {
			// Touched and dragged right --> move right
			rotateRight();
		} else if (isDraggingLeft) {
			// Touched and dragged left --> move left
			rotateLeft();
		}
	}

	void rotateRight() {
		setTouchSpaceRotation(true);
	}

	void rotateLeft() {
		setTouchSpaceRotation(false);
	}

	void setTouchSpaceRotation(bool rotateRight) {
		var move = rotateRight ? -1 : 1;
		var moving = Math.Abs(move) > MOVE_EPSILON || Math.Abs(move) < -MOVE_EPSILON;
		if (!moving) return;
		
		// negate rotate force to align with movement directions
		myRigidBody.angularVelocity = 0.0f;
		transform.Rotate(0, 0, move * rotateSpeed);
	}
	
	//////////////////// Keyboard Input ////////////////////////
	
	private void handleKeyboardPlanetMovement() {
		var move = Input.GetAxis("Horizontal");
		var moving = Math.Abs(move) > MOVE_EPSILON || Math.Abs(move) < -MOVE_EPSILON;
		myAnimator.SetBool("Running", moving);
		if (!moving) return;
		
		// flip orientation if we're reversing directions
		if (move > 0 && !facingRight || move < 0 && facingRight) flip();

		var velocity = transform.right * move * moveSpeed;
		velocity.z = 0.0f;
		myRigidBody.velocity = velocity;
	}
	
	private void handleKeyboardSpaceMovement() {
		var move = Input.GetAxis("Horizontal");
		var moving = Math.Abs(move) > MOVE_EPSILON || Math.Abs(move) < -MOVE_EPSILON;
		if (!moving) return;
		
		// negate rotate force to align with movement directions
		myRigidBody.angularVelocity = 0.0f;
		transform.Rotate(0, 0, move * -rotateSpeed);
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

	private void applyJump(float jumpForce) {
		myAnimator.SetTrigger("Jump");
		var jumpDirection = transform.up * jumpForce;
		myRigidBody.AddForce(jumpDirection);
		myRigidBody.angularVelocity = 0.0f;
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

	//////////////////// Accessor Methods ////////////////////////

	public float getJumpForce() {
		return curJumpForce;
	}
	
	//////////////////// Setter Methods ////////////////////////

	public void SetMoving(bool value) {
		moving = value;
	}

	public void SetMovingRight(bool value) {
		movingRight = value;
	}
}
