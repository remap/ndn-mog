using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Data : MonoBehaviour {
	
	public static bool ready = false;
	public static Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();
	
	// this is called in Initialize.cs
	public bool Load () {
		
		string line;
		StreamReader file = new StreamReader("data.txt");
		while((line = file.ReadLine()) != null)
		{
   			//print(line);
			string [] split = line.Split(new char [] {','});
			
			string key = split[0];
			string singlevalue = split[1];
			
			if(data.ContainsKey(key))
			{
				data[key].Add(singlevalue);
			}
			else
			{
				data.Add(key,new List<string>());
				data[key].Add(singlevalue);
			}
		}

		file.Close();
		
//		foreach(string k in data.Keys)
//			print(k);
		
		
		return true;
		
	}
	
}
