using System;

public class UnityConstants
{
	public const string configFilePath = "config.txt";
	public const string gameLogName = "game-";
	public const string libraryLogName = "library-";
	public const bool logFileEnabled = true;
	
	public const string LoggingLevelNone = "none";
	
	// The constants for resources path
	public const string playerTransformPath = "/player/graphics";
	public const string cubeTransformPath = "/octant";
	public const string selfTransformPath = "/player";
	public const string minimapCameraPath = "/minimapCamera";
	
	public const string dollPath = "/doll";
	public const string haloPath = "/halo";
	public const string labelTransformPath = "/label";
	
	public const string playerParentPath = "/players";
	public const string asteroidParentPath = "/asteroids";
	
	// The game object's in game name prefix for displaying in minimap
	public const string minimapPrefix = "minimap";
	// When the virtual world distance between the center of an octant and current location is larger than this,
	// the octant is considered to be no longer cared about.
	public const int distanceThreshold = 600;
	// For each scenario, populate the world to ten trees in different octants; 
	public const int treeNum = 10;
	// Currently, the game has two scenarios; (Each scenario includes a preset number of asteroids, for testing octree partitioning functions.)
	public const int scenarioNum = 2;
	
	// startingLoc one at 0/0/0/5/6/2/7
	public static UnityEngine.Vector3 startingLocOne = new UnityEngine.Vector3(6750, 3950, 4800);
	
	// startingLoc two at 0/4/0/1/2/2/2
	public static UnityEngine.Vector3 startingLocTwo = new UnityEngine.Vector3(16478, 3950, 4288);
	// startingLoc three at 0/0/4/5/6/6/6
	public static UnityEngine.Vector3 startingLocThree = new UnityEngine.Vector3(15976, 3950, 4286);
	
	// an array of vectors to populate the world with octants
	public static UnityEngine.Vector3[] asteroidOffset = new UnityEngine.Vector3[treeNum];
	
	public static void init()
	{
		asteroidOffset[0] = new UnityEngine.Vector3(0, -50, 0);
		asteroidOffset[4] = new UnityEngine.Vector3(0, -50, 500);
		asteroidOffset[5] = new UnityEngine.Vector3(0, -50, -500);
		
		asteroidOffset[1] = new UnityEngine.Vector3(500, -50, 0);
		asteroidOffset[2] = new UnityEngine.Vector3(500, -50, 500);
		asteroidOffset[3] = new UnityEngine.Vector3(500, -50, -500);
		
		asteroidOffset[6] = new UnityEngine.Vector3(-500, -50, 0);
		asteroidOffset[7] = new UnityEngine.Vector3(-500, -50, 500);
		asteroidOffset[8] = new UnityEngine.Vector3(-500, -50, -500);
		
		asteroidOffset[9] = new UnityEngine.Vector3(0, 450, 0);
	}
}