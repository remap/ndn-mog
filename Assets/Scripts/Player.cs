using UnityEngine;
using System.Collections;
using System;


public class Player : MonoBehaviour {
	
	public Vector2 scrollPosition = Vector2.zero;
	public static float radius = 100;
	public Vector3 player_pos = new Vector3(0, 0, 0);
	
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
		player_pos = new Vector3(pos_x, pos_y, pos_z);
		transform.position = player_pos;
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
		GUILayout.Label("Number of Players: " + count);
		GUILayout.Label ("List of Players: ");
		GUILayout.Label (AssetSync.me + ": " + player_pos.x + ", " + player_pos.y + ", " + player_pos.z + " (this is me)");
		ICollection keys = AssetSync.Others.Keys;
		IEnumerator kinum = keys.GetEnumerator();
		ICollection values = AssetSync.Others.Values;
		IEnumerator vinum = values.GetEnumerator();
		while(kinum.MoveNext() && vinum.MoveNext())
		{
			GUILayout.Label("" + kinum.Current + ": " + vinum.Current);
		}
		 GUI.EndScrollView();
		
	}
	

}
