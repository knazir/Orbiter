using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchButtonHandler : MonoBehaviour {

	public static bool ButtonPressed = false;
	
	protected PlatformerController playerController;

	private void Start () {
		playerController = FindObjectOfType<PlatformerController>();
	}
}
