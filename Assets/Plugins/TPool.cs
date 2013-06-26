using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.Threading;

public class TPool : MonoBehaviour {

	public static HandlePool AllHandles = new HandlePool();
	private static Mutex mut = new Mutex();
	
	void Start () {
		
		StartCoroutine(AllHandles.Run());;
	}
	
	public class HandlePool
	{
		// dictionary <handle, time when the closure was last called back>
		private Dictionary<IntPtr, float> handles = new Dictionary<IntPtr, float>(); 
		
		
		public void Delete(IntPtr ccn)
		{
			mut.WaitOne();
			print("Delete");
			if(handles.ContainsKey(ccn) == true)
			{
				handles.Remove(ccn);
			}
         	mut.ReleaseMutex(); 
		}
		
		public void Add(IntPtr ccn)
		{	
			mut.WaitOne();
			print("Add");
			if(handles.ContainsKey(ccn) == false)
			{
				handles.Add (ccn, Time.time);
			}
      		mut.ReleaseMutex();
		}	
		
		public void Update(IntPtr ccn)
		{
			print("Update");
			mut.WaitOne();
			if(handles.ContainsKey(ccn) == false)
			{
				handles.Add (ccn, Time.time);
			}
			else
			{
				handles[ccn] = Time.time;
			}
			mut.ReleaseMutex();
		}
		
		public IEnumerator Run()
		{
			while(Application.isPlaying)
			{
				while(handles.Count==0)
				{
					yield return new WaitForSeconds(0.5f);
				}
				
				//print("outside mutex");
				mut.WaitOne();
				List<IntPtr> keystodelete = new List<IntPtr>();
				foreach(IntPtr h in handles.Keys)
				{
					print("Run: " + h + "long... long... long... long... long... long... long... long... long... long... long... long...");
					float delta = Time.time - handles[h];
					if(delta>3)
					{
						keystodelete.Add(h);
						continue;
					}
					HandleState state = new HandleState(h, 20);
					ThreadPool.QueueUserWorkItem(Egal.run,state);
				}
				foreach(IntPtr k in keystodelete)
				{
					handles.Remove(k);
				}
				mut.ReleaseMutex();
				
				yield return null;
			}
		}
		
		
	}
}



	