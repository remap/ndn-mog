using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.Threading;

public class TPool : MonoBehaviour {

	public static HandlePool AllHandles = new HandlePool();
	
	private static ReaderWriterLockSlim rwl = new ReaderWriterLockSlim();
	
	void Start () {
		
		StartCoroutine(AllHandles.Run());
	}
	
	void OnApplicationQuit()
	{
		AllHandles.Clear();
	}
	
	public class HandlePool
	{
		// dictionary <handle, labels>
		private Dictionary<IntPtr, string> handles = new Dictionary<IntPtr, string>(); 
		
		public void Delete(IntPtr ccn)
		{
			
			
			if(handles.ContainsKey(ccn) == true)
			{
				rwl.EnterWriteLock();
				print("Delete: " + ccn + ", " + handles[ccn]);
				handles.Remove(ccn);
				rwl.ExitWriteLock();
			}
         	 
		}
		
		public void Add(IntPtr ccn, string labels)
		{	
			
			print("Add: " + ccn + ", " + labels);
			if(handles.ContainsKey(ccn) == false)
			{
				rwl.EnterWriteLock();
				handles.Add (ccn, labels);
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
					print("Run: " + h + ", " + handles[h]);

					HandleState state = new HandleState(h, 20);
					ThreadPool.QueueUserWorkItem(Egal.run,state);
				}
				rwl.ExitReadLock();
								
				yield return null;
			}
			
			yield break;
		}
		
		public void Clear()
		{
			rwl.EnterWriteLock();
			handles.Clear();
			rwl.ExitWriteLock();
		}
	}
}



	