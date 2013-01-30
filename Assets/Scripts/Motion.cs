using UnityEngine;
using System.Collections;

public class Motion : MonoBehaviour {

	public float rotate_speed = 90f; // for rotating around 
	public float zoom_speed = 90f; // for zooming
	public float min_bound = 60f;
	public float max_bound = 300f;
	
	void Update () {
		float rotate_movement = Input.GetAxis("Horizontal") * rotate_speed;
		transform.RotateAround (Vector3.zero, Vector3.up, (-1) * rotate_movement * Time.deltaTime);
		float zoom_movement = Input.GetAxis("Vertical") * zoom_speed;
		if(SafeInBound(zoom_movement)==true)
		{
			transform.Translate(Vector3.forward * zoom_movement * Time.deltaTime);
		}
		
		
	}
	
	bool SafeInBound(float movement)
	{
		bool safe = true;
		float dis = Vector3.Distance(Vector3.zero, this.transform.position);
		if(dis<=min_bound && movement>0)
		{
			safe=false;
		}
		else if(dis>=max_bound && movement<0)
		{
			safe=false;
		}
		return safe;
	}
}
