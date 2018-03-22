using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCompleteManager : MonoBehaviour {

	[SerializeField] private GameObject[] stars;
	[SerializeField] private StatsCounter statsCounter;

	public void ShowStars() {
		var numStars = statsCounter.getStarScore();
		for (var i = 0; i < Mathf.Min(numStars, stars.Length); i++) {
			stars[i].SetActive(true);
		}
	}

}
