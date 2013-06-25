using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.Threading;

public class HandlePool : MonoBehaviour
{
	private List<IntPtr> handles;
	private bool readerFlag = false;
		
	void Start()
	{
		print("Start of something new!");
		this.Run ();
	}
		
	public void Delete(IntPtr ccn)
		{
			
			lock(this)
      		{
         		if (!readerFlag)
         		{            
            		try
            		{
               			Monitor.Wait(this);
            		}
            		catch (SynchronizationLockException e)
            		{
               			Console.WriteLine(e);
            		}
            		catch (ThreadInterruptedException e)
            		{
               			Console.WriteLine(e);
            		}
         		}
         		// delete here
				handles.Remove(ccn);
         		readerFlag = false;    
         		Monitor.Pulse(this);   
      		}   
		}
		
	public void Add(IntPtr ccn)
		{	
			lock(this)  
      		{
         		if (readerFlag)
         		{      
            		try
            		{
               			Monitor.Wait(this);   
            		}
            		catch (SynchronizationLockException e)
            		{
               			Console.WriteLine(e);
            		}
            		catch (ThreadInterruptedException e)
            		{
               			Console.WriteLine(e);
            		}
         		}
         		// add here
				handles.Add (ccn);
         		readerFlag = true;   
         		Monitor.Pulse(this);  
      		}   
		}	
		
	public IEnumerator Run()
		{
			print("Handle Pool starts to run.");
			while(Application.isPlaying)
			{
				if(handles.Count==0)
				{
					yield return new WaitForSeconds(0.5f);
				}
				else
				{
					foreach(IntPtr h in handles)
					{
						HandleState state = new HandleState(h, 200);
						ThreadPool.QueueUserWorkItem(Egal.run,state);
					}
					return true;
				}	
				
			}
		}
}
	