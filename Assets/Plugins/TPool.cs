using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.Threading;

public class TPool : MonoBehaviour {

	public static HandlePool AllHandles = new HandlePool();
	private static Mutex mut = new Mutex();
	private static ReaderWriterLockSlim rwl = new ReaderWriterLockSlim();
	
	void Start () {
		
		StartCoroutine(AllHandles.Run());
	}
	
	public class HandlePool
	{
		// dictionary <handle, time when the closure was last called back>
		private Dictionary<IntPtr, float> handles = new Dictionary<IntPtr, float>(); 
		
		
		public void Delete(IntPtr ccn)
		{
			
			print("Delete: " + ccn);
			if(handles.ContainsKey(ccn) == true)
			{
				rwl.EnterWriteLock();
				handles.Remove(ccn);
				rwl.ExitWriteLock();
			}
         	 
		}
		
		public void Add(IntPtr ccn)
		{	
			
			print("Add: " + ccn);
			if(handles.ContainsKey(ccn) == false)
			{
				rwl.EnterWriteLock();
				handles.Add (ccn, DateTime.Now.Second);
				rwl.ExitWriteLock();
			}
      		
		}	
		

		public IEnumerator Run()
		{
			while(Application.isPlaying)
			{
				while(handles.Count==0)
				{
					yield return new WaitForSeconds(0.5f);
				}
					
				rwl.EnterReadLock();
				foreach(IntPtr h in handles.Keys)
				{
					//print("Run: " + h);

					HandleState state = new HandleState(h, 20);
					ThreadPool.QueueUserWorkItem(Egal.run,state);
				}
				rwl.ExitReadLock();
								
				yield return null;
			}
		}
		
		
	}
}



	