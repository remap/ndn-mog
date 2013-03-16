using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

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
		Egal.ccn_closure Action = new Egal.ccn_closure(ENCallback, IntPtr.Zero, 0);
		IntPtr pnt = Marshal.AllocHGlobal(Marshal.SizeOf(Action));
		Marshal.StructureToPtr(Action, pnt, true);
		
		Egal.ccn_express_interest(ccn, nm, pnt, templ);
		return 0;
	}
	
	Upcall.ccn_upcall_res ENCallback(IntPtr selfp, Upcall.ccn_upcall_kind kind, IntPtr info)
	{
		print("Name Enumeration Callback!");
		return Upcall.ccn_upcall_res.CCN_UPCALL_RESULT_OK;
	}
}
