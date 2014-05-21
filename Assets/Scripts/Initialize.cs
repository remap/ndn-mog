using UnityEngine;

using System.Collections;
using System;
using System.Collections.Generic;

using remap.NDNMOG.DiscoveryModule;
using System.Threading;
using net.named_data.jndn.encoding;

public class Initialize : MonoBehaviour {
	public Instance instance;
	
	public static Transform playerTransformPath;
	
	public Hashtable hashtable = new Hashtable();
	
	public Mutex hashtableLock = new Mutex();
	public remap.NDNMOG.DiscoveryModule.Vector3 selfLocation = new remap.NDNMOG.DiscoveryModule.Vector3(0, 0, 0);
	public Transform selfTransform;
	
	public void Start () {
		//WireFormat.setDefaultWireFormat(BinaryXmlWireFormat.get());
		
		AsteroidInstantiate.init();
		playerTransformPath = GameObject.Find("/player/graphics").transform;
		
		UnityEngine.Vector3 initLoc = new UnityEngine.Vector3(6750, 3500, 4800);
		
		string initId = "10";
		
		UnityEngine.Vector3 pos = AsteroidInstantiate.MakeAnAsteroid(initId, initLoc, true);
		
		UnityEngine.Vector3 dollPosUnity = pos + new UnityEngine.Vector3(0, 50, 0);
		transform.position = dollPosUnity;
		
		remap.NDNMOG.DiscoveryModule.Vector3 dollPos = new remap.NDNMOG.DiscoveryModule.Vector3(dollPosUnity.x, dollPosUnity.y, dollPosUnity.z);
		
		List<int> startingLoc = CommonUtility.getOctantIndicesFromVector3(dollPos);
		
		string name = "zening";
		Debug.Log(CommonUtility.getStringFromList(startingLoc));
		
		instance = new Instance(startingLoc, name, dollPos, setPosCallback); 
		
		instance.discovery ();

		// instance is interested in its starting location
		instance.trackOctant (instance.getOctantByIndex(startingLoc));
	}
	
	public void Update()
	{
		selfTransform = GameObject.Find("/player").transform;
		selfLocation.x_ = selfTransform.localPosition.x;
		selfLocation.y_ = selfTransform.localPosition.y;
		selfLocation.z_ = selfTransform.localPosition.z;
		
		instance.getSelfGameEntity().setLocation(selfLocation, false);
		
		hashtableLock.WaitOne(Constants.MutexLockTimeoutMilliSeconds);
		foreach (DictionaryEntry pair in hashtable)
		{
			GameObject entity = GameObject.Find((string)pair.Key);
			remap.NDNMOG.DiscoveryModule.Vector3 location = new remap.NDNMOG.DiscoveryModule.Vector3((remap.NDNMOG.DiscoveryModule.Vector3)pair.Value);
			//Debug.Log(location.x_);
			UnityEngine.Vector3 locationUnity = new UnityEngine.Vector3(location.x_, location.y_, location.z_);
				
			if (entity == null)
			{
				Transform newEntity = Instantiate(playerTransformPath, locationUnity, Quaternion.identity) as Transform;
				newEntity.name = (string)pair.Key;
				//newEntity.tag = "";
			}
			else
			{
				entity.transform.localPosition = locationUnity;
			}
		}
		hashtableLock.ReleaseMutex();
	}
	
	public void Awake()
	{
		Application.targetFrameRate = 30;
	}
	
	public bool setPosCallback(string name, remap.NDNMOG.DiscoveryModule.Vector3 location)
	{
		if (location.Equals(new remap.NDNMOG.DiscoveryModule.Vector3(Constants.DefaultLocationNewEntity, Constants.DefaultLocationNewEntity, Constants.DefaultLocationNewEntity)))
		{
			Debug.Log("New entity " + name + " discovered from returned names");
		}
		else if (location.Equals(new remap.NDNMOG.DiscoveryModule.Vector3(Constants.DefaultLocationDropEntity, Constants.DefaultLocationDropEntity, Constants.DefaultLocationDropEntity)))
		{
			Debug.Log("Entity " + name + " dropped.");
		}
		else
		{
			hashtableLock.WaitOne(Constants.MutexLockTimeoutMilliSeconds);
			if (hashtable.Contains(name))
			{
				hashtable[name] = location;
			}
			else
			{
				hashtable.Add(name, location);
			}
			hashtableLock.ReleaseMutex();
		}
		return true;
	}
	
	public void OnApplicationQuit()
	{
		// if somehow this method is not called, the discovery thread will not be stopped
		instance.stopDiscovery();
	}
}