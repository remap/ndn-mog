using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Runtime.InteropServices;

public class CCNScript : MonoBehaviour {
	
	
	
	
	public static Thread ccnRun(IntPtr ccn, int time)
	{
		
		Thread oThread = new Thread(() => run(ccn, time));
      	oThread.Start();
		return oThread;
	}
	
	public static void run(IntPtr ccn, int time)
	{
		Thread t = Thread.CurrentThread;
		// print (t.IsAlive);
		while(t.IsAlive == true)
		{
			//while(counter_for_run>0)
				//;
			Egal.ccn_run(ccn, time);
			
		}
		
	}
	
	public static IntPtr GetHandle()
	{
		
		IntPtr ccn = Egal.ccn_create();
		if (Egal.ccn_connect(ccn, "") == -1) 
        	print("could not connect to ccnd.");
		else
			print ("a handle is connected to ccnd.");
		return ccn;
	}
	
	public static int ExpressInterest(IntPtr ccn, string name, Egal.ccn_handler callback, IntPtr template)
	{
		IntPtr nm = Egal.ccn_charbuf_create();
		Egal.ccn_name_from_uri(nm,name);
		Egal.ccn_closure Action = new Egal.ccn_closure(callback, IntPtr.Zero, 0);
		IntPtr pnt = Marshal.AllocHGlobal(Marshal.SizeOf(Action));
		Marshal.StructureToPtr(Action, pnt, true);
		
		Egal.ccn_express_interest(ccn,nm,pnt,template);
		
		return 0;
	}
	
	public static void killCurrentThread() 
	{
		print ("killing thread...");
		Thread oThread = Thread.CurrentThread;
		oThread.Abort();
		oThread.Join();
	}
}
