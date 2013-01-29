using UnityEngine;
using System.Collections;

public class Motion : MonoBehaviour {

	public float speed = 90f;
	
	void Update () {
		float movement = Input.GetAxis("Horizontal") * speed;
		transform.RotateAround (Vector3.zero, Vector3.up, (-1) * movement * Time.deltaTime);
	}
}
