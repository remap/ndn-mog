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
	public static string FirstAsteroidLabel;
	
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
		
		Vector3 pos = FindAsteroids.MakeAnAsteroid(FirstAsteroid);
		FindAsteroids.AddToDic(FirstAsteroidName);
		
		Vector3 dollpos = pos + new Vector3(0, 50, 0);
		transform.position = dollpos;
		GameObject.Find("MainCamera").transform.position = dollpos;
		
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
			FirstAsteroidLabel = M.GetLabelFromName(FirstAsteroidName);
			break;
		case Upcall.ccn_upcall_kind.CCN_UPCALL_INTEREST_TIMED_OUT:
			print("Initialize: request first asteroid interest timed out.");
			break;
		case Upcall.ccn_upcall_kind.CCN_UPCALL_FINAL:
			Egal.ccn_set_run_timeout(h, 0); 
			Egal.killCurrentThread(); // kill current thread
			break;
		}
		return Upcall.ccn_upcall_res.CCN_UPCALL_RESULT_OK;
	}
	
	
	

	
}
