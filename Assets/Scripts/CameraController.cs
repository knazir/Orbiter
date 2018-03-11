using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	[SerializeField] private Transform player;
	[SerializeField] private float zoomInSize = 10.0f;
	[SerializeField] private float zoomOutSize = 20.0f;

	private bool followPlayer;
	private Vector3 offset;

	public void FixPosition(Transform fixedTransform) {
		followPlayer = false;
		transform.position = new Vector3(fixedTransform.position.x, fixedTransform.position.y, transform.position.z);
	}

	public void FollowPlayer() {
		followPlayer = true;
	}

	public void ZoomOut() {
		Camera.main.orthographicSize = zoomOutSize;
	}

	public void ZoomIn() {
		Camera.main.orthographicSize = zoomInSize;
	}

	private void Start() {
		followPlayer = true;
		transform.position = new Vector3(player.position.x, player.position.y, transform.position.z);
		
		// TODO: Figure out if we want some kind of offset (redundant for now)
		offset = transform.position - player.position;
	}
	

	private void Update() {		

	}
	
	// Called after each Update every frame
	private void LateUpdate() {
		if (followPlayer) transform.position = player.transform.position + offset;
	}
}
