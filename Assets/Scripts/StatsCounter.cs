using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsCounter : MonoBehaviour {
	
	private const int MAX_BOOSTS = 10;
	
	[SerializeField] private int defaultBoost = 1;
	[SerializeField] private Text starScoreText;
	[SerializeField] private Text boostCountText;
	[SerializeField] private AudioClip starPickup;
	[SerializeField] private AudioClip boostPickup;

	private AudioSource audioSource;
	private int extraBoosts = 0; // Extra boosts are collected
	private int starScore = 0;

	private void Start() {
		audioSource = GetComponent<AudioSource>();
	}

	private void OnTriggerEnter2D(Collider2D col) {
		if (col.gameObject.CompareTag(Constants.BOOSTER)) {
			playSound(boostPickup);
			Destroy(col.gameObject);
			extraBoosts++;
			updateBoostCount();
		} else if (col.gameObject.CompareTag(Constants.STAR)) { 
			playSound(starPickup);
			Destroy(col.gameObject);
			starScore++;
			updateStarScoreText();
		}
	}

	private void playSound(AudioClip sound) {
		audioSource.PlayOneShot(sound);
	}
	
	//////////// Helper Methods /////////////
	
	public void replenishDefaultBoost() {
		defaultBoost = 1;
		updateBoostCount();
	}

	public void useBoost() {
		if (!canUseBoost()) return;
		if (defaultBoost != 0) defaultBoost--;
		else extraBoosts--;
		updateBoostCount();
	}
		
	public bool canUseBoost() {
		return defaultBoost > 0 || extraBoosts > 0;
	}

	private void updateBoostCount(){
		boostCountText.text = "" + (defaultBoost + extraBoosts);
	}

	private void updateStarScoreText(){
		starScoreText.text = "" + starScore;
	}

	public int getStarScore() {
		return starScore;
	}
}
