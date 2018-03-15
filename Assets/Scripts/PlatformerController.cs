using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlatformerController : MonoBehaviour {

	private const KeyCode JUMP = KeyCode.Space;
	private const float MOVE_EPSILON = 0.001f;

	[SerializeField] private int maxBoosts = 1;
	[SerializeField] private float defaultJumpForce = 500.0f;
	[SerializeField] private float moveSpeed = 5.0f;
	[SerializeField] private float rotateSpeed = 5.0f;
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private float groundDetectRadius = 1.0f;
	[SerializeField] private float groundRayLength = 1.0f;
	[SerializeField] private AudioClip jumpAudio;

	private Rigidbody2D myRigidBody;
	private Animator myAnimator;
	private AudioSource myAudioSource;
	private bool facingRight = true;
	private float curJumpForce = 0.0f;
	private float curTotalTouchDelta = 0.0f;
	private bool moving = false;
	private bool movingRight = true;
	private int boostsRemaining;

	//////////////////// Unity Event Handlers ////////////////////
	
	private void Start () {
		myRigidBody = GetComponent<Rigidbody2D>();
		myAnimator = GetComponent<Animator>();
		myAudioSource = GetComponent<AudioSource>();
		Input.simulateMouseWithTouches = true;
		boostsRemaining = maxBoosts;
	}

	private void Update() {
		if (getInputJump()) applyJump(!isGrounded());
	}
	
	private void FixedUpdate () {
		var grounded = isGrounded();
		if (grounded) boostsRemaining = maxBoosts;
		
		// touch controls (remove animator bool set for non-mobile testing)
		myAnimator.SetBool("Running", grounded && moving);
		if (moving) {
			if (grounded) {
				if (movingRight) moveRight();
				else moveLeft();
			} else {
				if (movingRight) rotateRight();
				else rotateLeft();
			}
		}
		
		// keyboard controls
		if (grounded) handleKeyboardPlanetMovement();
		else handleKeyboardSpaceMovement();
	
		myRigidBody.angularVelocity = 0.0f;
		
		if (getIncomingGround() != null) reorientToLandOn();
	}
	
	//////////////////// Touch Input ////////////////////////
	
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

	void rotateRight() {
		setTouchSpaceRotation(true);
	}

	void rotateLeft() {
		setTouchSpaceRotation(false);
	}

	void setTouchSpaceRotation(bool rotateRight) {
		var move = rotateRight ? -1 : 1;
		// negate rotate force to align with movement directions
		myRigidBody.angularVelocity = 0.0f;
		transform.Rotate(0, 0, move * rotateSpeed);
	}
	
	//////////////////// Keyboard Input ////////////////////////
	
	private void handleKeyboardPlanetMovement() {
		var move = Input.GetAxis("Horizontal");
		var isMoving = Math.Abs(move) > MOVE_EPSILON || Math.Abs(move) < -MOVE_EPSILON;
		if (!isMoving) return;
		
		// flip orientation if we're reversing directions
		if (move > 0 && !facingRight || move < 0 && facingRight) flip();

		var velocity = transform.right * move * moveSpeed;
		velocity.z = 0.0f;
		myRigidBody.velocity = velocity;
	}
	
	private void handleKeyboardSpaceMovement() {
		var move = Input.GetAxis("Horizontal");
		var isMoving = Math.Abs(move) > MOVE_EPSILON || Math.Abs(move) < -MOVE_EPSILON;
		if (!isMoving) return;
		
		// negate rotate force to align with movement directions
		myRigidBody.angularVelocity = 0.0f;
		transform.Rotate(0, 0, move * -rotateSpeed);
	}
	
	//////////////////// Helper Methods ////////////////////////
	
	private bool getInputJump() {
		// check keyboard input
		if (Input.GetKeyDown(JUMP)) return true;
		
		// check if there's at least one finger touching
		for (var i = 0; i < Input.touchCount; i++) {
			var touchingGuiElement = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId);
			if (Input.GetTouch(i).phase == TouchPhase.Began && !touchingGuiElement) {
				return true;
			}
		}
		return false;
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

	private void applyJump(bool isBoost) {
		if (!isBoost) myAudioSource.PlayOneShot(jumpAudio);
		else if (boostsRemaining <= 0) return;
		else boostsRemaining--;
		myAnimator.SetTrigger("Jump");
		var jumpDirection = transform.up * defaultJumpForce;
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

	//////////////////// Setter Methods ////////////////////////

	public void SetMoving(bool value) {
		moving = value;
	}

	public void SetMovingRight(bool value) {
		movingRight = value;
	}
}
