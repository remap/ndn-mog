using UnityEngine;
using System.Collections;

public class move : MonoBehaviour {

	public static float speed = 4.0F;
    public static float jumpSpeed = 6.0F;
    public static float gravity = 10F;
	public static float autoflyspeed = 12F;
	
	
	public enum Mode {walk, fly};
	public static int currentmode = (int)Mode.walk;
	
    private Vector3 moveDirection = Vector3.zero;
    
	void Update() {
        CharacterController controller = GetComponent<CharacterController>();
		
		if(currentmode == (int)Mode.walk)
		{
			if (controller.isGrounded) {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;
            if (Input.GetButton("Jump"))
                moveDirection.y = jumpSpeed;
            
        	}
        	moveDirection.y -= gravity * Time.deltaTime;
        	controller.Move(moveDirection * Time.deltaTime);
		}
		else if(currentmode == (int)Mode.fly)
		{
			moveDirection = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 1);
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= autoflyspeed;
            
        	controller.Move(moveDirection * Time.deltaTime);
		}
        
    }
}
