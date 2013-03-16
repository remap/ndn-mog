using UnityEngine;
using System.Collections;
using System;

public class GetAsteroidNames : MonoBehaviour {

	void Start () {
	
		IntPtr ccn = CCNScript.GetHandle();
		IntPtr name = Egal.ccn_charbuf_create();
		Egal.ccn_name_from_uri(name, "ccnx:/ndn/ucla.edu/airports/%C1.E.be");
		EnumerateNames(ccn, name, IntPtr.Zero);
		Egal.ccn_run(ccn, 500);
		Egal.ccn_destroy(ref ccn);
	}
	
	int EnumerateNames(IntPtr ccn, IntPtr nm, IntPtr templ)
	{
		print("Enumerate Names.");
		return 0;
	}
}
