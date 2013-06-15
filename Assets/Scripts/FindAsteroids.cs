using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FindAsteroids : MonoBehaviour {
	
	// boundary: 512*512*512
	struct Boundary{
		public float xmin;
		public float xmax;
		public float ymin;
		public float ymax;
		public float zmin;
		public float zmax;
		public Boundary(float a, float b, float c, 
			float d, float e, float f)
		{
			xmin = a;
			xmax = b;
			ymin = c;
			ymax = d;
			zmin = e;
			zmax = f;
		}
	};
	Boundary bry;
	
	// octant names
	List<string> aura; // <octant labels>
	List<string> nimbus;
	
	// object names
	Dictionary<string,List<string>> asteroidDic = new Dictionary<string, List<string>>(); // <label,<id>>
	
	void Start () {
		
		//GetComponent<Initialize>().LandOnRandomAsteroid();
		
		aura = new List<string>();
		nimbus = new List<string>();
		
		string temp = GetLabel(transform.position);
		if(temp==null)
		{
			print("FindAsteroids.Start(): Aura is null!");
			return;
		}
		
		aura.Add ( temp );
		nimbus.AddRange( aura ); // nimbus contains aura
		nimbus.AddRange ( GetNeighbors(transform.position) );
		AddAsteroidBySpace(nimbus);
		
		bry = GetBoundaries(aura[0]);
		
		InvokeRepeating("CheckPos", 0, 0.3F);
	}
	
	void CheckPos() {
		
		string temp = null;
		
		if( InBound(transform.position) == false )
		{
			aura.Clear();
			temp = GetLabel(transform.position);
			if(temp == null)
			{
				print("FindAsteroids.CheckPos(): Aura is null!");
				return;
			}
			
			aura.Add ( temp );
			
			List<string> newnimbus = new List<string>();
			newnimbus.AddRange( aura );
			newnimbus.AddRange ( GetNeighbors(transform.position) );
			
			List<string> newoct = new List<string>(); // octants to be added to nimubs
			List<string> oldoct = new List<string>(); // octants to be deleted from nimbus
			
			CompareNimbus(nimbus, newnimbus, newoct, oldoct);
			
//			print(string.Join(",  ", nimbus.ToArray()));
//			print(string.Join(",  ", newnimbus.ToArray()));
			print("new octant to be added: " + string.Join(",  ", newoct.ToArray()));
			print("old octant to be deleted: " + string.Join(",  ", oldoct.ToArray()));
			string sum = "";
			foreach(string k in asteroidDic.Keys)
			{
				sum = sum + k + ",  ";
			}
			print("asteroidDictionary before +/-: " + sum);
				
			AddAsteroidBySpace(newoct);
			DeleteAsteroidBySpace(oldoct);
			
			sum = "";
			foreach(string k in asteroidDic.Keys)
			{
				sum = sum + k + ",  ";
			}
			print("asteroidDictionary after +/-: " + sum);
			
			newoct.Clear();
			oldoct.Clear();
			
			nimbus.Clear();
			nimbus.AddRange(newnimbus);
			bry = GetBoundaries(aura[0]);
			
		}
		
    }
	
	
	void AddAsteroidBySpace(List<string> nimbus)
	{
		List<string> asteroidnames = null;
		foreach(string n in nimbus)
		{
			if(asteroidDic.ContainsKey(n)==true)
			{
				// print("AddAsteroidBySpace(): this octant is not new! --" + n);
				continue;
			}
			
			asteroidnames = Request("/" + n + "/asteroid");
			if(asteroidnames != null)
			{ 
				foreach(string a in asteroidnames)
				{
					string id = DoAsteroid(a);
					
					if(asteroidDic.ContainsKey(n)==false)
					{
						asteroidDic.Add (n,new List<string>());
					}
					asteroidDic[n].Add(id);
						
				}
			}
			
		}
	}
	
	void DeleteAsteroidBySpace(List<string> octs)
	{
		List<string> asteroidids;
		foreach(string o in octs)
		{
			if(asteroidDic.ContainsKey(o) == false)
			{
				// print("DeleteAsteroidBySpace(): this octant is not old! --" + o);
				continue;
			}
			
			asteroidids = asteroidDic[o];
			foreach(string id in asteroidids)
			{
				GameObject t = GameObject.Find("asteroid"+id);
				if(!t)
				{
					print("Can't destroy asteroid with given id.");
				}
				Destroy( t );
			}
			asteroidDic.Remove(o);
		}
	}
	
	void CompareNimbus(List<string> oldnimbus, List<string> newnimbus, 
		List<string> newoct, List<string> oldoct)
	{
		foreach(string o in oldnimbus)
		{
			if(newnimbus.Contains(o)==false)
			{
				oldoct.Add(o);
			}
			
		}
		
		foreach(string n in newnimbus)
		{
			if(oldnimbus.Contains(n)==false)
			{
				newoct.Add(n);
			}
		}
			
	}
	
	bool InBound(Vector3 position)
	{
		if(position.x<bry.xmin || position.y<bry.ymin || position.z<bry.zmin)
		{
			return false;
		}
		if(position.x>bry.xmax || position.y>bry.ymax || position.z>bry.zmax)
		{
			return false;
		}
		return true;
	}
	
	bool InWorld(Vector3 position)
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
	
	string GetLabel(Vector3 position)
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
	
	List<string> GetNeighbors(Vector3 position)
	{
		List<string> neighborlist = new List<string>();
		int[,] neighbors = {{1,0,0}, {-1,0,0}, // x
							{0,1,0}, {0,-1,0}, // y
							{0,0,1}, {0,0,-1}, // z
							{1,1,0}, {1,-1,0}, {-1,1,0}, {-1,-1,0}, // x,y
							{1,0,1}, {1,0,-1}, {-1,0,1}, {-1,0,-1}, // x,z
							{0,1,1}, {0,1,-1}, {0,-1,1}, {0,-1,-1}, // y,z
							{1,1,1}, {-1,1,1}, {1,-1,1}, {1,1,-1}, {1,-1,-1}, {-1,1,-1}, {-1,-1,1},{-1,-1,-1} // x,y,z
		};
		
		Vector3 offset = new Vector3();
		string temp = null;
		for(int i = 0; i<26; i++)
		{
			offset.x = neighbors[i,0] * 512;
			offset.y = neighbors[i,1] * 512;
			offset.z = neighbors[i,2] * 512;
			
			temp = GetLabel(position+offset);
			if(temp!=null)
			{
				neighborlist.Add(temp);
			}
		}
		
		
		return neighborlist;
	}
	
	Boundary GetBoundaries(string labels)
	{
		// print(labels);
		string [] split = labels.Split(new char [] {'/'},StringSplitOptions.RemoveEmptyEntries);
		
		int L1oct = Convert.ToInt32(split[0],8);
		int L2oct = Convert.ToInt32(split[1],8);
		int L3oct = Convert.ToInt32(split[2],8);
		int L4oct = Convert.ToInt32(split[3],8);
		
		string L1bits = Convert.ToString (L1oct,2).PadLeft(3,'0');
		string L2bits = Convert.ToString (L2oct,2).PadLeft(3,'0');
		string L3bits = Convert.ToString (L3oct,2).PadLeft(3,'0');
		string L4bits = Convert.ToString (L4oct,2).PadLeft(3,'0');
		
		string xbits = "" + L1bits[0] + L2bits[0] + L3bits[0] + L4bits[0];
		string ybits = "" + L1bits[1] + L2bits[1] + L3bits[1] + L4bits[1];
		string zbits = "" + L1bits[2] + L2bits[2] + L3bits[2] + L4bits[2];
		
		int x = Convert.ToInt32 (xbits,2);
		int y = Convert.ToInt32 (ybits,2);
		int z = Convert.ToInt32 (zbits,2);
		
		int xmin = x * 512; 
		int ymin = y * 512;
		int zmin = z * 512;
		
		int xmax = xmin + 512;
		int ymax = ymin + 512;
		int zmax = zmin + 512;
		
		//print(labels + ":" + L1bits + "," + L2bits + "," + L3bits + "," + L4bits);
		//print(xbits + "," + ybits + "," + zbits);
//		
//		print(transform.position.x>xmin);
//		print(transform.position.x<xmax);
//		
//		print(transform.position.y>ymin);
//		print(transform.position.y<ymax);
//		
//		print(transform.position.z>zmin);
//		print(transform.position.z<zmax);
		
		Boundary bry = new Boundary(xmin, xmax, ymin, ymax, zmin, zmax);
		return bry;
		
	}
	
	public List<string> Request(string name)
	{
		
		Dictionary<string, List<string>> source = Data.data;
	
		if(source.ContainsKey(name))
		{
			// print("There's something in this octant.");
			return source[name];
		}
		else
		{
			// print("Nothing here!");
			return null;
		}
	}
	
	string DoAsteroid(string info)
	{
		//print(info);
		
		string [] split = info.Split((char[])null,StringSplitOptions.RemoveEmptyEntries);
//		foreach(string s in split)
//			print(s);
		
		string id = split[0];
		float x = float.Parse(split[1]);
		float y = float.Parse(split[2]);
		float z = float.Parse(split[3]);
		string type = split[4];
		
		Vector3 pos = new Vector3(x,y,z);
		GameObject asteroid1 = GameObject.Find("tree2");
		
		
		GameObject newAsteroid = UnityEngine.Object.Instantiate(asteroid1, pos, Quaternion.identity) as GameObject;
		newAsteroid.name = "asteroid"+id;
		newAsteroid.transform.localScale = new Vector3(2000f,2000f,2000f);
		
		return id;
	}
}
