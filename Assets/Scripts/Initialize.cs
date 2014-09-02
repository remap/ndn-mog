using UnityEngine;

using System.Collections;
using System;
using System.Collections.Generic;

using remap.NDNMOG.DiscoveryModule;
using System.Threading;
using net.named_data.jndn.encoding;

using System.IO;

public class Initialize : MonoBehaviour
{
	public int unUsed = 0;
	
	public Instance instance;
	public string playerName;
	public string gameInstanceName;
	
	public const string playerTransformPath = "/player/graphics";
	public const string cubeTransformPath = "/octant";
	public const string selfTransformPath = "/player";
	public const string minimapPrefix = "minimap";
	
	public const int distanceThreshold = 600;
	
	public static Transform playerTransform;
	public static Transform cubeTransform;
	
	public Hashtable hashtable = new Hashtable ();
	
	public Hashtable trackedOctant = new Hashtable ();
	
	public Mutex hashtableLock = new Mutex ();
	public remap.NDNMOG.DiscoveryModule.Vector3 selfLocation = new remap.NDNMOG.DiscoveryModule.Vector3 (0, 0, 0);
	public Transform selfTransform;
	
	public string loggingLevel = "none";
	public bool instantiated = false;
	
	public bool writeLog(string str)
	{
		if (UnityConstants.logFileEnabled)
		{
			// This method is not thread-safe, which could be one of the causes of Unity crashing my interest expression threads.
			System.IO.File.AppendAllText (UnityConstants.gameLogName + playerName + ".txt", str);	
		}
		return UnityConstants.logFileEnabled;
	}
	
	public bool libraryWriteCallback(string type, string info)
	{
		if (UnityConstants.logFileEnabled)
		{
			System.IO.File.AppendAllText (UnityConstants.libraryLogName + playerName + ".txt", type + " - " + info + "\n");	
		}
		return UnityConstants.logFileEnabled;
	}
	
	public bool readConfFromFile (string fileName)
	{
		if (!File.Exists (fileName)) {
			return false;	
		} else {
			using (StreamReader sr = File.OpenText(fileName)) {
				string s = "";
				while ((s = sr.ReadLine()) != null) {
					if (s.Contains ("name")) {
						string[] name = s.Split (':');
						if (name.Length > 0) {
							playerName = name [1];
							writeLog( "Name of player: " + playerName + "\n");
						} else {
							return false;	
						}
					}
					if (s.Contains ("instance")) {
						string[] name = s.Split (':');
						if (name.Length > 0) {
							gameInstanceName = name [1];
							writeLog( "Name of instance: " + gameInstanceName + "\n");
						}
					}
					if (s.Contains ("logging-level")) {
						string[] name = s.Split (':');
						if (name.Length > 0) {
							loggingLevel = name [1];
							writeLog( "Loggin level: " + loggingLevel + "\n");
						}
					}
				}
			}
			return true;
		}
	}
	
	public bool nullFunc(string s1, string s2)
	{
		return false;
	}
	
	public void instantiateOctant(string name, int xmin, int ymin, int zmin)
	{
		GameObject entity = GameObject.Find (name);
		
		if (entity == null)
		{
			// Create octant by code
			/*
			GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cube.transform.localScale = new UnityEngine.Vector3(512, 512, 512);
			cube.transform.localPosition = new UnityEngine.Vector3(xmin + 256, ymin + 256, zmin + 256);
			
			// Generic function and as keyword...
			MeshRenderer renderer = cube.AddComponent<MeshRenderer>();
			
			renderer.material = new Material(Shader.Find("Transparent/Diffuse"));
			renderer.material.color = new Color(1, 0, 0, 0.5f);
			*/
			
			// Create octant using existing path to a GameObject
			UnityEngine.Vector3 locationUnity = new UnityEngine.Vector3 (xmin, ymin, zmin);
			Transform octant = Instantiate (cubeTransform, locationUnity, Quaternion.identity) as Transform;
			
			octant.name = name;
			octant.renderer.material.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f, 0.5f));
			octant.renderer.material.shader = Shader.Find( "Transparent/Diffuse" );
			
		}
		else
		{
			//Debug.Log("octant " + name + " already instantiated.");	
		}
	}
	
	public void destroyOctant(string name)
	{
		// deinstantiateOctant does not automatically add minimap prefix
		GameObject entity = GameObject.Find(name);
		if (entity == null)
		{
			//Debug.Log("octant " + name + " does not exist.");
		}
		else
		{
			Destroy(entity);
		}
	}
	
	public void Start ()
	{
		//WireFormat.setDefaultWireFormat(BinaryXmlWireFormat.get());
		
		AsteroidInstantiate.init ();
		playerTransform = GameObject.Find (playerTransformPath).transform;
		cubeTransform = GameObject.Find(cubeTransformPath).transform;
		
		UnityEngine.Vector3 initLoc = new UnityEngine.Vector3 (6750, 3500, 4800);
		
		string initId = "10";
		
		UnityEngine.Vector3 pos = AsteroidInstantiate.MakeAnAsteroid (initId, initLoc, true);
		
		UnityEngine.Vector3 dollPosUnity = pos + new UnityEngine.Vector3 (0, 50, 0);
		transform.position = dollPosUnity;
		
		remap.NDNMOG.DiscoveryModule.Vector3 dollPos = new remap.NDNMOG.DiscoveryModule.Vector3 (dollPosUnity.x, dollPosUnity.y, dollPosUnity.z);
		
		List<int> startingOct = CommonUtility.getOctantIndicesFromVector3 (dollPos);
		Debug.Log (CommonUtility.getStringFromList (startingOct));
		
		if (!readConfFromFile (UnityConstants.configFilePath)) {
			playerName = "default";
			writeLog("Didn't parse config file, using default as name.");
		}
		if (loggingLevel != "none")
		{
			instance = new Instance (startingOct, playerName, dollPos, setPosCallback, libraryWriteCallback); 
		}
		else
		{
			instance = new Instance (startingOct, playerName, dollPos, setPosCallback, nullFunc); 
		}
		instance.discovery ();
		
		instantiated = true;
		// instance is interested in its starting location
		trackOctant (startingOct);
	}
	
	// updateOctantList decides which octants should be tracked, for now it's called per frame, which is not necessary
	public int updateOctantList(int x, int y, int z)
	{
		int xmin = 0;
		int ymin = 0;
		int zmin = 0;
		
		List<int> octList = CommonUtility.getOctantIndicesFromVector3 (new remap.NDNMOG.DiscoveryModule.Vector3(x, y, z));
		string octantName = CommonUtility.getStringFromList (octList);
		CommonUtility.GetBoundaries(octantName, ref xmin, ref ymin, ref zmin);
		
		List<string> toDelete = new List<string>();
		// naive strategy for untracking octants: for each octants that's not in range, we remove the octants from tracking list
		foreach (DictionaryEntry pair in trackedOctant) {
			UnityEngine.Vector3 position = (UnityEngine.Vector3)pair.Value;
			if (UnityEngine.Vector3.Distance(position, new UnityEngine.Vector3(x, y, z)) > distanceThreshold)
			{
				List<int> newOctList = CommonUtility.getOctantIndicesFromVector3(new remap.NDNMOG.DiscoveryModule.Vector3(position.x, position.y, position.z));
				untrackOctant(newOctList);
				toDelete.Add((string)pair.Key);
			}
		}
		// Separate deletion from traversal so that snapshot does not fall out of sync
		foreach (string str in toDelete) {
			trackedOctant.Remove (str);	
		}
		
		// Z direction on one direction does not work correctly, checking out reasons...
		if ((x - xmin > 462) && (x + 512 < 65536))
		{
			List<int> newOctList = CommonUtility.getOctantIndicesFromVector3 (new remap.NDNMOG.DiscoveryModule.Vector3(x + 512, y, z));
			trackOctant (newOctList);
		}
		if ((x - xmin < 50) && (x - 512 > 0))
		{
			List<int> newOctList = CommonUtility.getOctantIndicesFromVector3 (new remap.NDNMOG.DiscoveryModule.Vector3(x - 512, y, z));
			trackOctant (newOctList);
		}
		if ((y - ymin > 462) && (y + 512 < 65536))
		{
			List<int> newOctList = CommonUtility.getOctantIndicesFromVector3 (new remap.NDNMOG.DiscoveryModule.Vector3(x, y + 512, z));
			trackOctant (newOctList);
		}
		if ((y - ymin < 50) && (y - 512 > 0))
		{
			List<int> newOctList = CommonUtility.getOctantIndicesFromVector3 (new remap.NDNMOG.DiscoveryModule.Vector3(x, y - 512, z));
			trackOctant (newOctList);
		}
		if ((z - zmin > 462) && (z + 512 < 65536))
		{
			List<int> newOctList = CommonUtility.getOctantIndicesFromVector3 (new remap.NDNMOG.DiscoveryModule.Vector3(x, y, z + 512));
			trackOctant (newOctList);
		}
		if ((z - zmin < 50) && (z - 512 > 0))
		{
			List<int> newOctList = CommonUtility.getOctantIndicesFromVector3 (new remap.NDNMOG.DiscoveryModule.Vector3(x, y, z - 512));
			trackOctant (newOctList);
		}
		
		return 1;
	}
	
	public int untrackOctant(List<int> octList)
	{
		Octant oct = instance.getOctantByIndex(octList);
		// what is the relationship between tracked octant and kept octant in DiscoverModule
		if (oct != null) {
			instance.untrackOctant(oct);	
		}
		string octantName = CommonUtility.getStringFromList(octList);
		destroyOctant(minimapPrefix + octantName);
		return 1;
	}
	
	public int trackOctant(List<int> octList)
	{
		// redundant judgment here...
		if (trackedOctant[CommonUtility.getStringFromList(octList)] == null)
		{
			Octant oct = instance.getOctantByIndex(octList);
			if (oct == null)
			{
				oct = instance.addOctant(octList);	
			}
			instance.trackOctant (oct);
			
			int xmin = 0;
			int ymin = 0;
			int zmin = 0;
			
			// instantiate the octant being tracked.
			// GetBoundaries uses ref ints
			string octantName = CommonUtility.getStringFromList (octList);
			CommonUtility.GetBoundaries(octantName, ref xmin, ref ymin, ref zmin);
			
			instantiateOctant(minimapPrefix + octantName, xmin + 256, ymin + 256, zmin + 256);
			trackedOctant.Add(CommonUtility.getStringFromList (octList), new UnityEngine.Vector3(xmin + 256, ymin + 256, zmin + 256));
		}
		else
		{
			
		}
		return 1;
	}
	
	public void Update ()
	{
		selfTransform = GameObject.Find (selfTransformPath).transform;
		selfLocation.x_ = selfTransform.localPosition.x;
		selfLocation.y_ = selfTransform.localPosition.y;
		selfLocation.z_ = selfTransform.localPosition.z;
		
		// setLocation talks with DiscoveryModule without calling callback.
		instance.getSelfGameEntity ().setLocation (selfLocation, false);
		List<string> toDelete = new List<string> ();
		
		// Is generating a copy of hashtable a potentially better idea than this?
		hashtableLock.WaitOne (Constants.MutexLockTimeoutMilliSeconds);
		
		foreach (DictionaryEntry pair in hashtable) {
			GameObject entity = GameObject.Find ((string)pair.Key);
			remap.NDNMOG.DiscoveryModule.Vector3 location = new remap.NDNMOG.DiscoveryModule.Vector3 ((remap.NDNMOG.DiscoveryModule.Vector3)pair.Value);
			//Debug.Log(location.x_);
			UnityEngine.Vector3 locationUnity = new UnityEngine.Vector3 (location.x_, location.y_, location.z_);
				
			if (entity == null) {
				if (locationUnity.x != Constants.DefaultLocationDropEntity && locationUnity.x != Constants.DefaultLocationNewEntity) {
					Transform newEntity = Instantiate (playerTransform, locationUnity, Quaternion.identity) as Transform;
					newEntity.name = (string)pair.Key;
				}
				//newEntity.tag = "";
			} else {
				if (locationUnity.x != Constants.DefaultLocationDropEntity && locationUnity.x != Constants.DefaultLocationNewEntity) {
					entity.transform.localPosition = locationUnity;
				} else {
					Destroy (entity);
					
					// hashtable deletion in foreach loop is considered illegal
					toDelete.Add ((string)pair.Key);
				}
			}
		}
		foreach (string str in toDelete) {
			hashtable.Remove (str);	
		}
		if (instantiated)
		{
			updateOctantList((int)selfLocation.x_, (int)selfLocation.y_, (int)selfLocation.z_);
		}
		hashtableLock.ReleaseMutex ();
	}
	
	public bool setPosCallback (string name, remap.NDNMOG.DiscoveryModule.Vector3 location)
	{
		if (location.Equals (new remap.NDNMOG.DiscoveryModule.Vector3 (Constants.DefaultLocationNewEntity, Constants.DefaultLocationNewEntity, Constants.DefaultLocationNewEntity))) {
			Debug.Log ("New entity " + name + " discovered from returned names");
			writeLog("New entity " + name + " discovered from returned names");
			
			hashtableLock.WaitOne (Constants.MutexLockTimeoutMilliSeconds);
			if (hashtable.Contains (name)) {
				hashtable [name] = location;
			} else {
				hashtable.Add (name, location);
			}
			hashtableLock.ReleaseMutex ();
		} else if (location.Equals (new remap.NDNMOG.DiscoveryModule.Vector3 (Constants.DefaultLocationDropEntity, Constants.DefaultLocationDropEntity, Constants.DefaultLocationDropEntity))) {
			Debug.Log ("Entity " + name + " dropped.");
			writeLog("Entity " + name + " dropped.");
			
			hashtableLock.WaitOne (Constants.MutexLockTimeoutMilliSeconds);
			if (hashtable.Contains (name)) {
				hashtable [name] = location;
			} else {
				hashtable.Add (name, location);
			}
			hashtableLock.ReleaseMutex ();
		} else {
			hashtableLock.WaitOne (Constants.MutexLockTimeoutMilliSeconds);
			if (hashtable.Contains (name)) {
				hashtable [name] = location;
			} else {
				hashtable.Add (name, location);
			}
			hashtableLock.ReleaseMutex ();
		}
		return true;
	}
	
	public void OnApplicationQuit ()
	{
		// if somehow this method is not called, the discovery thread will not be stopped
		instance.stopDiscovery ();
	}
	
	public void Awake ()
	{
		Application.targetFrameRate = 30;
	}
}