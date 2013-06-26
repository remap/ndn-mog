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
		private HashSet<IntPtr> handles = new HashSet<IntPtr>(); // HashSet does not allow duplicates
		private bool readerFlag = false;
		
		public void Delete(IntPtr ccn)
		{
			mut.WaitOne();
			print("Delete");
			if(handles.Contains(ccn) == true)
			{
				handles.Remove(ccn);
			}
         	mut.ReleaseMutex(); 
		}
		
		public void Add(IntPtr ccn)
		{	
			mut.WaitOne();
			print("Add");
			if(handles.Contains(ccn) == false)
			{
				handles.Add (ccn);
			}
      		mut.ReleaseMutex();
		}	
		
		public void Update(IntPtr ccn, float last_active_time)
		{
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
				foreach(IntPtr h in handles)
				{
					print("Run: " + h + "long... long... long... long... long... long... long... long... long... long... long... long...");
					HandleState state = new HandleState(h, 20);
					ThreadPool.QueueUserWorkItem(Egal.run,state);
				}
				mut.ReleaseMutex();
				
				yield return null;
			}
		}
		
		
	}
}



	