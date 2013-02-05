using UnityEngine;
using System.Collections;
using System;


public class Player : MonoBehaviour {
	
	
	public static float radius = 100;
	public static Vector3 player_pos = new Vector3(0, 0, 0);
	
	public static string me = "";
	public static Hashtable PlayerList = new Hashtable(); 
	public static Hashtable ObjList = new Hashtable();
		
	void Start()
	{
		SetPlayerPosition();
		WritePlayerToRepo();
		InvokeRepeating("PollPlayerList", 0f, 0.2f); // player discovery, 5 per sec
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
		transform.LookAt(new Vector3(0,0,0));
	}
	
	void WritePlayerToRepo()
	{
		System.String name = AssetSync.prefix + "/players/" + UnityEngine.Random.Range(0, 9999);
		Vector3 pos = transform.position;
		System.String content = "" + pos.x + "," + pos.y + "," + pos.z;
		me = name;
		AssetSync assetsync = new AssetSync();
		assetsync.WriteToRepo(name, content); // publish my existence
		PlayerList.Add(name,content); // add myself to the player list
	}
	
	public void WriteObjToRepo()
	{
		
	}
	
	void PollPlayerList()
	{
		Hashtable bufferclone = new Hashtable();
		print (AssetSync.Buffer);
		if(AssetSync.Buffer.Count != 0) // there is sth to read at AssetSync
		{
			lock(AssetSync.lc)
			{
				bufferclone = (Hashtable)AssetSync.Buffer.Clone(); // clone the buffer
				AssetSync.Buffer.Clear(); // empty it
			}
			ProcessIncomingAssets(bufferclone);
		}
	}
	
	// sort buffer content into PlayerList, NpcList, ObjList 
	void ProcessIncomingAssets(Hashtable buffer)
	{
		ICollection keys = buffer.Keys;
		IEnumerator kinum = keys.GetEnumerator();
		ICollection values = buffer.Values;
		IEnumerator vinum = values.GetEnumerator();
		while(kinum.MoveNext() && vinum.MoveNext())
		{
			string AssetName = kinum.Current.ToString();
			string PartialName = AssetName.Remove(0, AssetSync.prefix.Length);
			if(PartialName.StartsWith("/players"))
			{
				if(PlayerList.ContainsKey(kinum.Current))
					{print ("Duplicated player detected.");}
				else
				{
					PlayerList.Add (kinum.Current, vinum.Current); // full name in the list
					RenderNewPlayer(PartialName, vinum.Current.ToString());
				}
			}
			else if(PartialName.StartsWith("/objects"))
			{
				// 
			}
			
		}
	}
	
	public GameObject sphere;
	public void RenderNewPlayer(string partialname, string content)
	{
		print("Add New Player." + content);
		// instantiate a white sphere in the location indicated by "content"
		GameObject p = GameObject.Find("/playermodel");
		string [] split = content.Split(new Char [] {','});
		Vector3 pos = new Vector3(Single.Parse(split[0]), Single.Parse(split[1]), Single.Parse(split[2]));
		GameObject NewPlayer = Instantiate(p, pos, p.transform.rotation) as GameObject;
		NewPlayer.name = partialname;
		NewPlayer.renderer.enabled = true;
	}
		
	
	

}
