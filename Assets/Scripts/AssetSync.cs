using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

public class AssetSync : MonoBehaviour {
	
	public static IntPtr h_ccns_watch;
	public static IntPtr hh;
	
	public static bool NewObj = false;
	public static string NewObjName = "";
	public static string NewObjContent = "";
	
	public static bool Initialized = false;
	
	public static System.String prefix = "ccnx:/ndn/ucla.edu/apps/Confetti";
	private static System.String topo = "ccnx:/ndn/broadcast/Confetti";
	public static int TIMEOUT = 10;
	
	Thread oThread;
	
	public GameObject b612;
	
	public static string me = "";
	
	public static Hashtable Buffer = new Hashtable(); // other players' ccnx name and content
	public static System.Object lc = new System.Object(); // a lock for the buffer, share this lock with Player.cs
	
	public static int counter_for_run = 0;
	
	static bool KnownAsset(string name)
	{
		bool ret = false;
		if(Player.PlayerList.ContainsKey(name))
			ret = true;
		if(name == Player.me)
			ret = true;
		return ret;
	}
	
	void Start()
	{
		int res = WriteSlice(prefix, topo);
		print("WriteSlice returned: " + res);
		
		WatchOverRepo(prefix, topo);
		
    	// RegisterInterestFilter(h, me + "/state");
    
	}
	
	static IntPtr GetHandle()
	{
		// this is a C# expansion of Egal.GetHandle()
		IntPtr ccn = Egal.ccn_create();
		if (Egal.ccn_connect(ccn, "") == -1) 
        	print("could not connect to ccnd.");
		//else
			//print ("a handle is connected to ccnd.");
		return ccn;
	}
	
	int WriteSlice(System.String p, System.String t)
	{
		// this is a C# expansion of Egal.WriteSlice
		int res;
		IntPtr prefix = Egal.ccn_charbuf_create();
		IntPtr topo = Egal.ccn_charbuf_create();
		
		Egal.ccn_name_init(prefix);
    	Egal.ccn_name_init(topo);
		
		res = Egal.ccn_name_from_uri(prefix, p);
		if(res<0)
		{
			print ("Prefix not right");
			return res;
		}
		
		res = Egal.ccn_name_from_uri(topo, t);
		if(res<0)
		{
			print ("Topo not right");
			return res;
		}
		
		IntPtr slice = Egal.ccns_slice_create();
		Egal.ccns_slice_set_topo_prefix(slice, topo, prefix);
		IntPtr h = GetHandle();
		res = Egal.ccns_write_slice(h, slice, prefix);
    	Egal.ccn_destroy(ref h);
    	Egal.ccns_slice_destroy(ref slice); // after this, slice == 0
		
		return res;
	}
	
	static String NameTrim(String playername)
	{
		// remove version and whatever after it from names
		String ShortPlayerName = "";
		int index = playername.IndexOf('%');
		if(index <= 0)
			ShortPlayerName = playername;
		else
			ShortPlayerName = playername.Remove (index-1);

		return ShortPlayerName;
	}
	
	static int WatchCallback(IntPtr nc, IntPtr lhash, IntPtr rhash, IntPtr pname)
	{
		print ("WatchCallback...");
		
		IntPtr uri = Egal.ccn_charbuf_create();
		
		Egal.ccn_charbuf Name = (Egal.ccn_charbuf)Marshal.PtrToStructure(pname, typeof(Egal.ccn_charbuf));
		Egal.ccn_uri_append(uri, Name.buf, Name.length, 1);
		
		IntPtr temp = Egal.ccn_charbuf_as_string(uri);
		String PlayerName = Marshal.PtrToStringAnsi(temp);
		
		String ShortPlayerName = NameTrim(PlayerName);
			
		if(KnownAsset(ShortPlayerName) == false)
		{
			ReadFromRepo(ShortPlayerName);
		}
		
		Egal.ccn_charbuf_destroy(ref uri);
		
		return 0;
	}
	
	static Upcall.ccn_upcall_res ReadCallback(IntPtr selfp, Upcall.ccn_upcall_kind kind, IntPtr info)
	{
		print("ReadCallback! " + kind);
		Upcall.ccn_upcall_res res = Upcall.ccn_upcall_res.CCN_UPCALL_RESULT_OK;
		
		Egal.ccn_upcall_info Info = new Egal.ccn_upcall_info();
		Info = (Egal.ccn_upcall_info)Marshal.PtrToStructure(info, typeof(Egal.ccn_upcall_info));
		IntPtr h=Info.h;
		
		// get name of content object from self->data->nm ... phew!
		Egal.ccn_closure Selfp = new Egal.ccn_closure();
		Selfp = (Egal.ccn_closure)Marshal.PtrToStructure(selfp, typeof(Egal.ccn_closure));
		NormalStruct Data = new NormalStruct();
		Data = (NormalStruct) Marshal.PtrToStructure(Selfp.data, typeof(NormalStruct));	
		IntPtr uri = Egal.ccn_charbuf_create();
		Egal.ccn_charbuf Name = (Egal.ccn_charbuf)Marshal.PtrToStructure(Data.nm, typeof(Egal.ccn_charbuf));
		Egal.ccn_uri_append(uri, Name.buf, Name.length, 1);
		IntPtr temp = Egal.ccn_charbuf_as_string(uri);
		String PlayerName = Marshal.PtrToStringAnsi(temp);
		String ShortPlayerName = NameTrim(PlayerName);
		//print (PlayerName);
		
		
		switch (kind) {
        case Upcall.ccn_upcall_kind.CCN_UPCALL_CONTENT:
            //print("ReadCallback: CCN_UPCALL_CONTENT");
			
			// get human-readable content
			IntPtr source_ptr = Info.content_ccnb;
			Egal.ccn_parsed_ContentObject Pco = new Egal.ccn_parsed_ContentObject();
			Pco = (Egal.ccn_parsed_ContentObject)Marshal.PtrToStructure(Info.content_ccnb, typeof(Egal.ccn_parsed_ContentObject));
			UInt16 source_length = Pco.offset[(int)PCO.ccn_parsed_content_object_offsetid.CCN_PCO_E];
			IntPtr result_ptr = IntPtr.Zero;
			int result_length = 0;
            Egal.ccn_content_get_value(source_ptr, source_length, Info.pco, ref result_ptr, ref result_length);
			String content = Marshal.PtrToStringAnsi(result_ptr);
			
			string pack = ShortPlayerName + ": " + content;
			print(pack);
			lock(lc)
			{
				Buffer.Add(ShortPlayerName, content);
			}
			
			break;
		case Upcall.ccn_upcall_kind.CCN_UPCALL_FINAL:
            //print("ReadCallback: CCN_UPCALL_FINAL");
            Egal.ccn_set_run_timeout(h, 0);
            break;
        default:
            break;
    	}
            
		
		return res;
	}
	
	
	static void ReadFromRepo(string dst)
	{
		print("Reading from the repo.");
		IntPtr ccn = GetHandle();
		
		IntPtr nm = Egal.ccn_charbuf_create();
		Egal.ccn_name_from_uri(nm,dst);
		Egal.ccn_create_version(ccn, nm, VersioningFlags.CCN_V_LOW, 0, 0); // without version, Unity crashes!
		//CCN_V_LOW might be right, might not!
		
		// I need to include name in the closure
		// because readcallback will need this name
		// and I don't know how to parse content object yet
		NormalStruct Data = new NormalStruct(nm, IntPtr.Zero, IntPtr.Zero, 0, "");
		IntPtr pData = Marshal.AllocHGlobal(Marshal.SizeOf(Data));
		Marshal.StructureToPtr(Data, pData, true);
		
		IntPtr template = Egal.SyncGenInterest(IntPtr.Zero, 1, 4, -1, -1, IntPtr.Zero);
		
		Egal.ccn_closure Action = new Egal.ccn_closure(ReadCallback, pData, 0);
		IntPtr pnt = Marshal.AllocHGlobal(Marshal.SizeOf(Action));
		Marshal.StructureToPtr(Action, pnt, true);
		
		Egal.ccn_express_interest(ccn,nm,pnt,template);
		Egal.ccn_run(ccn,-1);
		
		Egal.ccn_destroy(ref ccn);
	}
	
	int WatchOverRepo(string p, string t)
	{
		// this is a C# expansion of Egal.WatchOverRepo
		int res;
		h_ccns_watch = GetHandle();
		IntPtr prefix = Egal.ccn_charbuf_create();
		IntPtr topo = Egal.ccn_charbuf_create();
		
		Egal.ccn_name_init(prefix);
    	Egal.ccn_name_init(topo);
		
		res = Egal.ccn_name_from_uri(prefix, p);
		if(res<0){print ("Prefix not right");}
		
		res = Egal.ccn_name_from_uri(topo, t);
		if(res<0){print ("Topo not right");}
		
		
		IntPtr slice = Egal.ccns_slice_create();
    	Egal.ccns_slice_set_topo_prefix(slice, topo, prefix);
    
		Egal.ccns_name_closure closure = new Egal.ccns_name_closure(WatchCallback, IntPtr.Zero, 0);
		IntPtr p_closure = Marshal.AllocHGlobal(Marshal.SizeOf(closure));
		Marshal.StructureToPtr(closure, p_closure, true);
		
    	IntPtr ccns = Egal.ccns_open(h_ccns_watch, slice, p_closure, IntPtr.Zero, IntPtr.Zero);
		
		//Egal.ccn_run(h_ccns_watch, -1);
		
		oThread = new Thread(new ThreadStart(run));
      	oThread.Start();
		
		// ccns_close(&ccns, NULL, NULL);
    
    	Egal.ccns_slice_destroy(ref slice);
    
    	return res;
		
	}
	
	public void run()
	{
		Thread t = Thread.CurrentThread;
		// print (t.IsAlive);
		while(t.IsAlive == true)
		{
			//while(counter_for_run>0)
				//;
			Egal.ccn_run(h_ccns_watch, -1);
			
		}
		
	}
	public struct NormalStruct
	{
    	public IntPtr nm;
    	IntPtr cb;
    	public IntPtr ccn; 	// handle
    	public int vSize;
    	public string value; /* not so sure */
		
		// constructor
		public NormalStruct(IntPtr name, IntPtr contentbuffer, IntPtr handle,
			 int valuesize, string content)
		{
			this.nm = name;
			this.cb = contentbuffer;
			this.ccn = handle;
			this.vSize = valuesize;
			this.value = content;
		}
	}
	
	int PutContent(IntPtr h, NormalStruct Data)
	{
		int res = 0;
		
		Egal.ccn_charbuf Nm = new Egal.ccn_charbuf();
		Nm = (Egal.ccn_charbuf)Marshal.PtrToStructure(Data.nm, typeof(Egal.ccn_charbuf));
		IntPtr uri = Egal.ccn_charbuf_create();
		Egal.ccn_uri_append(uri, Nm.buf, Nm.length, 0);
		
		IntPtr name = Egal.SyncCopyName(Data.nm);
		
		// test probe //
		Egal.ccn_charbuf Name = new Egal.ccn_charbuf();
		Name = (Egal.ccn_charbuf)Marshal.PtrToStructure(name, typeof(Egal.ccn_charbuf));
		//print("Name length before numeric: " + Name.length);
		// end test probe //
		
		res = Egal.ccn_name_append_numeric(name, Marker.ccn_marker.CCN_MARKER_SEQNUM, 0);
		
		// test probe //
		Name = (Egal.ccn_charbuf)Marshal.PtrToStructure(name, typeof(Egal.ccn_charbuf));
		//print("Name length after numeric: " + Name.length);
		//print ("size of enum type is: " + sizeof(Marker.ccn_marker));
		// end test probe //
		
		//print("append numeric returns: " + res);
		//print(Marker.ccn_marker.CCN_MARKER_SEQNUM);
		
		IntPtr cb = Egal.ccn_charbuf_create();
		Egal.ccn_charbuf_append(cb, Data.value, Data.vSize);
		Egal.ccn_charbuf Cb = new Egal.ccn_charbuf();
		Cb = (Egal.ccn_charbuf)Marshal.PtrToStructure(cb, typeof(Egal.ccn_charbuf));
		
		Egal.ccn_signing_params Sp = new Egal.ccn_signing_params(CCN.CCN_API_VERSION);
		Sp.sp_flags |= SP.signingparameters.CCN_SP_FINAL_BLOCK;
		IntPtr pSp = Marshal.AllocHGlobal(Marshal.SizeOf(Sp));
		Marshal.StructureToPtr(Sp, pSp, true);
		
		// Data.ccn, Cb.buf, Data.vSize is correct
		
		IntPtr cob = Egal.ccn_charbuf_create();
		
		res = Egal.ccn_sign_content(h, cob, name, pSp, Cb.buf, Data.vSize);
		if(res<0) print ("sign content error.");
		
		Egal.ccn_charbuf Cob = new Egal.ccn_charbuf();
		Cob = (Egal.ccn_charbuf)Marshal.PtrToStructure(cob, typeof(Egal.ccn_charbuf));
		
		res = Egal.ccn_put(Data.ccn, Cob.buf, Cob.length);
		if (res<0) print ("ccn_put error.");
		
		// cleanup
		Egal.ccn_charbuf_destroy(ref uri);
		Egal.ccn_charbuf_destroy(ref name);
		Egal.ccn_charbuf_destroy(ref cb);
		Egal.ccn_charbuf_destroy(ref cob);
		Marshal.FreeHGlobal(pSp);
		
		return res;
	}
	
	Upcall.ccn_upcall_res WriteCallback(IntPtr selfp, Upcall.ccn_upcall_kind kind, IntPtr info)
	{
		print("WriteCallback... " + kind);
		Upcall.ccn_upcall_res ret = Upcall.ccn_upcall_res.CCN_UPCALL_RESULT_OK;
		
		Egal.ccn_upcall_info Info = new Egal.ccn_upcall_info();
		Info = (Egal.ccn_upcall_info)Marshal.PtrToStructure(info, typeof(Egal.ccn_upcall_info));
		IntPtr h = Info.h;
		
		Egal.ccn_closure Selfp = new Egal.ccn_closure();
		Selfp = (Egal.ccn_closure)Marshal.PtrToStructure(selfp, typeof(Egal.ccn_closure));
		NormalStruct Data = new NormalStruct();
		Data = (NormalStruct) Marshal.PtrToStructure(Selfp.data, typeof(NormalStruct));
		
		switch(kind)
		{
		case Upcall.ccn_upcall_kind.CCN_UPCALL_FINAL:
           	// Marshal.FreeHGlobal(selfp); // this again, will make Unity crash
			break;
			
		case Upcall.ccn_upcall_kind.CCN_UPCALL_INTEREST:	
			// print ("put content handle: " + h + ", content: " + Data.value);
			int res = PutContent(h, Data);
			if(res >= 0)
			{
				ret = Upcall.ccn_upcall_res.CCN_UPCALL_RESULT_INTEREST_CONSUMED;
				Egal.ccn_set_run_timeout(h, 0);
			}
			else
				print("put content error");
			
			break;
			
		case Upcall.ccn_upcall_kind.CCN_UPCALL_CONTENT:
			// repo first returns a content...
			// this is info of the repo...
			break;
			
		default:
			ret = Upcall.ccn_upcall_res.CCN_UPCALL_RESULT_ERR;
			break;
		}
		
		// print ("ref count: " + Selfp.refcount);
		// print ("WriteCallback returnning..." + ret);
		
		return ret;
		
	}
	
	public void WriteToRepo(System.String name, System.String content)
	{
		print ("Writing " + name + " to repo: " + content);
		
		int res;
		IntPtr h = GetHandle();
		IntPtr cb = Egal.ccn_charbuf_create();
		IntPtr nm = Egal.ccn_charbuf_create();
		IntPtr cmd = Egal.ccn_charbuf_create();
		
		Egal.ccn_name_from_uri(nm, name);
		Egal.ccn_create_version(h, nm, VersioningFlags.CCN_V_NOW, 0, 0);
		
		NormalStruct Data = new NormalStruct(nm, cb, h, content.Length, content);
		IntPtr pData = Marshal.AllocHGlobal(Marshal.SizeOf(Data));
		Marshal.StructureToPtr(Data, pData, true);
		
		IntPtr template = Egal.SyncGenInterest(IntPtr.Zero, 1, 4, -1, -1, IntPtr.Zero);
		
		Egal.ccn_closure Action = new Egal.ccn_closure(WriteCallback, pData, 0);
		IntPtr pnt = Marshal.AllocHGlobal(Marshal.SizeOf(Action));
		Marshal.StructureToPtr(Action, pnt, true);

		res = Egal.ccn_set_interest_filter(h, nm, pnt); // listen: interest
		
		res = Egal.ccn_charbuf_append_charbuf(cmd, nm);	
		res = Egal.ccn_name_from_uri(cmd, "%C1.R.sw");
		Egal.ccn_name_append_nonce(cmd);
		
		Egal.ccn_express_interest(h, cmd, pnt, template); // express interest
		
		Egal.ccn_run(h,-1);
		
		Egal.ccn_destroy(ref h);
		 
	}
	
	void AsteroidToRepo()
	{
		b612 = GameObject.Find ("b612");
		float pos_x = UnityEngine.Random.Range(0, 999f);
		float pos_y = UnityEngine.Random.Range(0, 999f);
		float pos_z = UnityEngine.Random.Range(0, 999f);
		Vector3 pos = new Vector3(pos_x, pos_y, pos_z);
		b612.transform.position = pos;
		
		System.String name = prefix + "/b612/" + UnityEngine.Random.Range(-999999, 999999);
		System.String content = "" + pos.x + "," + pos.y + "," + pos.z + ',' + b612.GetInstanceID();
		me = name;
		
		WriteToRepo(name, content);
	
		
	}
	
	/*
	void PlayerToRepo()
	{
		GameObject player = GameObject.Find("Player");
		float theta = UnityEngine.Random.Range(0, 360f);
		float pos_x = radius * Mathf.Sin(theta);
		float pos_y = 0;
		float pos_z = radius * Mathf.Cos(theta);
		Vector3 pos = new Vector3(pos_x, pos_y, pos_z);
		player.transform.position = pos;
		
		System.String name = prefix + "/players/" + UnityEngine.Random.Range(0, 9999);
		System.String content = "" + pos.x + "," + pos.y + "," + pos.z;
		me = name;
		
		WriteToRepo(name, content);
	}
	*/
	
	void OnApplicationQuit() 
	{
		print ("quitting...");
		print ("killing thread...");
		Egal.ccn_set_run_timeout(h_ccns_watch, 0);
		oThread.Abort();
		oThread.Join();
	}
	
	
}
