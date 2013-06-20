using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

public class FindAsteroids : MonoBehaviour {
	
	public string prefix = "/ndn/ucla.edu/apps/matryoshka";
	
	// boundary: 512*512*512
	struct Boundary{
		public float xmin;
		public float xmax;
		public float ymin;
		public float ymax;
		public float zmin;
		public float zmax;
		public Boundary(float a, float b, float c, 
			float d, float e, float f)
		{
			xmin = a;
			xmax = b;
			ymin = c;
			ymax = d;
			zmin = e;
			zmax = f;
		}
	};
	static Boundary bry;
	
	// List <octant labels>
	List<string> aura; 
	List<string> nimbus;
	
	// Dictionary < octant label, List <asteroid ids> >
	public static Dictionary<string,List<string>> asteroidDic = new Dictionary<string, List<string>>(); 
	public static Dictionary<string, string> asteroidBuffer = new Dictionary<string, string> ();
	
	public struct Exclude
	{
		public string filter; // components, seperated by ','
		
	}
	
	
	
	IEnumerator Start () {
		
		aura = new List<string>();
		nimbus = new List<string>();
		
		while(Initialize.finished != true)
		{
			yield return new WaitForSeconds(0.05f);
		}
		
		print(transform.position);
		
		string temp = M.GetLabel(transform.position);
		if(temp==null)
		{
			print("FindAsteroids.Start(): Aura is null!");
			return false;
		}
		
		aura.Add ( temp );
		nimbus.AddRange ( aura ); // nimbus contains aura
		nimbus.AddRange ( GetNeighbors(transform.position) );
		AddAsteroidBySpace ( nimbus );
		
		bry = GetBoundaries ( aura[0] );
		
		InvokeRepeating("CheckPos", 0, 0.3F);
		InvokeRepeating("Render", 0, 0.1F);
		//RequestAll("/ndn/ucla.edu/apps/matryoshka/asteroid/octant/0/0/0/0");
	}
	
	void Render()
	{
		if(asteroidBuffer.Count != 0)
		{ 
			foreach(string name in asteroidBuffer.Keys)
			{
				string info = asteroidBuffer[name];
				string n = M.GetLabelFromName(name);
				print(n);
				Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(info);
				string id = values["fs"];
				if(asteroidDic.ContainsKey(n)==true)
				{
					if(asteroidDic[n].Contains(id))
					{
						continue;
					}
				}
				
				
				DoAsteroid(info);
					
				if(asteroidDic.ContainsKey(n)==false)
				{
					asteroidDic.Add (n,new List<string>());
				}
				asteroidDic[n].Add(id);
				
			}
			asteroidBuffer.Clear();
		}
		
	}
	
	
	
	void CheckPos() {
		
		string temp = null;
		
		if( InBound(transform.position) == false )
		{
			aura.Clear();
			temp = M.GetLabel(transform.position);
			if(temp == null)
			{
				print("FindAsteroids.CheckPos(): Aura is null!");
				return;
			}
			
			aura.Add ( temp );
			
			List<string> newnimbus = new List<string>();
			newnimbus.AddRange( aura );
			newnimbus.AddRange ( GetNeighbors(transform.position) );
			
			List<string> newoct = new List<string>(); // octants to be added to nimubs
			List<string> oldoct = new List<string>(); // octants to be deleted from nimbus
			
			CompareNimbus(nimbus, newnimbus, newoct, oldoct);
			
//			print(string.Join(",  ", nimbus.ToArray()));
//			print(string.Join(",  ", newnimbus.ToArray()));
			print("new octant to be added: " + string.Join(",  ", newoct.ToArray()));
			print("old octant to be deleted: " + string.Join(",  ", oldoct.ToArray()));
			string sum = "";
			foreach(string k in asteroidDic.Keys)
			{
				sum = sum + k + ",  ";
			}
			print("asteroidDictionary before +/-: " + sum);
				
			AddAsteroidBySpace(newoct);
			DeleteAsteroidBySpace(oldoct);
			
			sum = "";
			foreach(string k in asteroidDic.Keys)
			{
				sum = sum + k + ",  ";
			}
			print("asteroidDictionary after +/-: " + sum);
			
			newoct.Clear();
			oldoct.Clear();
			
			nimbus.Clear();
			nimbus.AddRange(newnimbus);
			bry = GetBoundaries(aura[0]);
			
		}
		
    }
	
	
	void AddAsteroidBySpace(List<string> nimbus)
	{
		List<string> asteroidnames = null;
		foreach(string n in nimbus)
		{
			if(asteroidDic.ContainsKey(n)==true)
			{
				// print("AddAsteroidBySpace(): this octant is not new! --" + n);
				continue;
			}
			
			RequestAll( prefix + "/asteroid/octant/" + n);
			

			
		}
	}
	
	void DeleteAsteroidBySpace(List<string> octs)
	{
		List<string> asteroidids;
		foreach(string o in octs)
		{
			if(asteroidDic.ContainsKey(o) == false)
			{
				// print("DeleteAsteroidBySpace(): this octant is not old! --" + o);
				continue;
			}
			
			asteroidids = asteroidDic[o];
			foreach(string id in asteroidids)
			{
				GameObject t = GameObject.Find("asteroid-"+id);
				if(!t)
				{
					print("Can't destroy asteroid with given id.");
				}
				Destroy( t );
			}
			asteroidDic.Remove(o);
		}
	}
	
	void CompareNimbus(List<string> oldnimbus, List<string> newnimbus, 
		List<string> newoct, List<string> oldoct)
	{
		foreach(string o in oldnimbus)
		{
			if(newnimbus.Contains(o)==false)
			{
				oldoct.Add(o);
			}
			
		}
		
		foreach(string n in newnimbus)
		{
			if(oldnimbus.Contains(n)==false)
			{
				newoct.Add(n);
			}
		}
			
	}
	
	static bool InBound(Vector3 position)
	{
		if(position.x<bry.xmin || position.y<bry.ymin || position.z<bry.zmin)
		{
			return false;
		}
		if(position.x>bry.xmax || position.y>bry.ymax || position.z>bry.zmax)
		{
			return false;
		}
		return true;
	}
	

	
	
	List<string> GetNeighbors(Vector3 position)
	{
		List<string> neighborlist = new List<string>();
		int[,] neighbors = {{1,0,0}, {-1,0,0}, // x
							{0,1,0}, {0,-1,0}, // y
							{0,0,1}, {0,0,-1}, // z
							{1,1,0}, {1,-1,0}, {-1,1,0}, {-1,-1,0}, // x,y
							{1,0,1}, {1,0,-1}, {-1,0,1}, {-1,0,-1}, // x,z
							{0,1,1}, {0,1,-1}, {0,-1,1}, {0,-1,-1}, // y,z
							{1,1,1}, {-1,1,1}, {1,-1,1}, {1,1,-1}, {1,-1,-1}, {-1,1,-1}, {-1,-1,1},{-1,-1,-1} // x,y,z
		};
		
		Vector3 offset = new Vector3();
		string temp = null;
		for(int i = 0; i<26; i++)
		{
			offset.x = neighbors[i,0] * 512;
			offset.y = neighbors[i,1] * 512;
			offset.z = neighbors[i,2] * 512;
			
			temp = M.GetLabel(position+offset);
			if(temp!=null)
			{
				neighborlist.Add(temp);
			}
		}
		
		
		return neighborlist;
	}
	
	Boundary GetBoundaries(string labels)
	{
		// print(labels);
		string [] split = labels.Split(new char [] {'/'},StringSplitOptions.RemoveEmptyEntries);
		
		int L1oct = Convert.ToInt32(split[0],8);
		int L2oct = Convert.ToInt32(split[1],8);
		int L3oct = Convert.ToInt32(split[2],8);
		int L4oct = Convert.ToInt32(split[3],8);
		
		string L1bits = Convert.ToString (L1oct,2).PadLeft(3,'0');
		string L2bits = Convert.ToString (L2oct,2).PadLeft(3,'0');
		string L3bits = Convert.ToString (L3oct,2).PadLeft(3,'0');
		string L4bits = Convert.ToString (L4oct,2).PadLeft(3,'0');
		
		string xbits = "" + L1bits[0] + L2bits[0] + L3bits[0] + L4bits[0];
		string ybits = "" + L1bits[1] + L2bits[1] + L3bits[1] + L4bits[1];
		string zbits = "" + L1bits[2] + L2bits[2] + L3bits[2] + L4bits[2];
		
		int x = Convert.ToInt32 (xbits,2);
		int y = Convert.ToInt32 (ybits,2);
		int z = Convert.ToInt32 (zbits,2);
		
		int xmin = x * 512; 
		int ymin = y * 512;
		int zmin = z * 512;
		
		int xmax = xmin + 512;
		int ymax = ymin + 512;
		int zmax = zmin + 512;
		
		//print(labels + ":" + L1bits + "," + L2bits + "," + L3bits + "," + L4bits);
		//print(xbits + "," + ybits + "," + zbits);
//		
//		print(transform.position.x>xmin);
//		print(transform.position.x<xmax);
//		
//		print(transform.position.y>ymin);
//		print(transform.position.y<ymax);
//		
//		print(transform.position.z>zmin);
//		print(transform.position.z<zmax);
		
		Boundary bry = new Boundary(xmin, xmax, ymin, ymax, zmin, zmax);
		return bry;
		
	}
	
	static Upcall.ccn_upcall_res RequestAllCallback (IntPtr selfp, Upcall.ccn_upcall_kind kind, IntPtr info)
	{
		//print("RequestAllCallback: " + kind);
		Egal.ccn_upcall_info Info = Egal.GetInfo(info);
		IntPtr h=Info.h;
		
		switch (kind) {
			case Upcall.ccn_upcall_kind.CCN_UPCALL_CONTENT_UNVERIFIED:
        	case Upcall.ccn_upcall_kind.CCN_UPCALL_CONTENT:
				IntPtr c = Egal.ccn_charbuf_create();
				IntPtr templ = Egal.ccn_charbuf_create();
				IntPtr comp = Egal.ccn_charbuf_create();
            	Egal.ccn_name_init(c);
				Egal.ccn_name_init(comp);
			
				string name = Egal.GetContentName(Info.content_ccnb);
				print("received: " + name);
				string content = Egal.GetContentValue(Info.content_ccnb, Info.pco); 
			
				asteroidBuffer.Add (name, content);
			
				int index = name.IndexOf("/octant/");
				string matchedprefix = name.Substring(0, index + 15);
				string [] split = matchedprefix.Split(new char [] {'/'},StringSplitOptions.RemoveEmptyEntries);
				foreach(string s in split)
				{
					Egal.ccn_name_append_str(c, s);
				}
			
				string tail = name.Substring(index + 16);
				split = tail.Split(new char [] {'/'},StringSplitOptions.RemoveEmptyEntries);
				string oldcomponent = split[0]; 
				//Egal.ccn_name_append_str(comp, oldcomponent);
			
				Egal.ccn_charbuf_append_tt(templ, (int)Dtag.ccn_dtag.CCN_DTAG_Interest, (int)TT.ccn_tt.CCN_DTAG);
				Egal.ccn_charbuf_append_tt(templ, (int)Dtag.ccn_dtag.CCN_DTAG_Name, (int)TT.ccn_tt.CCN_DTAG);
            	Egal.ccn_charbuf_append_closer(templ); // </Name> 
			
				Egal.ccn_charbuf_append_tt(templ, (int)Dtag.ccn_dtag.CCN_DTAG_Exclude, (int)TT.ccn_tt.CCN_DTAG);
			
				Egal.ccn_closure Selfp = (Egal.ccn_closure)Marshal.PtrToStructure(selfp, typeof(Egal.ccn_closure));
				Exclude Data = (Exclude) Marshal.PtrToStructure(Selfp.data, typeof(Exclude));	
				print("exclusion filter: " + Data.filter);
				Data.filter = Data.filter + "," + oldcomponent;
				IntPtr pData = Marshal.AllocHGlobal(Marshal.SizeOf(Data));
				Marshal.StructureToPtr(Data, pData, true);
				Selfp.data = pData;
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
				Egal.ccn_express_interest(h,c,selfp,templ);
			
				break;
			
			case Upcall.ccn_upcall_kind.CCN_UPCALL_FINAL:
				Egal.ccn_set_run_timeout(h, 0); 
				Egal.killCurrentThread(); // kill current thread
				break;
			default: break;
		}
		return Upcall.ccn_upcall_res.CCN_UPCALL_RESULT_OK;
	}
	
	public List<string> RequestAll(string name)
	{
		//print("requestall: " + name);
		
		Exclude Data = new Exclude();
		IntPtr pData = Marshal.AllocHGlobal(Marshal.SizeOf(Data));
		Marshal.StructureToPtr(Data, pData, true);
		

		IntPtr ccn = Egal.GetHandle(); // connect to ccnd
		Egal.ExpressInterest(ccn, name, RequestAllCallback, pData, IntPtr.Zero); // express interest
		Egal.ccnRun(ccn, -1); // ccnRun starts a new thread
		
		return null;
	}
	
	string DoAsteroid(string info)
	{
		//print(info);
		Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(info);
		Vector3 pos = M.GetGameCoordinates(values["latitude"], values["longitude"]);
		Initialize.MakeAnAsteroid(pos, values["fs"]);
		string label = M.GetLabel(pos);
		print("fs: " + values["fs"] + ", label: " + label);
		return values["fs"];
	}
}
