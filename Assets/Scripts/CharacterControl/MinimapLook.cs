using UnityEngine;
using System.Collections;

/// MouseLook rotates the transform based on the mouse delta.
/// Minimum and Maximum values can be used to constrain the possible rotation

/// To make an FPS style character:
/// - Create a capsule.
/// - Add the MouseLook script to the capsule.
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
/// - Add FPSInputController script to the capsule
///   -> A CharacterMotor and a CharacterController component will be automatically added.

/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add a MouseLook script to the camera.
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)
/// 
/// This script is taken from built-in Unity C#, 
/// the step of attaching it to Character and use X for Character does the trick for looking left and right
/// and the last step of attaching it to Camera and use Y for Camera does the trick for looking up and down

[AddComponentMenu("Camera-Control/Mouse Look")]
public class MinimapLook : MonoBehaviour {
	public Transform Target;
	
	void Update ()
	{
		transform.position = new Vector3(Target.position.x, transform.position.y, Target.position.z);
	}
	
	void Start ()
	{
		
	}
}