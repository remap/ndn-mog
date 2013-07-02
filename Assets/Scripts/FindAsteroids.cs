using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Linq;

public class FindAsteroids : MonoBehaviour {
	
	public string prefix = "/ndn/ucla.edu/apps/matryoshka";
	
	// boundary: 512*512*512
	public struct Boundary{
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
	public static List<string> aura; 
	public static List<string> nimbus;
	
	// Dictionary < octant label, List <asteroid ids> >
	public static Dictionary<string,List<string>> OctAstDic = new Dictionary<string, List<string>>(); 
	public static void AddToDic(string oct, string id)
	{
		if(OctAstDic.ContainsKey(oct)==false)
		{
			OctAstDic.Add (oct,new List<string>());
		}
		OctAstDic[oct].Add(id);
	}
	public static void AddToDic(string name)
	{
		string oct = M.GetLabelFromName(name);
		string id = M.GetIDFromName(name);
		
		if(oct == null || id == null)
			return;
		
		if(OctAstDic.ContainsKey(oct)==false)
		{
			OctAstDic.Add (oct,new List<string>());
		}
		OctAstDic[oct].Add(id);
	}
	public static bool DicContains(string oct, string id)
	{
		if(OctAstDic.ContainsKey(oct)==true)
		{
			if(OctAstDic[oct].Contains(id))
			{
				return true;
			}
		}
		return false;
	}
	
	// Dictionary < asteroid name, asteroid content >
	public static NameContBuf AstNameContBuf = new NameContBuf();
		
	public class NameContBuf
	{
		private Dictionary<string, string> buf = new Dictionary<string, string> ();
		private bool readerFlag = false;
		private static ReaderWriterLockSlim rwl = new ReaderWriterLockSlim();
		
		public Dictionary<string, string> Read()
		{
			Dictionary<string, string> copy;
			rwl.EnterWriteLock();
			copy = new Dictionary<string, string>(buf);
			buf.Clear();
			rwl.ExitWriteLock();
         		
      		return copy;
		}
		
		public void Write(string name, string content)
		{
			rwl.EnterWriteLock();
			buf.Add (name, content);
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
		
		string temp = M.GetLabel(transform.position);
		if(temp==null)
		{
			print("FindAsteroids.Start(): Aura is null!");
			return false;
		}
		
		aura.Add ( temp );
		bry = M.GetBoundaries ( aura[0] );
		
		nimbus.AddRange ( aura ); // nimbus contains aura
		nimbus.AddRange ( M.GetNeighbors(transform.position) );
		
		AddAsteroidBySpace ( nimbus );
		
		InvokeRepeating("CheckPos", 0, 0.5F); 
		//InvokeRepeating("Render", 0, 0.1F);
		
	}
	
	void Update()
	{
		Render ();
	}
	
	void Render()
	{
		if(AstNameContBuf.IsEmpty() == false)
		{ 
			Dictionary<string, string> buf = AstNameContBuf.Read();
			
			foreach(string name in buf.Keys)
			{
				if(name == Initialize.FirstAsteroidName)
				{
					continue;
				}
				
				string info = buf[name];
				
				string n = M.GetLabelFromName(name);
				string id = M.GetIDFromName(name);
				
				if(n == null || id == null)
					continue;
				if(DicContains(n, id)==true)
					continue;
				
				MakeAnAsteroid(info);
					
				AddToDic(n,id);
				
			}
			
		}
		
	}
		
	void CheckPos() {
		
		string temp = null;
		
		if( InBound(transform.position) == false )
		{
			//print("Out of Bound!");
			aura.Clear();
			temp = M.GetLabel(transform.position);
			if(temp == null)
			{
				//print("FindAsteroids.CheckPos(): Aura is null!");
				return;
			}
			
			aura.Add ( temp );
			bry = M.GetBoundaries(aura[0]); // update the boundaries
			
			List<string> newnimbus = new List<string>();
			newnimbus.AddRange( aura );
			newnimbus.AddRange ( M.GetNeighbors(transform.position) );
			
			List<string> newoct = newnimbus.Except(nimbus).ToList();
			List<string> datedoct = nimbus.Except(newnimbus).ToList();
			
			DeleteAsteroidBySpace(datedoct);
			AddAsteroidBySpace(newoct);
			
			nimbus.Clear();
			nimbus.AddRange(newnimbus);
			
		}
		
    }
	
	void AddAsteroidBySpace(List<string> nimbus)
	{
		List<string> asteroidnames = null;
		foreach(string n in nimbus)
		{
			if(OctAstDic.ContainsKey(n)==true && n!=Initialize.FirstAsteroidLabel)
			{
				//print("AddAsteroidBySpace(): this octant is not new! --" + n);
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
			if(OctAstDic.ContainsKey(o) == false)
			{
				// print("DeleteAsteroidBySpace(): this octant is not old! --" + o);
				continue;
			}
			
			asteroidids = OctAstDic[o];
			foreach(string id in asteroidids)
			{
				GameObject t = GameObject.Find("asteroid-"+id);
				if(!t)
				{
					print("Can't destroy asteroid with given id.");
				}
				Destroy( t );
			}
			OctAstDic.Remove(o);
		}
		Resources.UnloadUnusedAssets();
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
		
	
	
	
	
	static Upcall.ccn_upcall_res RequestAllCallback (IntPtr selfp, Upcall.ccn_upcall_kind kind, IntPtr info)
	{
		//print("RequestAllCallback: " + kind + " long... long... long... long... long... long... long... long...");
		Egal.ccn_upcall_info Info = Egal.GetInfo(info);
		IntPtr h=Info.h;
		
		switch (kind) {
			case Upcall.ccn_upcall_kind.CCN_UPCALL_CONTENT_UNVERIFIED:
        	case Upcall.ccn_upcall_kind.CCN_UPCALL_CONTENT:
			
				string name = Egal.GetContentName(Info.content_ccnb);
				string content = Egal.GetContentValue(Info.content_ccnb, Info.pco); 
				AstNameContBuf.Write (name, content);
			
				string labels = M.GetLabelFromName(name);
				string oldcomponent = M.GetIDFromName(name);
				if(labels == null || oldcomponent == null) 
				{
					print("Ill name: " + name + ", belongs to: " + h);
					break;
				}
				 
				if(nimbus.Contains(labels)==false) // we don't care about this octant any more
				{
					print("don't care: " + h + ", " + labels);
					TPool.AllHandles.Delete(h);
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
				Egal.ccn_express_interest(h,c,selfp,templ);
			
				break;
			
			case Upcall.ccn_upcall_kind.CCN_UPCALL_FINAL:
				print("CCN_UPCALL_FINAL: " + h);
				TPool.AllHandles.Delete(h);
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
	
	public void RequestAll(string name)
	{	
		Exclude Data = new Exclude();
		IntPtr pData = Marshal.AllocHGlobal(Marshal.SizeOf(Data));
		Marshal.StructureToPtr(Data, pData, true);
		
		IntPtr ccn = Egal.GetHandle(); // connect to ccnd
		Egal.ExpressInterest(ccn, name, RequestAllCallback, pData, IntPtr.Zero); // express interest
		
		string labels = M.GetLabelFromName(name);
		TPool.AllHandles.Add(ccn, labels);
	}
	
	public static Vector3 MakeAnAsteroid(string info)
	{
		Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(info);
		Vector3 pos = M.GetGameCoordinates(values["latitude"], values["longitude"]);
		RenderAnAsteroid(pos, values["fs"]);
		string label = M.GetLabel(pos);
		return pos;
	}
	
	public static void RenderAnAsteroid(Vector3 position, string id)
	{
		// instantiate an asteroid
		GameObject asteroid1 = GameObject.Find("tree2");
		GameObject newAsteroid = UnityEngine.Object.Instantiate(asteroid1, position, Quaternion.identity) as GameObject;
		newAsteroid.name = "asteroid-"+id;
		newAsteroid.transform.localScale = new Vector3(1000f,1000f,1000f);
		newAsteroid.tag = "Asteroid";
	}
	

}
