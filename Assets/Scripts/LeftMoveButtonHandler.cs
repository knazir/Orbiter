using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftMoveButtonHandler : TouchButtonHandler {
	
	public void onPress() {
		if (buttonPressed) return;
		buttonPressed = true;
		playerController.SetMoving(true);
		playerController.SetMovingRight(false);
	}

	public void onRelease() {
		buttonPressed = false;
		playerController.SetMoving(false);
	}
}
