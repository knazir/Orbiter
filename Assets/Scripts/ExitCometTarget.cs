using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitCometTarget : MonoBehaviour {
    private void OnTriggerEnter2D(Collider2D other) {
        Debug.Log("TRIGGERED");
        if (other.CompareTag(Constants.PLAYER)) transform.parent.gameObject.GetComponent<CometExit>().FlyAway();
    }
}
