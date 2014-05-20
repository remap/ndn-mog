using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

public class Initialize : MonoBehaviour {
	
	public void Start () {
		AsteroidInstantiate.init();
		
		LandOnFirstAsteroid();
	}
	
	
	void LandOnFirstAsteroid()
	{
		Vector3 initLoc = new Vector3(0, 0, 0);
		string initId = "10";
		
		Vector3 pos = AsteroidInstantiate.MakeAnAsteroid(initId, initLoc, true);
		
		Vector3 dollpos = pos + new Vector3(0, 50, 0);
		transform.position = dollpos;
	}
	
	/*
	public void Awake()
	{
		Application.targetFrameRate = 30;
	}
	*/
}
