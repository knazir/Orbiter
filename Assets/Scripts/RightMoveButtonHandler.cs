using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightMoveButtonHandler : TouchButtonHandler {
	
	public void onPress() {
		buttonPressed = true;
		playerController.SetMoving(true);
		playerController.SetMovingRight(true);
	}

	public void onRelease() {
		buttonPressed = false;
		playerController.SetMoving(false);
	}
}
