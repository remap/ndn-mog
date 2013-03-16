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
		Egal.ccn_upcall_info Info = new Egal.ccn_upcall_info();
		Info = (Egal.ccn_upcall_info)Marshal.PtrToStructure(info, typeof(Egal.ccn_upcall_info));
		
		switch (kind) {
		case Upcall.ccn_upcall_kind.CCN_UPCALL_CONTENT_UNVERIFIED:
        case Upcall.ccn_upcall_kind.CCN_UPCALL_CONTENT:
			
			IntPtr ccnb = Info.content_ccnb;
			IntPtr result_ptr = IntPtr.Zero;
			int result_length = 0;
            Egal.ccn_content_get_value(ccnb, 0, Info.pco, ref result_ptr, ref result_length);
			print(result_ptr);
			// I still need to parse the ccnb encoded content here...
			break;
		case Upcall.ccn_upcall_kind.CCN_UPCALL_FINAL:
			print("EN upcall final");
			break;
		}
		return Upcall.ccn_upcall_res.CCN_UPCALL_RESULT_OK;
	}
}
