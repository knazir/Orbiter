using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour {

	[SerializeField] private GameObject objectToSpawn;
	[SerializeField] private Camera mainCamera;
	
	private void FixedUpdate () {
		if (Input.GetMouseButtonDown(0)) {
			spawnObject();
		}
	}

	private void spawnObject() {
		var spawnPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
		spawnPosition.z = 0.0f;
		Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
	}
}
