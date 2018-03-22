using UnityEngine;
using System.Collections;

public class Score : MonoBehaviour
{
	public int score = 0;					// The player's score.

	void Awake ()
	{
	}

	void Update ()
	{
		// Set the score text.
		GetComponent<GUIText>().text = "Exploder 2D: " + score;
	}
}
