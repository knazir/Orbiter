using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CometParticleSystem : MonoBehaviour {
	private void Start () {
		GetComponent<Renderer>().sortingLayerName = Constants.SORTING_LAYER_EFFECTS;
	}
}
