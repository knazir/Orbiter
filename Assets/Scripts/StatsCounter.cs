using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsCounter : MonoBehaviour {

	[SerializeField] private int defaultBoost = 1;
	private int extraBoosts = 0; // Extra boosts are collected

//	// Use this for initialization
//	void Start () {
//		
//	}
//	
//	// Update is called once per frame
//	void Update () {
//		
//	}

	void OnTriggerEnter2D(Collider2D col) {
		if (col.gameObject.name == Constants.ROCKET) {
			extraBoosts++;
			Destroy (col.gameObject);
		}
	}

	//////////// Helper Methods /////////////
	public void replenishDefaultBoost() {
		defaultBoost = 1;
	}

	public void useBoost() {
		if (!canUseBoost())
			return;

		if (defaultBoost != 0)
			defaultBoost--;
		else
			extraBoosts--;
	}
		
	public bool canUseBoost() {
		return defaultBoost > 0 || extraBoosts > 0;
	}
}
