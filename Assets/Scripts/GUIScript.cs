using UnityEngine;
using System.Collections;

public class GUIScript : MonoBehaviour {
	
	public Vector2 scrollPosition = Vector2.zero;
	
	// Use this for initialization
	void Start () {
		Player playerscript = new Player();
		for(int i=0; i<1; i++)
		{
			GameObject Obj = GameObject.Find("/objmodel");
			
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
			newobj.renderer.enabled = true;
			
			string NdnName = playerscript.WriteObjToRepo(pos,rot);
			newobj.name = NdnName;
		}
		
	}
	
	void OnGUI(){
		scrollPosition = GUI.BeginScrollView(new Rect(5, 5, Screen.width-5, Screen.height-5), 
											scrollPosition, 
											new Rect(0, 0, Screen.width, Screen.height*Player.PlayerList.Count/10));
		GUILayout.Label("Number of Players: " + Player.PlayerList.Count);
		GUILayout.Label ("List of Players: ");
		GUILayout.Label (Player.me + ": " + Player.player_pos.x + ", " + Player.player_pos.y + ", " + Player.player_pos.z + " (this is me)");
		ICollection keys = Player.PlayerList.Keys;
		IEnumerator kinum = keys.GetEnumerator();
		ICollection values = Player.PlayerList.Values;
		IEnumerator vinum = values.GetEnumerator();
		while(kinum.MoveNext() && vinum.MoveNext())
		{
			if(kinum.Current!=Player.me)
				GUILayout.Label("" + kinum.Current + ": " + vinum.Current);
		}
		 GUI.EndScrollView();
		
	}
}
