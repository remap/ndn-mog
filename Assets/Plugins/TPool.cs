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
		private ManualResetEvent[] doneEvents = null;
		
		public void Delete(IntPtr ccn)
		{
			rwl.EnterWriteLock();
			if(handles.ContainsKey(ccn) == true)
			{
				
				
				print("Delete: " + ccn + ", " + handles[ccn]);
				handles.Remove(ccn);
				
			}
         	rwl.ExitWriteLock();
		}
		
		public void Add(IntPtr ccn, string labels)
		{	
			
			rwl.EnterWriteLock();
			if(handles.ContainsKey(ccn) == false)
			{
				
				
				print("Add: " + ccn + ", " + labels);
				handles.Add (ccn, labels);
				
			}
			rwl.ExitWriteLock();
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
				Dictionary<IntPtr, string> copy = new Dictionary<IntPtr, string>(handles);
				PrintDic();
				rwl.ExitReadLock();
				
				int size = copy.Count;
				doneEvents = new ManualResetEvent[size];
				int index = 0;
				foreach(IntPtr h in copy.Keys)
				{
					//print("Run: " + h + ", " + copy[h]);
					doneEvents[index] = new ManualResetEvent(false);
					HandleState state = new HandleState(h, 20, doneEvents[index]);
					ThreadPool.QueueUserWorkItem(ThreadRun,state);
					index++;
				}
				print("doneEvents array size: " + doneEvents.Length);
				if(doneEvents.Length>27)
				{
					print("Abnormal doneEvents array size!");
				}
				WaitHandle.WaitAll(doneEvents);
				doneEvents = null;
				
				yield return null;
			}
			
			yield break;
		}
		
		public void ThreadRun(System.Object state)
		{
			HandleState hs = (HandleState)state;
			Egal.ccn_run (hs.ccn, hs.timeout);
			hs.doneEvent.Set();
		}
		
		public void Clear()
		{
			rwl.EnterWriteLock();
			handles.Clear();
			rwl.ExitWriteLock();
		}
		
		public void PrintDic()
		{
			string str = "";
			foreach(KeyValuePair<IntPtr, string> p in handles)
			{
				str = str + p.Key + "," + p.Value + "    ";
			}
			print ("Print Dictionary: " + str);
		}
	}
}



	