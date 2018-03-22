using System.Collections;
using System.Collections.Generic;
using Exploder2D;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RemoveHelp : MonoBehaviour {

	private const float DESTROY_DELAY = 1.0f;
	
	private Exploder2DObject exploder;

	private void Start() {
		exploder = Exploder2D.Utils.Exploder2DSingleton.Exploder2DInstance;
	}
	
	private void OnTriggerEnter2D(Collider2D col) {
		if (col.gameObject.CompareTag(Constants.PLAYER)) {
			explode();
			Invoke("destroy", DESTROY_DELAY);
		}
	}

	private void explode() {
		Exploder2DUtils.SetActive(exploder.gameObject, true);
		exploder.Radius = 0.1f;
		exploder.Force = 1.0f;
		exploder.TargetFragments = 50;
		exploder.transform.position = Exploder2DUtils.GetCentroid(gameObject);
		exploder.Explode();
	}

	private void destroy() {
		Destroy(gameObject);
	}
	
}
