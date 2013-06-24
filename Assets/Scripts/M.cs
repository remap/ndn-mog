using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class M : MonoBehaviour {
	
	
	// commonly used functions shared by all scripts
	
	public static string GetLabel(Vector3 position)
	{
		// decimal points in x,y,z
		// will not be used in this funciton
		
		// check if the point is in the game world
		if(InWorld(position) == false)
		{
			return null;
		}
		
		// get binaries
		string xbits = Convert.ToString((int)position.x, 2).PadLeft(13,'0');
		string ybits = Convert.ToString((int)position.y, 2).PadLeft(13,'0');
		string zbits = Convert.ToString((int)position.z, 2).PadLeft(13,'0');
		
		
		// reorganize
		string L1bits = ""+xbits[0] + ybits[0] + zbits[0]; 
		string L2bits = ""+xbits[1] + ybits[1] + zbits[1];
		string L3bits = ""+xbits[2] + ybits[2] + zbits[2];
		string L4bits = ""+xbits[3] + ybits[3] + zbits[3];
		
		int temp1 = Convert.ToInt32(L1bits, 2); 
		int temp2 = Convert.ToInt32(L2bits, 2); 
		int temp3 = Convert.ToInt32(L3bits, 2); 
		int temp4 = Convert.ToInt32(L4bits, 2); 
		
		string L1 = Convert.ToString(temp1, 8);
		string L2 = Convert.ToString(temp2, 8);
		string L3 = Convert.ToString(temp3, 8);
		string L4 = Convert.ToString(temp4, 8);
		
		string labels = ""+L1 + "/" + L2 + "/" + L3 + "/" + L4;
		
//		print(xbits);
//		print(ybits);
//		print(zbits);
		
		
		return labels;
	}
	
	
	public static Vector3 GetGameCoordinates(string str_lati, string str_longi)
	{
		// convert from latitude and longitude to game coordinates
		
		float latitude = Convert.ToSingle( str_lati );
		float longitude = Convert.ToSingle( str_longi ) ;
		float pi = 3.14159265359f;
		float theta = (float)(pi*(1.0/2.0 + latitude/180));
		float fi = (float)(pi*(longitude/180));
		double x = 4000* 0.78*Mathf.Cos(theta/4)*Mathf.Sin(theta)*Mathf.Sin(fi);
		double y = 4000*1.0*Mathf.Cos(theta);
        double z = 4000* 0.78*Mathf.Cos(theta/4)*Mathf.Sin(theta)*Mathf.Cos(fi);

		// rotate the egg
		double xx = x;
		double yy = Mathf.Cos(pi/4)*y - Mathf.Sin(pi/4)*z;
		double zz = Mathf.Sin(pi/4)*y + Mathf.Cos(pi/4)*z;

		// translate the egg
		double xxx = xx + 4000;
		double yyy = yy + 4000;
		double zzz = zz + 4000;

		Vector3 pos = new Vector3((float)xxx,(float)yyy,(float)zzz);

		return pos;
	}
	
		static bool InWorld(Vector3 position)
	{
		if(position.x<0 || position.y<0 || position.z<0)
		{
			return false;
		}
		if(position.x>8192 || position.y>8192 || position.z>8192)
		{
			return false;
		}
		return true;
	}
	
	
	
	
	public static string GetLabelFromName(string name)
	{
		int index = name.IndexOf("/octant/");
		print("index: " + index + ", name: " + name);
		string n = name.Substring(index+8,7);
		return n;
	}
}
