using UnityEngine;
using System.Collections;
using System;

public class GUIScript : MonoBehaviour {
	
	
	void Start () {
		//ObjTextures =  Resources.LoadAll("GUI", typeof(Texture2D)) as Texture2D[];
		//ObjMaterials =  Resources.LoadAll("Materials", typeof(Material)) as Material[];
		ObjTextures = new Texture2D[6];
		ObjTextures[0] = Resources.Load("GUI/red") as Texture2D;
		ObjTextures[1] = Resources.Load("GUI/purple") as Texture2D;
		ObjTextures[2] = Resources.Load("GUI/green") as Texture2D;
		ObjTextures[3] = Resources.Load("GUI/alum") as Texture2D;
		ObjTextures[4] = Resources.Load("GUI/steel") as Texture2D;
		ObjTextures[5] = Resources.Load("GUI/rust") as Texture2D;
		ObjMaterials = new Material[6];
		ObjMaterials[0] = Resources.Load("Materials/red") as Material;
		ObjMaterials[1] = Resources.Load("Materials/purple") as Material;
		ObjMaterials[2] = Resources.Load("Materials/green") as Material;
		ObjMaterials[3] = Resources.Load("Materials/alum") as Material;
		ObjMaterials[4] = Resources.Load("Materials/steel") as Material;
		ObjMaterials[5] = Resources.Load("Materials/rust") as Material;
	}
	
	
	void OnGUI(){
		
		GUILayout.Window(0, new Rect(20, 20, 200, 50), InfoPanel, "Information");
		
		GUILayout.Window(1, new Rect(20, 120, 200, 50), CtrlPanel, "Create Some Objects");
		
		GUILayout.Window(2, new Rect(Screen.width-220, 20, 200, DetailHeight), Detail, "Players & Objects");
		
		
	}
	
	public bool ShowDetails = false;
	public Vector2 scrollPosition = Vector2.zero;
	public int DetailHeight = 50;
	void Detail(int windowID)
	{
		if(ShowDetails == false)
		{
			if(GUILayout.Button("Show Name List")) 
			{
				ShowDetails=true;
				DetailHeight = Screen.height-40;
			}
		}
		else // show details
		{
			
			if(GUILayout.Button("Hide Name List")) 
			{
				ShowDetails=false;
				DetailHeight = 50;
			}
			
			scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		
			ICollection keys = Player.PlayerList.Keys;
			IEnumerator kinum = keys.GetEnumerator();
			while(kinum.MoveNext())
			{
				GUILayout.Label("" + kinum.Current);
			}
			
			keys = Player.ObjList.Keys;
			kinum = keys.GetEnumerator();
			while(kinum.MoveNext())
			{
				GUILayout.Label("" + kinum.Current);
			}
			
		 	GUILayout.EndScrollView();
		 
		}
		
	}
	
	void InfoPanel(int windowID)
	{
		GUILayout.Label("# of players: " + Player.PlayerList.Count);
		
		GUILayout.Label("# of objects: " + Player.ObjList.Count);

	}
	
	// default parameters for the Control Panel (Create Some Objects)
	string ObjNum = "1";
	public int ObjColor = 0;
    public static Texture2D[] ObjTextures; 
	public static Material[] ObjMaterials; // not displayed but related
	void CtrlPanel(int windowID)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("How Many?");  
		ObjNum = GUILayout.TextArea (ObjNum, 20);
		GUILayout.EndHorizontal();
		
		GUILayout.Label("Which Color?");
		ObjColor = GUILayout.SelectionGrid(ObjColor, ObjTextures, 3);
		
		if(GUILayout.Button("Done!"))
		{
			CreateObjs(ObjNum,ObjColor);
		}
	}
	
	void CreateObjs(string num, int color)
	{
		
		int count;
		try
		{
			count = Int32.Parse(num);
		}
		catch
		{
			print ("CreateObjs() Error.");
			return;
		}
		
		for(int i=0; i<count; i++)
		{
			CreateSingleObj(color);
		}
	}
	
	void CreateSingleObj(int color)
	{
		GameObject Obj = GameObject.Find("/objmodel");
		Player playerscript = new Player();
		
			float radius = UnityEngine.Random.Range(0, Player.radius/4);
			float theta = UnityEngine.Random.Range(0, 360f);
			float beta = UnityEngine.Random.Range(0, 360f);
			float pos_x = radius * Mathf.Sin (theta);
			float pos_y = radius * Mathf.Sin (beta);
			float pos_z = radius * Mathf.Cos (theta);
			Vector3 pos = new Vector3(pos_x, pos_y, pos_z);
			float rot_x = UnityEngine.Random.Range(0,360f);
			float rot_y = UnityEngine.Random.Range(0,360f);
			float rot_z = UnityEngine.Random.Range(0,360f);
			Vector3 rot = new Vector3(rot_x, rot_y, rot_z);
			Quaternion quater = Quaternion.identity;
			quater.eulerAngles = rot;
			GameObject newobj = Instantiate(Obj, pos, quater) as GameObject;
			newobj.renderer.material = ObjMaterials[color];
			newobj.renderer.enabled = true;
			
			string NdnName = playerscript.WriteObjToRepo(pos,rot,color);
			newobj.name = NdnName;
			
	}
}
