using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchButtonHandler : MonoBehaviour {

	protected static bool buttonPressed = false;
	
	protected PlatformerController playerController;

	private void Start () {
		playerController = FindObjectOfType<PlatformerController>();
	}

	public bool ButtonPressed() {
		return buttonPressed;
	}
}
