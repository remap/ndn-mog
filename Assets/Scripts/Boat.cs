using UnityEngine;
using System.Collections;

public class Boat : MonoBehaviour {

	
	void Update () {
		CharacterController controller = GetComponent<CharacterController>();
		if(controller.isGrounded)
		{
		}
	}
}
