using UnityEngine;
using System.Collections;
using System;

public class discover : MonoBehaviour {

	
	void Start () {
	
		
		Vector3 pos = Birth();
		transform.position = pos;
		
		// compute labels for my current octant
		string label = GetLabel(pos);
        
	}
	
	Vector3 Birth()
	{
		// move player to a random legal position
		
		// random point on an egg surface
		float theta = UnityEngine.Random.Range(0f, 3.14f);
		float fi = UnityEngine.Random.Range(-3.14f, 3.14f);
		double x = 4000* 0.78*Mathf.Cos(theta/4)*Mathf.Sin(theta)*Mathf.Sin(fi);
		double y = 4000*1.0*Mathf.Cos(theta);
        double z = 4000* 0.78*Mathf.Cos(theta/4)*Mathf.Sin(theta)*Mathf.Cos(fi);
		
		// rotate the egg
		double xx = x;
		double yy = 0.7071*y - 0.7071*z;
		double zz = 0.7071*y + 0.7071*z;
		
		// translate the egg
		double xxx = xx + 4000;
		double yyy = yy + 4000;
		double zzz = zz + 4000;
		
		Vector3 pos = new Vector3((float)xxx,(float)yyy,(float)zzz);
		
		return pos;
	}
	
	string GetLabel(Vector3 position)
	{
		// decimal points in x,y,z
		// will not be used in this funciton
		
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
//		print(labels);
		
		return labels;
	}
	
	void Update () {
	
	}
}
