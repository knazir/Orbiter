using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightMoveButtonHandler : TouchButtonHandler {
	
	public void onPress() {
		ButtonPressed = true;
		playerController.SetMoving(true);
		playerController.SetMovingRight(true);
	}

	public void onRelease() {
		ButtonPressed = false;
		playerController.SetMoving(false);
	}
}
