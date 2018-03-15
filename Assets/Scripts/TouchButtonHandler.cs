using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchButtonHandler : MonoBehaviour {
	
	protected PlatformerController playerController;

	private void Start () {
		playerController = FindObjectOfType<PlatformerController>();
	}
}
