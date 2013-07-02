using UnityEngine;
using System.Collections;
using System;
using System.Threading;

public class Handle : MonoBehaviour {

	public static IntPtr ccn; // handle
	private static ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
    private static ManualResetEvent _pauseEvent = new ManualResetEvent(true);
    private static Thread _thread;
	private static bool IsPlaying = true;
	
	void Start () {
		ccn = Egal.GetHandle(); 
		_thread = new Thread(Run);
        _thread.Start();
		
	}
	
	void OnApplicationQuit()
	{
		//_shutdownEvent.Set();
        //_pauseEvent.Set();
		IsPlaying = false;
		//Egal.ccn_set_run_timeout(ccn, 0);
		_thread.Abort();
		_thread.Join();
	}
	
	public void Run () 
	{
		while(IsPlaying)
		{
			_pauseEvent.WaitOne(Timeout.Infinite);
			if (_shutdownEvent.WaitOne(0))
                break;
            Egal.ccn_run(ccn, 20);
		}
		
	}
	
	public static void Pause()
    {
		print("Pause()");
        _pauseEvent.Reset();
    }

    public static void Resume()
    {
		print("Resume()");
        _pauseEvent.Set();
    }

	
}
