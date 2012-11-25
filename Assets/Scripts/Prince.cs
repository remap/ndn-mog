using UnityEngine;
using System.Collections;
using System;

public class Prince : MonoBehaviour {

	void OnGUI(){
		
		GUILayout.Label ("My name is: "+AssetSync.me);
		int count = AssetSync.Others.Count+1;
		GUILayout.Label ("The current number of players (including myself) in the game is: " + count);
		GUILayout.Label ("The other players' names are:");
		
		
	}
}
