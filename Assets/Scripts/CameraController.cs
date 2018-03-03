using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	[SerializeField] private Transform player;

	private Vector3 offset;

	private void Start() {
		transform.position = new Vector3(player.position.x, player.position.y, transform.position.z);
		
		// TODO: Figure out if we want some kind of offset (redundant for now)
		offset = transform.position - player.position;
	}
	
	// Called after each Update every frame
	private void LateUpdate() {
		transform.position = player.transform.position + offset;
	}
}
