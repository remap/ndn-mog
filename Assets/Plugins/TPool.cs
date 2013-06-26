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
		
//		public void Update(IntPtr ccn)
//		{
//			print("Update");
//			rwl.EnterWriteLock();
//			if(handles.ContainsKey(ccn) == false)
//			{
//				handles.Add (ccn, DateTime.Now.Second);
//			}
//			else
//			{
//				handles[ccn] = DateTime.Now.Second;
//			}
//			rwl.ExitWriteLock();
//			
//		}
		
		public IEnumerator Run()
		{
			while(Application.isPlaying)
			{
				while(handles.Count==0)
				{
					yield return new WaitForSeconds(0.5f);
				}
				
				//print("outside mutex");
				//mut.WaitOne();
				
//				List<IntPtr> keystodelete = new List<IntPtr>();
				
//				var enumerator = handles.Keys.GetEnumerator();
//    			while (enumerator.MoveNext())
//				{
//					IntPtr h = enumerator.Current;
//					print("Run: " + h + "long... long... long... long... long... long... long... long... long... long... long... long...");
//					float delta = DateTime.Now.Second - handles[h];
//					if(delta>3)
//					{
//						keystodelete.Add(h);
//						continue;
//					}
//					HandleState state = new HandleState(h, 20);
//					ThreadPool.QueueUserWorkItem(Egal.run,state);
//				}
				
				rwl.EnterReadLock();
				foreach(IntPtr h in handles.Keys)
				{
					//print("Run: " + h + "long... long... long... long... long... long... long... long... long... long... long... long...");
//					float delta = DateTime.Now.Second - handles[h];
//					if(delta>3)
//					{
//						keystodelete.Add(h);
//						continue;
//					}
					HandleState state = new HandleState(h, 20);
					ThreadPool.QueueUserWorkItem(Egal.run,state);
				}
				rwl.ExitReadLock();
				
//				rwl.EnterWriteLock();
//				foreach(IntPtr k in keystodelete)
//				{
//					handles.Remove(k);
//				}
//				rwl.ExitWriteLock();
				
				yield return null;
			}
		}
		
		
	}
}



	