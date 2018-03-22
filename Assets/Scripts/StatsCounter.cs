using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsCounter : MonoBehaviour {

	public SimpleHealthBar boostBar;
	public Text starScoreText;
	
	private const int MAX_BOOSTS = 10;
	
	[SerializeField] private int defaultBoost = 1;
	
	private int extraBoosts = 0; // Extra boosts are collected
	private int starScore = 0;

	private void Awake() {
		boostBar = GameObject.FindGameObjectWithTag("BoostBar").GetComponent<SimpleHealthBar>();
		starScoreText = GameObject.FindGameObjectWithTag("StarScore").GetComponent<Text>();
		updateBoostBar();
	}

	private void OnTriggerEnter2D(Collider2D col) {
		if (col.gameObject.CompareTag (Constants.BOOSTER)) {
			Destroy (col.gameObject);
			extraBoosts++;
			updateBoostBar ();
		} else if (col.gameObject.CompareTag (Constants.STAR)) { 
			Destroy (col.gameObject);
			starScore++;
			updateStarScoreText ();
		}
	}
	
	//////////// Helper Methods /////////////
	
	public void replenishDefaultBoost() {
		defaultBoost = 1;
		updateBoostBar();
	}

	public void useBoost() {
		if (!canUseBoost()) return;
		if (defaultBoost != 0) defaultBoost--;
		else extraBoosts--;
		updateBoostBar();
	}
		
	public bool canUseBoost() {
		return defaultBoost > 0 || extraBoosts > 0;
	}

	private void updateBoostBar(){
		boostBar.UpdateBar(defaultBoost + extraBoosts, MAX_BOOSTS);
	}

	private void updateStarScoreText(){
		starScoreText.text = "" + starScore;
	}
}
