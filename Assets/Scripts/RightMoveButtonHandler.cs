using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightMoveButtonHandler : TouchButtonHandler {
	
	public void onPress() {
		playerController.SetMoving(true);
		playerController.SetMovingRight(true);
	}

	public void onRelease() {
		playerController.SetMoving(false);
	}
}
