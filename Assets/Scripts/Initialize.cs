using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

public class Initialize : MonoBehaviour {
	
	// this is called in FindAsteroid.cs
	public void LandOnRandomAsteroid () {
	
		GetComponent<Data>().Load();
		
		
		int id = UnityEngine.Random.Range(1,967);
		string name = "/asteroid/" + id;
		print(name);
		
		IntPtr ccn = CCNScript.GetHandle(); // connect to ccnd
		CCNScript.ExpressInterest(ccn, name, RequestCallback, IntPtr.Zero); // express interest
		CCNScript.ccnRun(ccn, -1); // ccnRun starts a new thread
		
		
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
		print("Request Callback");
		
		Egal.ccn_upcall_info Info = new Egal.ccn_upcall_info();
		Info = (Egal.ccn_upcall_info)Marshal.PtrToStructure(info, typeof(Egal.ccn_upcall_info));
		IntPtr h=Info.h;
		
		switch (kind) {
        	case Upcall.ccn_upcall_kind.CCN_UPCALL_CONTENT:
			
			case Upcall.ccn_upcall_kind.CCN_UPCALL_FINAL:
				Egal.ccn_set_run_timeout(h, 0); 
				CCNScript.killCurrentThread(); // kill current thread
			 	
				break;
		}
		return Upcall.ccn_upcall_res.CCN_UPCALL_RESULT_OK;
	}
	
}
