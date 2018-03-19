using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelBoundary : MonoBehaviour {
	public Color loadToColor = Color.white;

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.CompareTag ("Player")) {
			string scene = SceneManager.GetActiveScene().name;
			Initiate.Fade(scene, loadToColor, 0.8f);	
//			SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
		}
	}
}
