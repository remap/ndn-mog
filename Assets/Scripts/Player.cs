using UnityEngine;
using System.Collections;
using System;


public class Player : MonoBehaviour {
	
	public Vector2 scrollPosition = Vector2.zero;
	public static float radius = 100;
	
	void Start()
	{
		SetPlayerPosition();
		WritePlayerToRepo();
	}
	
	void SetPlayerPosition()
	{
		//GameObject player = GameObject.Find("Player");
		float theta = UnityEngine.Random.Range(0, 360f);
		float pos_x = radius * Mathf.Sin(theta);
		float pos_y = 0;
		float pos_z = radius * Mathf.Cos(theta);
		Vector3 pos = new Vector3(pos_x, pos_y, pos_z);
		transform.position = pos;
	}
	
	void WritePlayerToRepo()
	{
		System.String name = AssetSync.prefix + "/players/" + UnityEngine.Random.Range(0, 9999);
		Vector3 pos = transform.position;
		System.String content = "" + pos.x + "," + pos.y + "," + pos.z;
		AssetSync.me = name;
		AssetSync assetsync = new AssetSync();
		assetsync.WriteToRepo(name, content);
	}
	
	public GameObject sphere;
	public void AddNewPlayer(String content)
	{
		print("Add New Player." + content);
		// instantiate a white sphere in the location indicated by "content"
		GameObject p = sphere;
		string [] split = content.Split(new Char [] {','});
		Vector3 pos = new Vector3(Single.Parse(split[0]), Single.Parse(split[1]), Single.Parse(split[2]));
		//GameObject NewPlayer = Instantiate(p, pos, p.transform.rotation) as GameObject;
		//NewPlayer.name = "";
	}
		
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
