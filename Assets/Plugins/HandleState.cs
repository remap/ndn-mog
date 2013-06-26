using UnityEngine;
using System.Collections;
using System;

public struct HandleState
{
		public IntPtr ccn;
		public int timeout;
		public HandleState(IntPtr c, int t)
		{
			this.ccn = c;
			this.timeout = t;
		}
};

