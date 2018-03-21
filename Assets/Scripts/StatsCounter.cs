﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsCounter : MonoBehaviour {

	public SimpleHealthBar boostBar;

	private const int MAX_BOOSTS = 10;
	[SerializeField] private int defaultBoost = 1;
	private int extraBoosts = 0; // Extra boosts are collected

	void Awake() {
		boostBar = GameObject.Find ("/UI/Boost Bar/Boost Bar").GetComponent<SimpleHealthBar> ();
		updateBoostBar ();
	}

	void OnTriggerEnter2D(Collider2D col) {
		if (col.gameObject.tag == Constants.BOOSTER) {
			Destroy (col.gameObject);

			extraBoosts++;
			updateBoostBar ();
		}
	}

	//////////// Helper Methods /////////////
	public void replenishDefaultBoost() {
		defaultBoost = 1;
		updateBoostBar ();
	}

	public void useBoost() {
		if (!canUseBoost())
			return;

		if (defaultBoost != 0)
			defaultBoost--;
		else
			extraBoosts--;
		updateBoostBar ();
	}
		
	public bool canUseBoost() {
		return defaultBoost > 0 || extraBoosts > 0;
	}

	private void updateBoostBar(){
		boostBar.UpdateBar (defaultBoost + extraBoosts, MAX_BOOSTS);
	}
}