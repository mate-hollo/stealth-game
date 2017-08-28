using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public float moveSpeed;
	public float smoothMoveTime;
	public float turnSpeed;

	float smoothInputMagnitude;
	float smoothMoveVelocity;
	float angle;

	Rigidbody rb;

	Vector3 velocity;


	void Start()
	{
		//store the player's rigidbody in a variable if it has any
		rb = GetComponent<Rigidbody> ();
	}

	void Update () {
		//get a direction vector of the player and normalize it
		Vector3 inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
		
		//get the length of the player direction vector -> this is needed because we want to move the player only when we press the keys
		float inputMagnitude = inputDirection.magnitude;
		//smooth magnitude for smoother movement
		smoothInputMagnitude = Mathf.SmoothDamp(smoothInputMagnitude, inputMagnitude, ref smoothMoveVelocity, smoothMoveTime);


		//calculate the target angle
		float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
		//lerp between our current ang target angle, but stop interpolating when magnitude is zero (otherwise it will reset the direction when player is stopped)
		angle = Mathf.LerpAngle(angle, targetAngle, Time.deltaTime * turnSpeed * inputMagnitude);

		//add velocity to the player, which will be used during movement
		velocity = transform.forward * moveSpeed * smoothInputMagnitude;


		//apply rotation to the player -> use if we dont have rigidbody
		//transform.eulerAngles = Vector3.up * angle;

		//move the player the direction it is currently rotated to -> use if we dont have rigidbody
		//transform.Translate(transform.forward * moveSpeed * Time.deltaTime * smoothInputMagnitude, Space.World);
	}

	void FixedUpdate()
	{
		//apply rotation to the player who has rigidbody
		rb.MoveRotation(Quaternion.Euler(Vector3.up * angle));
		//move the player with rigidbody - time.deltatime here is time.fixeddeltatime
		rb.MovePosition(rb.position + velocity * Time.deltaTime);
	}
}
