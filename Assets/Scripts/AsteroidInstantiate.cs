using UnityEngine;
using System;
using remap.NDNMOG.DiscoveryModule;

public class AsteroidInstantiate : MonoBehaviour
{
	
	public static Transform Tree2; // prefab for asteroids
	public static Transform AsteroidParent; // parent of asteroids
		
	public static void init ()
	{
		Tree2 = GameObject.Find ("/tree2").transform;
		AsteroidParent = GameObject.Find ("/Asteroid").transform;
	}
	
	public static Transform MakeAnAsteroid (string id, UnityEngine.Vector3 position, bool activate = false)
	{
		// Generate DiscoveryModule.Vector3 from UnityEngine.Vector3 
		remap.NDNMOG.DiscoveryModule.Vector3 vector3 = new remap.NDNMOG.DiscoveryModule.Vector3 (position.x, position.y, position.z);
		string label = CommonUtility.GetLabel (vector3);
		
		Transform newAsteroid = Instantiate (Tree2, position, Quaternion.identity) as Transform;
		
		newAsteroid.name = id;
		//newAsteroid.transform.localScale = new Vector3(1000f,1000f,1000f);
		newAsteroid.tag = "Asteroid";
		newAsteroid.parent = AsteroidParent;
		newAsteroid.Find ("label").GetComponent<GUIText> ().text = "/asteroid/" + id + "\n" + "/asteroid/octant/" + label + "/" + id;
		ControlLabels.ApplyAsteroidName (newAsteroid);
		
		if (activate == true) {
			newAsteroid.GetComponent<TreeScript> ().Activate ();
		}
		return newAsteroid;
	}
}
