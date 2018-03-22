using System.Collections;
using System.Collections.Generic;
using Exploder2D;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BlowUpPlayer : MonoBehaviour {

	private const float FADE_DELAY = 1.0f;

	private Exploder2DObject exploder;
	
	private void Start() {
		exploder = Exploder2D.Utils.Exploder2DSingleton.Exploder2DInstance;
	}
	
	private void OnTriggerEnter2D(Collider2D other) {
		if (!other.CompareTag("Player")) return;
		FindObjectOfType<CameraController>().StopFollowingPlayer();
		other.gameObject.tag = Constants.EXPLODER_2D;
		Exploder2DUtils.SetActive(exploder.gameObject, true);
		exploder.transform.position = Exploder2DUtils.GetCentroid(other.gameObject);
		exploder.Radius = 1.0f;
		exploder.Force = 5.0f;
		exploder.TargetFragments = 50;
		exploder.Explode();
		GetComponent<AudioSource>().Play();
		Invoke("fadeOut", FADE_DELAY);
	}

	private void fadeOut() {
		GameObject.Find("Character").tag = Constants.PLAYER;
		var scene = SceneManager.GetActiveScene().name;
		Initiate.Fade(scene, Color.black, 0.8f);
	}
}
