using UnityEngine;
using System.Collections;

public class switchRole : MonoBehaviour {

	public GameObject FPS;
	public GameObject TPS;
	
	void Update () {
	
		if (Input.GetKeyUp (KeyCode.Escape))
		{
			FPS.SetActiveRecursively( !FPS.active );
			TPS.SetActiveRecursively( !TPS.active );
		}
	}
}
