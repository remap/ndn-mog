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

public struct HandleNode
{
	public IntPtr ccn;
	public float last_active_time;
	public HandleNode(IntPtr c, int t)
	{
		this.ccn = c;
		this.last_active_time = t;
	}
};