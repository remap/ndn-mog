using UnityEngine;
using System.Collections;

public class CheckMode : MonoBehaviour {

	public string OnAsteroid = null;
	public static bool start = false;
	private static GameObject asteroidparent;
	
	IEnumerator Start()
	{
		CharacterController controller = GetComponent<CharacterController>();
		while(controller.isGrounded == false)
			yield return new WaitForSeconds(0.1f);
		asteroidparent = GameObject.Find("Asteroid");
		OnAsteroid = FindNearestAsteroid();
		
		start = true;
	}
	
	void Update () {
		
		if(start== false)
			return;
		
		string homeasteroid = FindNearestAsteroid();
		
		if(homeasteroid != OnAsteroid)
		{
			print("Change of Mode!");
			OnAsteroid = homeasteroid;
			ChangeMode(OnAsteroid);
		}
		
		
	}
	
	public void ChangeMode(string homeast)
	{
		GameObject boat = transform.Find("graphics/boat").gameObject;
		if(homeast == null) // change to fly mode
		{
			boat.SetActiveRecursively(true);
			move.currentmode = (int)move.Mode.fly;
		}
		else // change to walk mode
		{
			boat.SetActiveRecursively(false);
			move.currentmode = (int)move.Mode.walk;
		}
	}
	
	public string FindNearestAsteroid()
	{
		
		string homeasteroid = null;
		float mindistance = 14;
		foreach(Transform a in asteroidparent.transform)
		{
			Transform cap = a.Find("cap");
			float distance = Vector3.Distance(transform.position, cap.position);
			if(distance<=mindistance && transform.position.y > cap.position.y)
			{
				mindistance = distance;
				homeasteroid = a.name;
			}
		}
		return homeasteroid;
	}
}
