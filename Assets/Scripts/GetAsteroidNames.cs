using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

public class GetAsteroidNames : MonoBehaviour {
	
	public struct NEData
	{
    	public int seg;
    	public IntPtr ccnb;
    	public int length; 	
    	
		public NEData(int s, int l)
		{
			this.seg = s;
			this.length = l;
			this.ccnb = IntPtr.Zero;
		}
	}
	
	static NEData GlobalNEData = new NEData(0,0);
	static IntPtr pgnedata = Marshal.AllocHGlobal(Marshal.SizeOf(GlobalNEData));
	
	
	void Start () {
	
		IntPtr ccn = CCNScript.GetHandle();
		IntPtr name = Egal.ccn_charbuf_create();
		Egal.ccn_name_from_uri(name, "ccnx:/ndn/ucla.edu/airports/%C1.E.be");
		
		//
		Marshal.StructureToPtr(GlobalNEData, pgnedata, true);
		
		EnumerateNames(ccn, name, pgnedata, IntPtr.Zero);
		Egal.ccn_run(ccn, -1);
		Egal.ccn_destroy(ref ccn);
	}
	
	static int EnumerateNames(IntPtr ccn, IntPtr nm, IntPtr nedata, IntPtr templ)
	{
		Egal.ccn_closure Action = new Egal.ccn_closure(ENCallback, nedata, 0);
		IntPtr pnt = Marshal.AllocHGlobal(Marshal.SizeOf(Action));
		Marshal.StructureToPtr(Action, pnt, true);
		
		Egal.ccn_express_interest(ccn, nm, pnt, templ);
		return 0;
	}
	
	static int seg_cb = 0;
	
	static Upcall.ccn_upcall_res ENCallback(IntPtr selfp, Upcall.ccn_upcall_kind kind, IntPtr info)
	{
		print("en callback");
		Egal.ccn_upcall_info Info = new Egal.ccn_upcall_info();
		Info = (Egal.ccn_upcall_info)Marshal.PtrToStructure(info, typeof(Egal.ccn_upcall_info));
		IntPtr h = Info.h;
		
		print(Info.pco);
		Egal.ccn_parsed_ContentObject Pco = new Egal.ccn_parsed_ContentObject();
		Pco = (Egal.ccn_parsed_ContentObject)Marshal.PtrToStructure(Info.pco, typeof(Egal.ccn_parsed_ContentObject));
			
		Egal.ccn_closure Selfp = new Egal.ccn_closure();
		Selfp = (Egal.ccn_closure)Marshal.PtrToStructure(selfp, typeof(Egal.ccn_closure));
		NEData NED = new NEData();
		NED = (NEData) Marshal.PtrToStructure(Selfp.data, typeof(NEData));
		
		switch (kind) {
		case Upcall.ccn_upcall_kind.CCN_UPCALL_CONTENT_UNVERIFIED:
        case Upcall.ccn_upcall_kind.CCN_UPCALL_CONTENT:
			
			IntPtr ccnb = Info.content_ccnb;
			IntPtr result_ptr = IntPtr.Zero;
			int result_length = 0;
            Egal.ccn_content_get_value(ccnb, 0, Info.pco, ref result_ptr, ref result_length);
			
			// *** collect ccnb encoded content *** //
			if(GlobalNEData.seg == seg_cb)
			{
				GlobalNEData.seg++;
				GlobalNEData.length += result_length;
				// collect ccnb also
			}
			else
			{
				print("We are out of order!");
			}
			
			
			
			// *** fetch later segments *** //
			if(Egal.ccn_is_final_block(info) == 1)
			{
				print("This is the final block.");
				Egal.ccn_set_run_timeout(h, 0);
			}
			else
			{
				//print("Current segment: " + seg_cb);
				seg_cb ++;
				//print("Next segment: " + seg_cb);
				IntPtr c = Egal.ccn_charbuf_create();
				
				int length_of_name = Pco.name_ncomps;
				
				Egal.ccn_name_init(c);
				int startid = (int)PCO.ccn_parsed_content_object_offsetid.CCN_PCO_B_Component0;
				int stopid = (int)PCO.ccn_parsed_content_object_offsetid.CCN_PCO_E_ComponentLast;
				int start = Pco.offset[startid];
				int stop = Pco.offset[stopid];
				int res = Egal.ccn_name_append_components(c, Info.content_ccnb, start, stop);
				res = Egal.ccn_name_chop(c, IntPtr.Zero, -1);
				res = Egal.ccn_name_append_numeric(c, Marker.ccn_marker.CCN_MARKER_SEQNUM, seg_cb);
				//print(res);
				
				
				Marshal.FreeHGlobal(pgnedata);
				pgnedata = Marshal.AllocHGlobal(Marshal.SizeOf(GlobalNEData));
				Marshal.StructureToPtr(GlobalNEData, pgnedata, true);
				EnumerateNames(h, c, pgnedata, IntPtr.Zero);
			}
			
			break;
			
		case Upcall.ccn_upcall_kind.CCN_UPCALL_FINAL:
			print("EN upcall final");
			break;
		}
		return Upcall.ccn_upcall_res.CCN_UPCALL_RESULT_OK;
	}
}
