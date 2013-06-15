using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Newtonsoft.Json;

public class Initialize : MonoBehaviour {
	
	// this is called in FindAsteroid.cs
	public void LandOnRandomAsteroid () {
	
		GetComponent<Data>().Load();
		
		
		int id = UnityEngine.Random.Range(1,967);
		string name = "/ndn/ucla.edu/apps/matryoshka/asteroid/octant";
		print(name);
		
		IntPtr ccn = Egal.GetHandle(); // connect to ccnd
		Egal.ExpressInterest(ccn, name, RequestCallback, IntPtr.Zero); // express interest
		Egal.ccnRun(ccn, -1); // ccnRun starts a new thread
		
		name = "/asteroid/" + id;
		List<string> content = GetComponent<FindAsteroids>().Request(name);
		
		string info = content[0];
		string [] split = info.Split((char[])null,StringSplitOptions.RemoveEmptyEntries);
		float x = float.Parse(split[1]);
		float y = float.Parse(split[2]);
		float z = float.Parse(split[3]);
		Vector3 pos = new Vector3(x,y+100,z);
		transform.position = pos;
		GameObject.Find("MainCamera").transform.position = pos;
		
		
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
				string content = Egal.GetContentValue(Info.content_ccnb, Info.pco); // it's a json object
				print("content value: " + content);
				Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
//				foreach(string k in values.Keys)
//					print(k);
			
				// get content name
				string contentname = Egal.GetContentName(Info.content_ccnb);
				print("content name: " + contentname);
			
				break;
			
			case Upcall.ccn_upcall_kind.CCN_UPCALL_FINAL:
				Egal.ccn_set_run_timeout(h, 0); 
				Egal.killCurrentThread(); // kill current thread
				break;
		}
		return Upcall.ccn_upcall_res.CCN_UPCALL_RESULT_OK;
	}
	
}
