using UnityEngine;
using System.Collections;
using System;
using System.Collections;

public class Prince : MonoBehaviour {
	
	 public Vector2 scrollPosition = Vector2.zero;
	void OnGUI(){
		int count = AssetSync.Others.Count+1;
		
		scrollPosition = GUI.BeginScrollView(new Rect(5, 5, Screen.width-5, Screen.height-5), scrollPosition, new Rect(0, 0, Screen.width, Screen.height*count/10));
		GUILayout.Label ("My name is: "+AssetSync.me);
		GUILayout.Label ("The current number of players (including myself) in the game is: " + count);
		GUILayout.Label ("The other players' names are: ");
		ICollection keys = AssetSync.Others.Keys;
		IEnumerator inum = keys.GetEnumerator();
		while(inum.MoveNext())
		{
			GUILayout.Label("" + inum.Current);
		}
		 GUI.EndScrollView();
		
	}
}
