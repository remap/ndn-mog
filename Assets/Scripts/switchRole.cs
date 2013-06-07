using UnityEngine;
using System.Collections;

public class switchRole : MonoBehaviour {

	
	Transform FPS;
	Transform TPS;
	
	void Start()
	{
		FPS = transform.Find("FPS");
		TPS = transform.Find("TPS");
		
		FPS.gameObject.SetActiveRecursively(false);
		TPS.gameObject.SetActiveRecursively(true);
		
	}
	
	void Update () {
	
		if (Input.GetKeyUp (KeyCode.Escape))
		{
			FPS.gameObject.SetActiveRecursively( !FPS.active );
			TPS.gameObject.SetActiveRecursively( !TPS.active );
			
		}
	}
}
