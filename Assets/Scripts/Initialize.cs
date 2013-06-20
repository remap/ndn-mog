using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Newtonsoft.Json;

public class Initialize : MonoBehaviour {
	
	public static bool finished = false;
	public static string FirstAsteroid = "";
	public static string FirstAsteroidName;
	
	public IEnumerator Start () {
	
		string name = "/ndn/ucla.edu/apps/matryoshka/asteroid/octant";
		IntPtr ccn = Egal.GetHandle(); // connect to ccnd
		Egal.ExpressInterest(ccn, name, RequestCallback, IntPtr.Zero, IntPtr.Zero); // express interest
		Egal.ccnRun(ccn, -1); // ccnRun starts a new thread
		
		while(FirstAsteroid == "")
		{
			yield return new WaitForSeconds(0.05f);
		}
		LandOnFirstAsteroid();  // create the 1st asteroid based on received data
								// & put the doll on this asteroid
		
		finished = true;
	}
	
	void LandOnFirstAsteroid()
	{
		string n = M.GetLabelFromName(FirstAsteroidName);
		Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(FirstAsteroid);
		string id = values["fs"];
		if(FindAsteroids.asteroidDic.ContainsKey(n)==true)
		{
			if(FindAsteroids.asteroidDic[n].Contains(id))
			{
				print("Initialize failed!");
				return;
			}
		}
		Vector3 pos = M.GetGameCoordinates(values["latitude"], values["longitude"]);
		MakeAnAsteroid(pos, values["fs"]);
		if(FindAsteroids.asteroidDic.ContainsKey(n)==false)
		{
			FindAsteroids.asteroidDic.Add (n,new List<string>());
		}
		FindAsteroids.asteroidDic[n].Add(id);
		
		
		Vector3 dollpos = pos + new Vector3(0, 95, 0);
		transform.position = dollpos;
		GameObject.Find("MainCamera").transform.position = dollpos;
		
	}
	
	public static void MakeAnAsteroid(Vector3 position, string id)
	{
		// instantiate an asteroid
		
		GameObject asteroid1 = GameObject.Find("tree2");
		GameObject newAsteroid = UnityEngine.Object.Instantiate(asteroid1, position, Quaternion.identity) as GameObject;
		newAsteroid.name = "asteroid-"+id;
		newAsteroid.transform.localScale = new Vector3(1000f,1000f,1000f);
		
		
	}
	
	
	static Upcall.ccn_upcall_res RequestCallback (IntPtr selfp, Upcall.ccn_upcall_kind kind, IntPtr info)
	{
		// this will be ran by a NON-Unity thread
		
		print("RequestCallback: " + kind);
		Egal.ccn_upcall_info Info = Egal.GetInfo(info);
		IntPtr h=Info.h;
		
		switch (kind) {
			case Upcall.ccn_upcall_kind.CCN_UPCALL_CONTENT_UNVERIFIED:
        	case Upcall.ccn_upcall_kind.CCN_UPCALL_CONTENT:
				FirstAsteroidName = Egal.GetContentName(Info.content_ccnb);
				FirstAsteroid = Egal.GetContentValue(Info.content_ccnb, Info.pco); 
				break;
			
			case Upcall.ccn_upcall_kind.CCN_UPCALL_FINAL:
				Egal.ccn_set_run_timeout(h, 0); 
				Egal.killCurrentThread(); // kill current thread
				break;
		}
		return Upcall.ccn_upcall_res.CCN_UPCALL_RESULT_OK;
	}
	
	
	

	
}
