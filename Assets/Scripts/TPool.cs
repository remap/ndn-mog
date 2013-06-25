using UnityEngine;
using System.Collections;

public class TPool : MonoBehaviour {

	public static HandlePool AllHandles = new HandlePool();
	
	void Start () {
	
		AllHandles.Run();
	}
	
	
}



	