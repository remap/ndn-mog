using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Linq;

public class DisFish : MonoBehaviour {

	public static Transform Fish;
	public static Transform FishParent;
	
	
	// Dictionary < octant label, List <fish ids> >
	public static Dictionary<string,List<string>> OctFishDic = new Dictionary<string, List<string>>(); 
	
	public static void AddToDic(string oct, string id)
	{
		if(oct==null || oct=="")
			return;

		if( (id == null || id == "") && OctFishDic.ContainsKey(oct)==false)
		{
			OctFishDic.Add (oct,new List<string>());
			return;
		}
		
		if( id != null && id != "" && OctFishDic.ContainsKey(oct)==false)
		{
			OctFishDic.Add (oct,new List<string>());
			OctFishDic[oct].Add(id);
			return;
		}
		
		if( id != null && id != "" && OctFishDic.ContainsKey(oct)==true)
		{
			OctFishDic[oct].Add(id);
			return;
		}
		
	}
	
	public static void AddToDic(string name)
	{
		string oct = M.GetLabelFromName(name);
		string id = M.GetIDFromName(name);
		
		AddToDic(oct, id);
	}
	
	public static bool DicContains(string oct, string id)
	{
		if(OctFishDic.ContainsKey(oct)==true)
		{
			if(OctFishDic[oct].Contains(id))
			{
				return true;
			}
		}
		return false;
	}
	
	
	public struct Exclude
	{
		public string filter; // components, seperated by ','
	}
	
	// Dictionary < asteroid name, asteroid content >
	public static NameContBuf FishNameContBuf = new NameContBuf();
		
	public class NameContBuf
	{
		private Queue buf = new Queue ();
		private static ReaderWriterLockSlim rwl = new ReaderWriterLockSlim();
		
		public string Read()
		{
			string item;
			
			rwl.EnterWriteLock();
			item = (string)buf.Dequeue();
			rwl.ExitWriteLock();
         		
      		return item;
		}
		
		public void Write(string name, string content)
		{
			rwl.EnterWriteLock();
			buf.Enqueue ("" + name + "|" + content);
      		rwl.ExitWriteLock();
		}
		
		public bool IsEmpty()
		{
			if(buf.Count == 0)
				return true;
			else
				return false;
		}
		
		
	}
	void Start () {
		
	 	Fish = GameObject.Find("/fish1").transform;
		FishParent = GameObject.Find("/Fish").transform;
		
	}
	
	
	void Update () {
		
	}
	
	public static void FishDestroy()
	{
	}
	
	public static void FishInstantiate()
	{
	}
	
	public static void DeleteFishBySpace(List<string> octs)
	{
	}
	
	public static void AddFishBySpace(List<string> toadd)
	{
		if(toadd.Count == 0)
			return;
		
		foreach(string n in toadd)
		{
			if(OctFishDic.ContainsKey(n)==true && n!=Initialize.FirstAsteroidLabel)
			{
				//print("AddFishBySpace(): this octant is not new! --" + n);
				continue;
			}
			
			DateTime ct = DateTime.Now.AddMinutes(-15);
			//DateTime ct = new DateTime(2013, 7, 9, 20, 46, 0);
			string currenttime = ct.ToString("ddd-MMM-dd-HH.mm") + ".00-PDT-" + ct.ToString("yyyy");
			string name = M.PREFIX + "/fish/octant/" + n + "/" + currenttime;
			print("discover fish name: " + name);
			RequestAll(name);
			AddToDic(n, null);
		}
	}
	
	static Upcall.ccn_upcall_res DiscoverFishCallback (IntPtr selfp, Upcall.ccn_upcall_kind kind, IntPtr info)
	{
		//print("Fish Callback: " + kind);
		Egal.ccn_upcall_info Info = Egal.GetInfo(info);
		IntPtr h=Info.h;
		
		switch (kind) {
			case Upcall.ccn_upcall_kind.CCN_UPCALL_CONTENT_UNVERIFIED:
        	case Upcall.ccn_upcall_kind.CCN_UPCALL_CONTENT:
			
				string name = Egal.GetContentName(Info.content_ccnb);
				string content = Egal.GetContentValue(Info.content_ccnb, Info.pco); 
				FishNameContBuf.Write (name, content);
			
				string labels = M.GetLabelFromName(name);
				string oldcomponent = M.GetIDFromName(name);
				if(labels == null || oldcomponent == null) 
				{
					print("Ill name: " + name + ", belongs to: " + h);
					break;
				}
				 
				if(Discovery.nimbus.Contains(labels)==false) // we don't care about this octant any more
				{
					print("don't care: " + h + ", " + labels);
					break;
				}
			
				IntPtr c = Egal.ccn_charbuf_create();
				IntPtr templ = Egal.ccn_charbuf_create();
				IntPtr comp = Egal.ccn_charbuf_create();
            	Egal.ccn_name_init(c);
				Egal.ccn_name_init(comp);
				
				string matchedprefix = M.GetNameTillID(name);
				string [] split = matchedprefix.Split(new char [] {'/'},StringSplitOptions.RemoveEmptyEntries);
				foreach(string s in split)
				{
					Egal.ccn_name_append_str(c, s);
				}
			
				Egal.ccn_charbuf_append_tt(templ, (int)Dtag.ccn_dtag.CCN_DTAG_Interest, (int)TT.ccn_tt.CCN_DTAG);
				Egal.ccn_charbuf_append_tt(templ, (int)Dtag.ccn_dtag.CCN_DTAG_Name, (int)TT.ccn_tt.CCN_DTAG);
            	Egal.ccn_charbuf_append_closer(templ); // </Name> 
			
				Egal.ccn_charbuf_append_tt(templ, (int)Dtag.ccn_dtag.CCN_DTAG_Exclude, (int)TT.ccn_tt.CCN_DTAG);
			
				Egal.ccn_closure Selfp = (Egal.ccn_closure)Marshal.PtrToStructure(selfp, typeof(Egal.ccn_closure));
				Exclude Data = (Exclude) Marshal.PtrToStructure(Selfp.data, typeof(Exclude));	
				Data.filter = Data.filter + "," + oldcomponent;
				Marshal.StructureToPtr(Data, Selfp.data, true);
				Marshal.StructureToPtr(Selfp, selfp, true);
			
				string newfilterlist = Data.filter;
				
				string [] filters = newfilterlist.Split(new char [] {','},StringSplitOptions.RemoveEmptyEntries);
				foreach(string s in filters)
				{
					Egal.ccn_name_append_str(comp, s);
				}
				
				Egal.ccn_charbuf_append2(templ, comp);
			
				Egal.ccn_charbuf_append_closer(templ); // </Exclude>
				Egal.ccn_charbuf_append_closer(templ); // </Interest>
		
				// express interest again
				Handle.Pause();
				Egal.ccn_express_interest(h,c,selfp,templ);
				Handle.Resume();
				break;
			
			case Upcall.ccn_upcall_kind.CCN_UPCALL_FINAL:
				print("CCN_UPCALL_FINAL: " + h);
				
				break;
			case Upcall.ccn_upcall_kind.CCN_UPCALL_INTEREST_TIMED_OUT:
				print("CCN_UPCALL_INTEREST_TIMED_OUT: " + h);
				break;
			default: 
				print("othercallback: " + kind);
				break;
		}
		return Upcall.ccn_upcall_res.CCN_UPCALL_RESULT_OK;
	}
	
	public static void RequestAll(string name)
	{	
		Exclude Data = new Exclude();
		IntPtr pData = Marshal.AllocHGlobal(Marshal.SizeOf(Data));
		Marshal.StructureToPtr(Data, pData, true);
		
		//IntPtr ccn = Egal.GetHandle(); // connect to ccnd
		Handle.Pause();
		Egal.ExpressInterest(Handle.ccn, name, DiscoverFishCallback, pData, IntPtr.Zero); // express interest
		Handle.Resume();
	}
	
//	static Upcall.ccn_upcall_res DiscoverFishCallback (IntPtr selfp, Upcall.ccn_upcall_kind kind, IntPtr info)
//	{
//		print("fish callback");
//		return Upcall.ccn_upcall_res.CCN_UPCALL_RESULT_OK;
//	}
}
