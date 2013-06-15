using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Newtonsoft.Json;

public class Initialize : MonoBehaviour {
	
	public static string FirstAsteroid = "";
	public static string FirstAsteroidName;
	
	public IEnumerator Start () {
	
		string name = "/ndn/ucla.edu/apps/matryoshka/asteroid/octant";
		IntPtr ccn = Egal.GetHandle(); // connect to ccnd
		Egal.ExpressInterest(ccn, name, RequestCallback, IntPtr.Zero); // express interest
		Egal.ccnRun(ccn, -1); // ccnRun starts a new thread
		
		while(FirstAsteroid == "")
		{
			yield return new WaitForSeconds(0.05f);
		}
		LandOnFirstAsteroid();  // create the 1st asteroid based on received data
								// & put the doll on this asteroid
		
	}
	
	void LandOnFirstAsteroid()
	{
		Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(FirstAsteroid);
		Vector3 pos = GetGameCoordinates(values["latitude"], values["longitude"]);
		MakeAnAsteroid(pos, values["fs"]);
		
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
		newAsteroid.transform.localScale = new Vector3(2000f,2000f,2000f);
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
	
	
	public static Vector3 GetGameCoordinates(string str_lati, string str_longi)
	{
		// convert from latitude and longitude to game coordinates
		
		float latitude = Convert.ToSingle( str_lati );
		float longitude = Convert.ToSingle( str_longi ) ;
				
		float theta = (float)(3.14*(1.0/2.0 + latitude/180));
		float fi = (float)(3.14*(longitude/180));
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
