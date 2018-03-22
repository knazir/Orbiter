using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsCounter : MonoBehaviour {
	
	private const int MAX_BOOSTS = 10;
	
	[SerializeField] private int defaultBoosts = 1;
	[SerializeField] private Text starScoreText;
	[SerializeField] private Text boostCountText;
	[SerializeField] private AudioClip starPickup;
	[SerializeField] private AudioClip boostPickup;

	private AudioSource audioSource;
	private int starScore = 0;
	private int currentBoosts;

	private void Awake() {
		currentBoosts = defaultBoosts;
		updateBoostCount();
	}

	private void Start() {
		audioSource = GetComponent<AudioSource>();
	}

	private void OnTriggerEnter2D(Collider2D col) {
		if (col.gameObject.CompareTag(Constants.BOOSTER)) {
			playSound(boostPickup);
			Destroy(col.gameObject);
			currentBoosts++;
			updateBoostCount();
		} else if (col.gameObject.CompareTag(Constants.STAR)) { 
			playSound(starPickup);
			Destroy(col.gameObject);
			starScore++;
			updateStarScoreText();
		}
	}

	private void playSound(AudioClip sound) {
		if (sound == null) return;
		audioSource.PlayOneShot(sound);
	}
	
	//////////// Helper Methods /////////////
	
	public void replenishDefaultBoost() {
		if (currentBoosts < defaultBoosts) currentBoosts += defaultBoosts;
		updateBoostCount();
	}

	public void useBoost() {
		if (!canUseBoost()) return;
		currentBoosts--;
		updateBoostCount();
	}
		
	public bool canUseBoost() {
		return currentBoosts > 0;
	}

	private void updateBoostCount() {
		if (boostCountText == null) return;
		boostCountText.text = "" + currentBoosts;
	}

	private void updateStarScoreText() {
		if (starScoreText == null) return;
		starScoreText.text = "" + starScore;
	}

	public int getStarScore() {
		return starScore;
	}
}
