using UnityEngine;
using System.Collections;
using System;
using System.Threading;

public struct HandleState
{
		public IntPtr ccn;
		public int timeout;
		public ManualResetEvent doneEvent;
	
		public HandleState(IntPtr c, int t, ManualResetEvent de)
		{
			this.ccn = c;
			this.timeout = t;
			this.doneEvent = de;
		}
};

