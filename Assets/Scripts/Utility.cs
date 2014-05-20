using UnityEngine;
using System;
using System.Collections.Generic;

public class Utility : MonoBehaviour {
	
	/*
	public static string GetLabelFromName(string name)
	{
		if(name.Contains("/octant/"))
		{
			int index = name.IndexOf("/octant/");
			
			if(name.Length<(index+21))
			{
				//print("Ill name: " + name);
				return null;
			}
			
			return name.Substring(index+8,13);
		}
		return null;
	}
	
	public static string GetIDFromName(string name)
	{
		if(name.Contains("/asteroid/octant/"))
		{
			int index = name.IndexOf("/octant/");
			if(name.Length<(index+22))
				return null;
			
			string tail = name.Substring(index + 22);
			string[] split = tail.Split(new char [] {'/'},StringSplitOptions.RemoveEmptyEntries);
			string id = split[0]; 
			
			return id;
		}
		
		if(name.Contains("/fish/octant/"))
		{
			int index = name.IndexOf("/octant/");
			if(name.Length<(index+51))
				return null;
			
			string tail = name.Substring(index + 51);
			string[] split = tail.Split(new char [] {'/'},StringSplitOptions.RemoveEmptyEntries);
			string id = split[0]; 
			
			return id;
		}
			
		return null;
	}
	*/
	/*
	public static string GetNameTillID(string name)
	{
		if(name.Contains("/asteroid/octant/"))
		{
			int index = name.IndexOf("/octant/");
			if(name.Length<(index+22))
				return null;
			
			string namebeforeid = name.Substring(0, index + 21);
			return namebeforeid;
		}
		
		if(name.Contains("/fish/octant/"))
		{
			int index = name.IndexOf("/octant/");
			if(name.Length<(index+51))
				return null;
			
			string namebeforeid = name.Substring(0, index + 50);
			return namebeforeid;
		}
		
		return null;
	}
	
	public static string GetTimeComponent(int addhour = -3, int addmin = -15)
	{
		DateTime ct = DateTime.Now.AddMinutes(addmin);
		ct = ct.AddHours(addhour);
		//DateTime ct = DateTime.Now.AddMinutes(2);
		string component = ct.ToString("ddd-MMM-dd-HH.mm") + ".00-PDT-" + ct.ToString("yyyy");
		return component;
	}
	
	public class NameContBuf
	{
		private Queue buf = new Queue ();
		private static ReaderWriterLockSlim rwl = new ReaderWriterLockSlim();
		
		public string Read()
		{
			string item;
			
			rwl.EnterWriteLock();
			item = (string)buf.Dequeue();
			rwl.ExitWriteLock();
         		
      		return item;
		}
		
		public void Write(string name, string content)
		{
			rwl.EnterWriteLock();
			buf.Enqueue ("" + name + "|" + content);
      		rwl.ExitWriteLock();
		}
		
		public bool IsEmpty()
		{
			if(buf.Count == 0)
				return true;
			else
				return false;
		}
	}
	*/
}
