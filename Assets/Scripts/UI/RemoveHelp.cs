using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RemoveHelp : MonoBehaviour {
	void OnTriggerEnter2D(Collider2D col) {
		if (col.gameObject.tag == Constants.PLAYER) {
			Destroy (gameObject);
		}
	}
}
