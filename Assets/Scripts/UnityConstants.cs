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
	
}