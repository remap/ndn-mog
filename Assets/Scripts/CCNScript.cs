using UnityEngine;
using System.Collections;
using System;

public class CCNScript : MonoBehaviour {

	public static IntPtr GetHandle()
	{
		
		IntPtr ccn = Egal.ccn_create();
		if (Egal.ccn_connect(ccn, "") == -1) 
        	print("could not connect to ccnd.");
		//else
			//print ("a handle is connected to ccnd.");
		return ccn;
	}
}
