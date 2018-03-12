using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftMoveButtonHandler : MonoBehaviour {
	private PlatformerController playerController;

	private void Start() {
		playerController = FindObjectOfType<PlatformerController>();
	}
	
	public void onPress() {
		playerController.SetMoving(true);
		playerController.SetMovingRight(false);
	}

	public void onRelease() {
		playerController.SetMoving(false);
	}
}
