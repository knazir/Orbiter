using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LeftMoveButtonHandler : TouchButtonHandler {
	
	public void onPress() {
		if (ButtonPressed) return;
		ButtonPressed = true;
		playerController.SetMoving(true);
		playerController.SetMovingRight(false);
	}

	public void onRelease() {
		ButtonPressed = false;
		playerController.SetMoving(false);
	}
}
