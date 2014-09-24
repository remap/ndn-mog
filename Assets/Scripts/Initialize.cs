using UnityEngine;

using System.Collections;
using System;
using System.Collections.Generic;

using remap.NDNMOG.DiscoveryModule;
using System.Threading;
using net.named_data.jndn.encoding;

using System.IO;

// EntityInfo class correponds with the GameEntityInfo class in plugin
public class EntityInfo
{
	public string name_;
	public string renderString_;
	
	public EntityInfo (string name, string renderString)
	{
		name_ = name;
		renderString_ = renderString;
	}
};

public class Initialize : MonoBehaviour
{
	public Instance instance;
	public string playerName;
	public string gameInstanceName;
	
	public static Transform playerTransform;
	public static Transform cubeTransform;
	
	// gameEntityHashtable is keyed by name of a game entity; with the vector3 location of the game entity as value.
	public Hashtable gameEntityHashtable = new Hashtable ();
	// renderList is the queue of entities that should be re-colored (re-rendered) in Unity
	public List<EntityInfo> renderList = new List<EntityInfo> ();
	
	// hashtableLock locks gameEntityHashtable
	public Mutex hashtableLock = new Mutex ();
	// renderListLock locks the queue of entities to be rendered
	public Mutex renderListLock = new Mutex ();
	
	// trackedOctant hashtable is keyed by string name of an octant; with the vector3 location of the octant as value.
	// trackedOctant should be sync'ed up with the interestExpressionOctants in discovery module.
	public Hashtable trackedOctant = new Hashtable ();
	
	// The static array of asteroids, used as a substitute for retrieving airport data from lioncub
	public Transform[] asteroids;
	
	public remap.NDNMOG.DiscoveryModule.Vector3 selfLocation = new remap.NDNMOG.DiscoveryModule.Vector3 (0, 0, 0);
	public Transform selfTransform;
	public Transform playersParentTransform;
	public Transform minimapCamera;
	
	public bool instantiated = false;
	public string loggingLevel = UnityConstants.LoggingLevelNone;
	
	public Logging log;
	// the render string for local instance
	public string renderString = Constants.DefaultRenderString;
	public string hubPrefix = "";
	
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
						} else {
							return false;	
						}
					}
					if (s.Contains ("instance")) {
						string[] name = s.Split (':');
						if (name.Length > 0) {
							gameInstanceName = name [1];
						}
					}
					if (s.Contains ("logging-level")) {
						string[] name = s.Split (':');
						if (name.Length > 0) {
							loggingLevel = name [1];
						}
					}
					if (s.Contains ("render-string")) {
						string[] name = s.Split (':');	
						if (name.Length > 0) {
							renderString = name [1];	
						}
					}
					if (s.Contains("hub-prefix")) {
						string[] name = s.Split(':');
						if (name.Length > 0) {
							hubPrefix = name [1];
						}
					}
					if (s.Contains("update-interval")) {
						string[] name = s.Split(':');
						int milliseconds = 0;
						if (name.Length > 0 && int.TryParse(name[1], out milliseconds)) {
							Constants.setPositionIntervalMilliseconds(milliseconds);
						}
					}
				}
			}
			return true;
		}
	}
	
	public bool nullFunc (string s1, string s2)
	{
		return false;
	}
	
	public void instantiateOctant (string name, int x, int y, int z)
	{
		GameObject entity = GameObject.Find (name);
		
		if (entity == null) {
			UnityEngine.Vector3 locationUnity = new UnityEngine.Vector3 (x, y, z);
			Transform cube = Instantiate (cubeTransform, locationUnity, Quaternion.identity) as Transform;
			
			cube.Find(UnityConstants.labelTransformPath.TrimStart('/')).guiText.text = name;
			cube.name = name;
			cube.renderer.material.SetColor ("_Color", new Color (0.5f, 0.5f, 0.5f, 0.5f));
			cube.renderer.material.shader = Shader.Find ("Transparent/Diffuse");
			
			// Whenever there's an octant being instantiated, enable the asteroid that's in there
			List<int> octList = CommonUtility.getOctantIndicesFromVector3 (new remap.NDNMOG.DiscoveryModule.Vector3 (x, y, z));
			for (int i = 0; i < UnityConstants.treeNum * UnityConstants.scenarioNum; i++) {
				if (inOct ((int)asteroids [i].position.x, (int)asteroids [i].position.y, (int)asteroids [i].position.z, octList)) {
					toggleVisibility (asteroids [i], true);
				}
			}
		} else {
			//Debug.Log("octant " + name + " already instantiated.");	
		}
	}
	
	public void destroyOctant (string name, int x = 0, int y = 0, int z = 0)
	{
		// deinstantiateOctant does not automatically add minimap prefix
		GameObject entity = GameObject.Find (name);
		if (entity == null) {
			//Debug.Log("octant " + name + " does not exist.");
		} else {
			Destroy (entity);
			
			// Whenever there's an octant being destroyed, disable the asteroid that's in there
			List<int> octList = CommonUtility.getOctantIndicesFromVector3 (new remap.NDNMOG.DiscoveryModule.Vector3 (x + 256, y + 256, z + 256));
			for (int i = 0; i < UnityConstants.treeNum * UnityConstants.scenarioNum; i++) {
				if (inOct ((int)asteroids [i].position.x, (int)asteroids [i].position.y, (int)asteroids [i].position.z, octList)) {
					toggleVisibility (asteroids [i], false);
				}
			}
		}
	}
	
	public bool inOct (int x, int y, int z, List<int> octList)
	{
		int xmin = 0;
		int ymin = 0;
		int zmin = 0;
		
		CommonUtility.GetBoundaries (CommonUtility.getStringFromList (octList), ref xmin, ref ymin, ref zmin);
		if (x > xmin && x < (xmin + 512) && y > ymin && y < (ymin + 512) && z > zmin && z < (zmin + 512)) {
			return true;
		} else {
			return false;	
		}
	}
	
	// recursively toggle the visibility of a transform and its children, including Halos as they are used for minimap camera display.
	void toggleVisibility (Transform obj, bool state)
	{
		for (int i = 0; i < obj.GetChildCount(); i++) {
			if (obj.GetChild (i).renderer != null)
				obj.GetChild (i).renderer.enabled = state;
			
			if (obj.GetChild (i).GetComponent ("Halo") as Behaviour != null)
				(obj.GetChild (i).GetComponent ("Halo") as Behaviour).enabled = state;
	 
			if (obj.GetChild (i).GetChildCount () > 0) {
				toggleVisibility (obj.GetChild (i), state);
			}
		}
	}
	
	// Populates the world with asteroids of predefined locations; and decide if they should be enabled according to startingOct
	public void populateAsteroids(List<int> startingOct)
	{
		UnityEngine.Vector3 [] asteroidLocOne = new UnityEngine.Vector3[UnityConstants.treeNum];
		UnityEngine.Vector3 [] asteroidLocTwo = new UnityEngine.Vector3[UnityConstants.treeNum];
		asteroids = new Transform[UnityConstants.treeNum * UnityConstants.scenarioNum];
		
		// for the demo, the trick is that asteroids are statically located in fixed positions.
		for (int i = 0; i < UnityConstants.treeNum; i++) {
			asteroidLocOne [i] = UnityConstants.startingLocOne + UnityConstants.asteroidOffset[i];
			asteroidLocTwo [i] = UnityConstants.startingLocTwo + UnityConstants.asteroidOffset[i];
		}
		
		for (int i = 0; i < UnityConstants.treeNum; i++) {
			asteroids [i] = AsteroidInstantiate.MakeAnAsteroid ("asteroid" + i.ToString (), asteroidLocOne [i], true);
			if (!inOct ((int)asteroids [i].position.x, (int)asteroids [i].position.y, (int)asteroids [i].position.z, startingOct)) {
				toggleVisibility (asteroids [i], false);
			}
		}
		for (int i = UnityConstants.treeNum; i < UnityConstants.treeNum * UnityConstants.scenarioNum; i++) {
			asteroids [i] = AsteroidInstantiate.MakeAnAsteroid ("asteroid" + i.ToString (), asteroidLocTwo [i - UnityConstants.treeNum], true);
			if (!inOct ((int)asteroids [i].position.x, (int)asteroids [i].position.y, (int)asteroids [i].position.z, startingOct)) {
				toggleVisibility (asteroids [i], false);
			}
		}
		
	}
	
	public void Start ()
	{
		UnityConstants.init();
		
		selfTransform = GameObject.Find (UnityConstants.selfTransformPath).transform;
		playersParentTransform = GameObject.Find(UnityConstants.playerParentPath).transform;
		
		minimapCamera = GameObject.Find(UnityConstants.selfTransformPath + UnityConstants.minimapCameraPath).transform;
		
		renderString = Constants.DefaultRenderString;
		
		AsteroidInstantiate.init ();
		playerTransform = GameObject.Find (UnityConstants.playerTransformPath).transform;
		cubeTransform = GameObject.Find (UnityConstants.cubeTransformPath).transform;
		
		UnityEngine.Vector3 dollPosUnity = UnityConstants.startingLocOne;
		transform.position = dollPosUnity;
		
		remap.NDNMOG.DiscoveryModule.Vector3 dollPos = new remap.NDNMOG.DiscoveryModule.Vector3 (dollPosUnity.x, dollPosUnity.y, dollPosUnity.z);
		List<int> startingOct = CommonUtility.getOctantIndicesFromVector3 (dollPos);
		populateAsteroids(startingOct);
		
		log = new Logging (playerName, UnityConstants.gameLogName, UnityConstants.libraryLogName);
		if (!readConfFromFile (UnityConstants.configFilePath)) {
			playerName = "default";
			log.writeLog ("Didn't parse config file, using default as name.");
		}
		
		if (loggingLevel != UnityConstants.LoggingLevelNone) {
			log.level_ = LoggingLevel.All;
		} else {
			log.level_ = LoggingLevel.None;
		}
		
		instance = new Instance (startingOct, playerName, dollPos, setPosCallback, log.libraryWriteCallback, infoCallback, null, null, null, renderString, hubPrefix); 
		instance.discovery ();
		
		instantiated = true;
		// instance is interested in its starting location
		trackOctant (startingOct);
		
		// set playerName for label: this is the full name of the local player.
		GameObject.Find (UnityConstants.playerTransformPath + UnityConstants.labelTransformPath).guiText.text = instance.getSelfGameEntity().getHubPrefix() + "/" + Constants.PlayersPrefix + "/" + playerName;
		
		GameObject entity = GameObject.Find (UnityConstants.playerTransformPath + UnityConstants.dollPath);
		if (entity != null) {
			string path = "Materials/" + renderString;
			Material unityMaterial = Resources.Load (path, typeof(Material)) as Material;
				
			if (unityMaterial != null) {
				entity.GetComponent<MeshRenderer> ().material = unityMaterial;
			}
		}
	}
	
	// updateOctantList decides which octants should be tracked, for now it's called per frame, which is not necessary
	public int updateOctantList (int x, int y, int z)
	{
		int xmin = 0;
		int ymin = 0;
		int zmin = 0;
		
		List<int> octList = CommonUtility.getOctantIndicesFromVector3 (new remap.NDNMOG.DiscoveryModule.Vector3 (x, y, z));
		string octantName = CommonUtility.getStringFromList (octList);
		CommonUtility.GetBoundaries (octantName, ref xmin, ref ymin, ref zmin);
		
		List<string> toDelete = new List<string> ();
		// naive strategy for untracking octants: for each octants that's not in range, we remove the octants from tracking list
		foreach (DictionaryEntry pair in trackedOctant) {
			UnityEngine.Vector3 position = (UnityEngine.Vector3)pair.Value;
			if (UnityEngine.Vector3.Distance (position, new UnityEngine.Vector3 (x, y, z)) > UnityConstants.distanceThreshold) {
				List<int> newOctList = CommonUtility.getOctantIndicesFromVector3 (new remap.NDNMOG.DiscoveryModule.Vector3 (position.x, position.y, position.z));
				untrackOctant (newOctList);
				toDelete.Add ((string)pair.Key);
			}
		}
		// Separate deletion from traversal so that snapshot does not fall out of sync
		foreach (string str in toDelete) {
			trackedOctant.Remove (str);	
		}
		
		// Z direction on one direction does not work correctly, checking out reasons...
		if ((x - xmin > 462) && (x + 512 < 65536)) {
			List<int> newOctList = CommonUtility.getOctantIndicesFromVector3 (new remap.NDNMOG.DiscoveryModule.Vector3 (x + 512, y, z));
			trackOctant (newOctList);
		}
		if ((x - xmin < 50) && (x - 512 > 0)) {
			List<int> newOctList = CommonUtility.getOctantIndicesFromVector3 (new remap.NDNMOG.DiscoveryModule.Vector3 (x - 512, y, z));
			trackOctant (newOctList);
		}
		if ((y - ymin > 462) && (y + 512 < 65536)) {
			List<int> newOctList = CommonUtility.getOctantIndicesFromVector3 (new remap.NDNMOG.DiscoveryModule.Vector3 (x, y + 512, z));
			trackOctant (newOctList);
		}
		if ((y - ymin < 50) && (y - 512 > 0)) {
			List<int> newOctList = CommonUtility.getOctantIndicesFromVector3 (new remap.NDNMOG.DiscoveryModule.Vector3 (x, y - 512, z));
			trackOctant (newOctList);
		}
		if ((z - zmin > 462) && (z + 512 < 65536)) {
			List<int> newOctList = CommonUtility.getOctantIndicesFromVector3 (new remap.NDNMOG.DiscoveryModule.Vector3 (x, y, z + 512));
			trackOctant (newOctList);
		}
		if ((z - zmin < 50) && (z - 512 > 0)) {
			List<int> newOctList = CommonUtility.getOctantIndicesFromVector3 (new remap.NDNMOG.DiscoveryModule.Vector3 (x, y, z - 512));
			trackOctant (newOctList);
		}
		
		return 1;
	}
	
	public int untrackOctant (List<int> octList)
	{
		Octant oct = instance.getOctantByIndex (octList);
		// what is the relationship between tracked octant and kept octant in DiscoverModule
		if (oct != null) {
			instance.untrackOctant (oct);	
		}
		string octantName = CommonUtility.getStringFromList (octList);
		
		// Added for the purpose of hinding everything in that octant
		int xmin = 0;
		int ymin = 0;
		int zmin = 0;
		
		CommonUtility.GetBoundaries (octantName, ref xmin, ref ymin, ref zmin);
		
		destroyOctant (UnityConstants.minimapPrefix + octantName, xmin, ymin, zmin);
		return 1;
	}
	
	public int trackOctant (List<int> octList)
	{
		// redundant judgment here...
		if (trackedOctant [CommonUtility.getStringFromList (octList)] == null) {
			Octant oct = instance.getOctantByIndex (octList);
			if (oct == null) {
				oct = instance.addOctant (octList);	
			}
			instance.trackOctant (oct);
			
			int xmin = 0;
			int ymin = 0;
			int zmin = 0;
			
			// instantiate the octant being tracked.
			// GetBoundaries uses ref ints
			string octantName = CommonUtility.getStringFromList (octList);
			CommonUtility.GetBoundaries (octantName, ref xmin, ref ymin, ref zmin);
			
			instantiateOctant (UnityConstants.minimapPrefix + octantName, xmin + 256, ymin + 256, zmin + 256);
			trackedOctant.Add (CommonUtility.getStringFromList (octList), new UnityEngine.Vector3 (xmin + 256, ymin + 256, zmin + 256));
		} else {
			
		}
		return 1;
	}
	
	public void Update ()
	{
		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
			if (Input.GetKeyDown(KeyCode.Alpha1)) {
				Movement.autoflyspeed = 0F;
				selfTransform.localPosition = UnityConstants.startingLocOne;
				trackOctant (CommonUtility.getOctantIndicesFromVector3
					(new remap.NDNMOG.DiscoveryModule.Vector3(UnityConstants.startingLocOne.x, UnityConstants.startingLocOne.y, UnityConstants.startingLocOne.z)));
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2)) {
				Movement.autoflyspeed = 0F;
				selfTransform.localPosition = UnityConstants.startingLocTwo;
				trackOctant (CommonUtility.getOctantIndicesFromVector3
					(new remap.NDNMOG.DiscoveryModule.Vector3(UnityConstants.startingLocTwo.x, UnityConstants.startingLocTwo.y, UnityConstants.startingLocTwo.z)));
			}
			else if (Input.GetKeyDown(KeyCode.Alpha3)) {
				Movement.autoflyspeed = 0F;
				selfTransform.localPosition = UnityConstants.startingLocThree;
				trackOctant (CommonUtility.getOctantIndicesFromVector3
					(new remap.NDNMOG.DiscoveryModule.Vector3(UnityConstants.startingLocThree.x, UnityConstants.startingLocThree.y, UnityConstants.startingLocThree.z)));
			}
		}
		
		selfLocation.x_ = selfTransform.localPosition.x;
		selfLocation.y_ = selfTransform.localPosition.y;
		selfLocation.z_ = selfTransform.localPosition.z;
		
		// setLocation talks with DiscoveryModule without calling callback.
		instance.getSelfGameEntity ().setLocation (selfLocation, false);
		List<string> toDelete = new List<string> ();
		// For iteration
		Transform entity;
		// Is generating a copy of hashtable a potentially better idea than this?
		hashtableLock.WaitOne ();
		foreach (DictionaryEntry pair in gameEntityHashtable) {
			entity = playersParentTransform.Find(((string)pair.Key).Replace("/", "-"));
			
			remap.NDNMOG.DiscoveryModule.Vector3 location = new remap.NDNMOG.DiscoveryModule.Vector3 ((remap.NDNMOG.DiscoveryModule.Vector3)pair.Value);
			//Debug.Log(location.x_);
			UnityEngine.Vector3 locationUnity = new UnityEngine.Vector3 (location.x_, location.y_, location.z_);
					
			if (entity == null) {
				if (locationUnity.x != Constants.DefaultLocationDropEntity && locationUnity.x != Constants.DefaultLocationNewEntity) {
					Transform newEntity = Instantiate (playerTransform, locationUnity, Quaternion.identity) as Transform;
					newEntity.name = ((string)pair.Key).Replace("/", "-");
					newEntity.parent = GameObject.Find(UnityConstants.playerParentPath).transform;
					newEntity.Find(UnityConstants.labelTransformPath.TrimStart('/')).guiText.text = (string)pair.Key;
				}
			} else {
				if (locationUnity.x != Constants.DefaultLocationDropEntity && locationUnity.x != Constants.DefaultLocationNewEntity) {
					entity.transform.localPosition = locationUnity;
				} else {
					Destroy (entity.gameObject);
						
					// hashtable deletion in foreach loop is considered illegal
					toDelete.Add ((string)pair.Key);
				}
			}
		}
		foreach (string str in toDelete) {
			gameEntityHashtable.Remove (str);	
		}
		if (instantiated) {
			updateOctantList ((int)selfLocation.x_, (int)selfLocation.y_, (int)selfLocation.z_);
		}
	
		hashtableLock.ReleaseMutex ();
		
		renderListLock.WaitOne ();
		if (renderList.Count != 0) {
			// Rendering based on the list...which still tells me collection is modified, even if I have the lock to protect it...finding out why.
			foreach (EntityInfo ei in renderList) {
				GameObject renderEntity = playersParentTransform.Find(ei.name_.Replace("/", "-") + UnityConstants.dollPath).gameObject;
				if (renderEntity != null) {
					string path = "Materials/" + ei.renderString_;
					Material unityMaterial = Resources.Load (path, typeof(Material)) as Material;
					
					if (unityMaterial != null) {
						renderEntity.GetComponent<MeshRenderer> ().material = unityMaterial;
					}
				}
			}
			
			renderList.Clear();
		}
		renderListLock.ReleaseMutex ();
	}
	
	public bool infoCallback (string prefix, string name, string info)
	{
		EntityInfo ei = new EntityInfo (prefix + "/" + Constants.PlayersPrefix + "/" + name, info);
		renderListLock.WaitOne ();
		renderList.Add (ei);
		renderListLock.ReleaseMutex ();
		return true;
	}
	
	// though setPosCallback is called by gameEntity, it's still trying to access the entities in Unity by name
	public bool setPosCallback (string prefix, string name, remap.NDNMOG.DiscoveryModule.Vector3 location)
	{
		string storedName = prefix + "/" + Constants.PlayersPrefix + "/" + name;
		hashtableLock.WaitOne ();
		if (location.Equals (new remap.NDNMOG.DiscoveryModule.Vector3 (Constants.DefaultLocationNewEntity, Constants.DefaultLocationNewEntity, Constants.DefaultLocationNewEntity))) {
			log.writeLog ("New entity " + storedName + " discovered from returned names");
			
			if (gameEntityHashtable.Contains (storedName)) {
				gameEntityHashtable [storedName] = location;
			} else {
				gameEntityHashtable.Add (storedName, location);
			}
		} else if (location.Equals (new remap.NDNMOG.DiscoveryModule.Vector3 (Constants.DefaultLocationDropEntity, Constants.DefaultLocationDropEntity, Constants.DefaultLocationDropEntity))) {
			log.writeLog ("Entity " + storedName + " dropped.");
			
			if (gameEntityHashtable.Contains (storedName)) {
				gameEntityHashtable [storedName] = location;
			} else {
				gameEntityHashtable.Add (storedName, location);
			}
			
		} else {
			if (gameEntityHashtable.Contains (storedName)) {
				gameEntityHashtable [storedName] = location;
			} else {
				gameEntityHashtable.Add (storedName, location);
			}
		}
		hashtableLock.ReleaseMutex ();
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