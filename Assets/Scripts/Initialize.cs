using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Initialize : MonoBehaviour {
	
	// this is called in FindAsteroid.cs
	public void LandOnRandomAsteroid () {
	
		GetComponent<Data>().Load();
		
		
		int id = UnityEngine.Random.Range(1,967);
		string name = "/asteroid/" + id;
		print(name);
		List<string> content = GetComponent<FindAsteroids>().Request(name);
		string info = content[0];
		string [] split = info.Split((char[])null,StringSplitOptions.RemoveEmptyEntries);
		float x = float.Parse(split[1]);
		float y = float.Parse(split[2]);
		float z = float.Parse(split[3]);
		Vector3 pos = new Vector3(x,y+100,z);
		transform.position = pos;
		GameObject.Find("MainCamera").transform.position = pos;
		//ShowBoat(false);
		
		
		
//		Vector3 pos = Birth();
//		transform.position = pos;
//		
//		string label = GetLabel(pos);
//		
//        string nameprefix = "/" + label + "/asteroid";
//		
//		yield return Data.Start(); // wait for data to be loaded
//		List<string> content = Request(nameprefix);
//		
//		if(content == null)
//		{
//			print("Nothing in this octant!");
//		}
//		
//		foreach(string c in content)
//		{
//			RenderAsteroid(c); 
//		}
		
	}
	
	void ShowBoat(bool b)
	{
		transform.Find("FPS").Find("Graphics").Find("boat").gameObject.SetActiveRecursively(b);
		transform.Find("TPS").Find("Graphics").Find("boat").gameObject.SetActiveRecursively(b);
	}
	
	Vector3 Birth()
	{
		// move player to a random legal position
		
		// random point on an egg surface
		float theta = UnityEngine.Random.Range(0f, 3.14f);
		float fi = UnityEngine.Random.Range(-3.14f, 3.14f);
		double x = 4000* 0.78*Mathf.Cos(theta/4)*Mathf.Sin(theta)*Mathf.Sin(fi);
		double y = 4000*1.0*Mathf.Cos(theta);
        double z = 4000* 0.78*Mathf.Cos(theta/4)*Mathf.Sin(theta)*Mathf.Cos(fi);
		
		// rotate the egg
		double xx = x;
		double yy = 0.7071*y - 0.7071*z;
		double zz = 0.7071*y + 0.7071*z;
		
		// translate the egg
		double xxx = xx + 4000;
		double yyy = yy + 4000;
		double zzz = zz + 4000;
		
		Vector3 pos = new Vector3((float)xxx,(float)yyy,(float)zzz);
		
		return pos;
	}
	
}
