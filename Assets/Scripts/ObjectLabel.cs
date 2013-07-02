using UnityEngine;
using System.Collections;
 
[RequireComponent (typeof (GUIText))]
public class ObjectLabel : MonoBehaviour {
 
public Transform target;  // Object that this label should follow
public Vector3 offset = new Vector3(0f, 1.5f, 0f);    // Units in world space to offset; 1 unit above object by default

public Camera cameraToUse ;   // Only use this if useMainCamera is false

 
    void Update()
    {
    	transform.position = cameraToUse.WorldToViewportPoint(target.position + offset);
    }
}