using UnityEngine;
using System.Collections;

public class switchRole : MonoBehaviour {

	public GameObject FPS;
	public GameObject TPS;
	public bool FirstPersonView;
	
	void Start()
	{
		FPS.SetActiveRecursively(false);
		TPS.SetActiveRecursively(true);
		FirstPersonView = false;
	}
	
	void Update () {
	
		if (Input.GetKeyUp (KeyCode.Escape))
		{
			FPS.SetActiveRecursively( !FPS.active );
			TPS.SetActiveRecursively( !TPS.active );
			FirstPersonView = !FirstPersonView;
		}
	}
}
