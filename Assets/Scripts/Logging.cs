using System;

public enum LoggingLevel
{
	None = 0,
	All = 1
};

public class Logging
{
	public LoggingLevel level_;
	public string playerName_;
	
	public string gameLogFileName_;
	public string libraryLogFileName_;
	
	// create default logging with logging level none.
	public Logging(string playerName, string gameLogFileName, string libraryLogFileName)
	{
		level_ = LoggingLevel.None;
		playerName_ = playerName;
		
		gameLogFileName_ = gameLogFileName;
		libraryLogFileName_ = libraryLogFileName;
	}
	
	public bool writeLog(string str)
	{
		if (UnityConstants.logFileEnabled && level_ != LoggingLevel.None)
		{
			// This method is not thread-safe, which could be one of the causes of Unity crashing my interest expression threads.
			// lock(this) is generally considered bad practice: for what reasons?
			lock(this)
			{
				System.IO.File.AppendAllText (gameLogFileName_ + playerName_ + ".txt", str);	
			}
		}
		return UnityConstants.logFileEnabled;
	}
	
	public bool libraryWriteCallback(string type, string info)
	{
		if (level_ != LoggingLevel.None && UnityConstants.logFileEnabled)
		{
			// lock(this) is generally considered bad practice: for what reasons?
			lock(this)
			{
				System.IO.File.AppendAllText (libraryLogFileName_ + playerName_ + ".txt", type + " - " + info + "\n");	
			}
		}
		return UnityConstants.logFileEnabled;
	}
	
};
