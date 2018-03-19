using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LeftMoveButtonHandler : TouchButtonHandler {
	
	public void onPress() {
		playerController.SetMoving(true);
		playerController.SetMovingRight(false);
	}

	public void onRelease() {
		playerController.SetMoving(false);
	}
}
