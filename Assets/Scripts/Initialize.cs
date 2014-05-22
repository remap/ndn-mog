using UnityEngine;

using System.Collections;
using System;
using System.Collections.Generic;

using remap.NDNMOG.DiscoveryModule;
using System.Threading;
using net.named_data.jndn.encoding;

using System.IO;

public class Initialize : MonoBehaviour {
	public const string configFilePath = "config.txt";
	
	public Instance instance;
	public string playerName;
	public string gameInstanceName;
	
	public static Transform playerTransformPath;
	
	public Hashtable hashtable = new Hashtable();
	
	public Mutex hashtableLock = new Mutex();
	public remap.NDNMOG.DiscoveryModule.Vector3 selfLocation = new remap.NDNMOG.DiscoveryModule.Vector3(0, 0, 0);
	public Transform selfTransform;
	
	public bool readConfFromFile(string fileName)
	{
		if (!File.Exists(fileName))
		{
			return false;	
		}
		else
		{
			using (StreamReader sr = File.OpenText(configFilePath))
        	{
            	string s = "";
            	while ((s = sr.ReadLine()) != null)
            	{
                	if (s.Contains("name"))
					{
						string[] name = s.Split(':');
						if (name.Length > 0)
						{
							playerName = name[1];
						}
						else
						{
							return false;	
						}
					}
					if (s.Contains("instance"))
					{
						string[] name = s.Split(':');
						if (name.Length > 0)
						{
							gameInstanceName = name[1];
						}
					}
            	}
        	}
			return true;
		}
	}
	
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
		
		Debug.Log(CommonUtility.getStringFromList(startingLoc));
		
		if (!readConfFromFile(configFilePath))
		{
			playerName = "default";
		}
		instance = new Instance(startingLoc, playerName, dollPos, setPosCallback); 
		
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
		List<string> toDelete = new List<string>();
		
		// Is generating a copy of hashtable a potentially better idea than this?
		hashtableLock.WaitOne(Constants.MutexLockTimeoutMilliSeconds);
		
		foreach (DictionaryEntry pair in hashtable)
		{
			GameObject entity = GameObject.Find((string)pair.Key);
			remap.NDNMOG.DiscoveryModule.Vector3 location = new remap.NDNMOG.DiscoveryModule.Vector3((remap.NDNMOG.DiscoveryModule.Vector3)pair.Value);
			//Debug.Log(location.x_);
			UnityEngine.Vector3 locationUnity = new UnityEngine.Vector3(location.x_, location.y_, location.z_);
				
			if (entity == null)
			{
				if (locationUnity.x != Constants.DefaultLocationDropEntity && locationUnity.x != Constants.DefaultLocationNewEntity)
				{
					Transform newEntity = Instantiate(playerTransformPath, locationUnity, Quaternion.identity) as Transform;
					newEntity.name = (string)pair.Key;
				}
				//newEntity.tag = "";
			}
			else
			{
				if (locationUnity.x != Constants.DefaultLocationDropEntity && locationUnity.x != Constants.DefaultLocationNewEntity)
				{
					entity.transform.localPosition = locationUnity;
				}
				else
				{
					Destroy(entity);
					
					// hashtable deletion in foreach loop is considered illegal
					toDelete.Add((string)pair.Key);
				}
			}
		}
		foreach (string str in toDelete)
		{
			hashtable.Remove(str);	
		}
		hashtableLock.ReleaseMutex();
	}
	
	public bool setPosCallback(string name, remap.NDNMOG.DiscoveryModule.Vector3 location)
	{
		if (location.Equals(new remap.NDNMOG.DiscoveryModule.Vector3(Constants.DefaultLocationNewEntity, Constants.DefaultLocationNewEntity, Constants.DefaultLocationNewEntity)))
		{
			Debug.Log("New entity " + name + " discovered from returned names");
			
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
		else if (location.Equals(new remap.NDNMOG.DiscoveryModule.Vector3(Constants.DefaultLocationDropEntity, Constants.DefaultLocationDropEntity, Constants.DefaultLocationDropEntity)))
		{
			Debug.Log("Entity " + name + " dropped.");
			
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
	
	public void Awake()
	{
		Application.targetFrameRate = 30;
	}
}