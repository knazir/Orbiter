using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelBoundary : MonoBehaviour {
	private void OnTriggerEnter2D(Collider2D other) {
		if (!other.CompareTag("Player")) return;
		var scene = SceneManager.GetActiveScene().name;
		Initiate.Fade(scene, Color.black, 0.8f);
	}
}
