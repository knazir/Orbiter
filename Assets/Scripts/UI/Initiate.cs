using UnityEngine;
using System.Collections;

public static class Initiate {
    //Create Fader object and assing the fade scripts and assign all the variables
    public static void Fade (string scene, Color col, float damp){
	    var init = new GameObject {name = "Fader"};
	    init.AddComponent<Fader> ();
		var scr = init.GetComponent<Fader> ();
		scr.fadeDamp = damp;
		scr.fadeScene = scene;
		scr.fadeColor = col;
		scr.start = true;
	}
}
