using UnityEngine;
using System;
using remap.NDNMOG.DiscoveryModule;
	
public class AsteroidInstantiate : MonoBehaviour {
	
	public static Transform Tree2; // prefab for asteroids
	public static Transform AsteroidParent; // parent of asteroids
		
	public static void init () {
		Tree2 = GameObject.Find("/tree2").transform;
		AsteroidParent = GameObject.Find("/Asteroid").transform;
	}
	
	/*
	public static void AstDestroy()
	{
		if(AstDustbin.Count != 0)
		{
			string id = (string) AstDustbin.Dequeue();
			GameObject t = GameObject.Find("/Asteroid/"+id);
			if(!t)
			{
				print("Can't destroy asteroid with given id.");
			}
			Destroy( t );
		}
			
	}
	*/
	/*
	public static void AstInstantiate()
	{
		if(AstNameContBuf.IsEmpty() == false)
		{ 
			
			string namecontent = AstNameContBuf.Read();
			
			string [] split = namecontent.Split(new char [] {'|'},StringSplitOptions.RemoveEmptyEntries);
			
			if(split.Length<2)
				return;
			
			string name = split[0];
			string info = split[1];
			
			
			{
				if(name == Initialize.FirstAsteroidName)
				{
					return;
				}
				
				string n = M.GetLabelFromName(name);
				string id = M.GetIDFromName(name);
				
				if(n == null || id == null)
					return;
				if(OctAstDic.Contains(n, id)==true)
					return;
				//print("Render label: " + n + "    id: " + id);
				OctAstDic.Add(n,id);
				
				MakeAnAsteroid(info);
			}
			
		}
		
	}

	public static void AddAsteroidBySpace(List<string> toadd)
	{
		//note by wzh : toadd is the list of octants' indices that needs to be added.
		if(toadd.Count == 0)
			return;
		foreach(string n in toadd)
		{
			if(OctAstDic.ContainsKey(n)==true && n!=Initialize.FirstAsteroidLabel)
			{
				//print("AddAsteroidBySpace(): this octant is not new! --" + n);
				continue;
			}
		}
	}
	
	public static void DeleteAsteroidBySpace(List<string> octs)
	{
		if(octs.Count == 0)
			return;
		
		List<string> asteroidids;
		foreach(string o in octs)
		{
			if(OctAstDic.ContainsKey(o) == false)
			{
				continue;
			}
			
			asteroidids = OctAstDic.Get(o);
			foreach(string id in asteroidids)
			{
				if(id=="" && id==null)
					continue;
				
				AstDustbin.Enqueue(id);
				
			}
		}

	}
		*/
	/*
	public static Vector3 MakeAnAsteroid(string info, bool activate = false)
	{
		
		Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(info);
		string id = values["fs"];
		Vector3 position = M.GetGameCoordinates(values["latitude"], values["longitude"]);
		string label = M.GetLabel(position);
		
		Transform newAsteroid = Instantiate(Tree2, position, Quaternion.identity) as Transform;
		
		newAsteroid.name = id;
		//newAsteroid.transform.localScale = new Vector3(1000f,1000f,1000f);
		newAsteroid.tag = "Asteroid";
		newAsteroid.parent = AsteroidParent;
		newAsteroid.Find("label").GetComponent<GUIText>().text = M.PREFIX + "/asteroid/" + id + "\n" 
			+ M.PREFIX + "/asteroid/octant/" + label + "/" + id;
		ControlLabels.ApplyAsteroidName(newAsteroid);
		
		if(activate == true)
		{
			newAsteroid.GetComponent<TreeScript>().Activate();
		}
		return position;
	}
	*/
	
	public static UnityEngine.Vector3 MakeAnAsteroid(string id, UnityEngine.Vector3 position, bool activate = false)
	{
		// Generate DiscoveryModule.Vector3 from UnityEngine.Vector3 
		remap.NDNMOG.DiscoveryModule.Vector3 vector3 = new remap.NDNMOG.DiscoveryModule.Vector3(position.x, position.y, position.z);
		string label = CommonUtility.GetLabel(vector3);
		
		Transform newAsteroid = Instantiate(Tree2, position, Quaternion.identity) as Transform;
		
		newAsteroid.name = id;
		//newAsteroid.transform.localScale = new Vector3(1000f,1000f,1000f);
		newAsteroid.tag = "Asteroid";
		newAsteroid.parent = AsteroidParent;
		newAsteroid.Find("label").GetComponent<GUIText>().text = "/asteroid/" + id + "\n"  + "/asteroid/octant/" + label + "/" + id;
		ControlLabels.ApplyAsteroidName(newAsteroid);
		
		if(activate == true)
		{
			newAsteroid.GetComponent<TreeScript>().Activate();
		}
		return position;
	}
}
