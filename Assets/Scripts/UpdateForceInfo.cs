using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateForceInfo : MonoBehaviour {

	private const string JUMP_STD = "Jump force: ";

	public PlatformerController characterController;
	private Text forceInfo;

	// Use this for initialization
	void Start () {
		forceInfo = GetComponent<Text> ();	
	}
	
	// Update is called once per frame
	void Update () {
		forceInfo.text = JUMP_STD + characterController.getJumpForce ();
	}
}
