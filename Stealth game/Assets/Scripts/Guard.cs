using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour {

	public Transform pathHolder;
	public float guardMoveSpeed;
	public float guardWaitTime;
	public float guardRotationSpeed;
	public float guardViewDistance;
	public Light spotLight;
	public LayerMask viewMask;

	float guardViewAngle;
	float waypointMarkerSize = 0.3f;
	Vector3[] waypoints;
	Transform player;
	Color originalSpotlightColor;

	void Start()
	{
		//get a reference of the player using it's tag
		player = GameObject.FindGameObjectWithTag("Player").transform;

		//save the spotlights angle in a variable
		guardViewAngle = spotLight.spotAngle;

		//save the guard's original spotlight color
		originalSpotlightColor = spotLight.color;

		//create an array to store the waypoint positions - the size will be the number of childs in pathholder
		waypoints = new Vector3[pathHolder.childCount];

		//assign waypoint positions from the patholder to the array elements
		for (int i = 0; i < waypoints.Length; i++)
		{
			waypoints[i] = pathHolder.GetChild(i).position;
			//set the waypoints height (y) so that it matches the guards height - guard won't sink in the ground
			waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
		}

		//guard path following coroutine
		StartCoroutine(FollowPath(waypoints));

	}

	bool canSeePlayer()
	{
		//first check: check if the player is within the guard's view distance
		if (Vector3.Distance(transform.position, player.position) < guardViewDistance)
		{
			//get direction vector to the player
			Vector3 directionToPlayer = (player.position - transform.position).normalized;
			//get the angle between guard and player (max 180 degree)
			float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward, directionToPlayer);

			//second check: check if the angle between the guard's forward direction and the direction to the player is within the guard's viewangle
			if (angleBetweenGuardAndPlayer < guardViewAngle / 2)
			{
				//third check: if the ray between guard and player have not hit anything
				if (!Physics.Linecast(transform.position, player.position, viewMask))
				{
					return true;
				}
			}
		}
		return false;	
	}

	private void Update()
	{
		//if guard sees player change spotlight to red, else to it's original color
		if (canSeePlayer())
			spotLight.color = Color.red;
		else
			spotLight.color = originalSpotlightColor;
	}


	//add a sphere to waypoints so they are visible
	private void OnDrawGizmos()
	{
		//starting position is the position of the first marker, previousposition equals starposition at start
		Vector3 waypointStartPosition = pathHolder.GetChild(0).position;
		Vector3 waypointPreviousPosition = waypointStartPosition;

		//loop through the waypoints
		foreach (Transform waypoints in pathHolder)
		{
			//draw a spehere at each waypoint
			Gizmos.DrawWireSphere(waypoints.position, waypointMarkerSize);
			//draw a line continously following the waypoints' order
			Gizmos.DrawLine(waypointPreviousPosition, waypoints.position);
			//after a line has been drawn, set the previous position the waypoint we just drew a line to
			waypointPreviousPosition = waypoints.position;
		}

		//after we looped through the pathholder, draw a line from the last waypoint to the starting position
		Gizmos.DrawLine(waypointPreviousPosition, waypointStartPosition);

		//draw a red line representing the guard's view distance
		Gizmos.color = Color.red;
		Gizmos.DrawRay(transform.position, transform.forward * guardViewDistance);
	}
	
	//coroutine to move guard through it's path
	IEnumerator FollowPath (Vector3[] waypoints)
	{
		//set the guard's position to the first waypoint position
		transform.position = waypoints[0];

		//keep track of the target waypoint index and set the next target
		int targetWaypointIndex = 1;
		Vector3 targetWaypoint = waypoints[targetWaypointIndex];

		//make the guard face the target waypoint initially
		transform.LookAt(targetWaypoint);

		//keep moving the guard 
		while (true)
		{
			//move the guard to the next waypoint
			transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, guardMoveSpeed * Time.deltaTime);

			//when we reach the targetted waypoint
			if (transform.position == targetWaypoint)
			{
				//set the next waypointindex by adding 1 and % with waypoint length --> if we reach the last waypointindex we will return to the first one
				targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
				targetWaypoint = waypoints[targetWaypointIndex];
				//have the guard wait for waitTime seconds
				yield return new WaitForSeconds(guardWaitTime);

				//wait until the guard is rotating
				yield return StartCoroutine(RotateGuard(targetWaypoint));

			}
			//yield for one frame between each iteration of the while loop
			yield return null;
		}
	}

	
	IEnumerator RotateGuard(Vector3 target)
	{
		//calculate the normal vector between our current position and the position the guard will rotate towards
		Vector3 directionToLookAt = (target - transform.position).normalized;

		//angle to target: arc tangent(x/z) and convert it to degrees.
		//substract this from 90 due to unity and trigonometric angle differences (or just switch x and z in atan2)
		float targetAngle = 90 - Mathf.Atan2(directionToLookAt.z, directionToLookAt.x) * Mathf.Rad2Deg;

		//check if our guards angle is different than the target angle (calc the difference, use a small value because it won't be 0)
		while ( Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f )
		{
			//calculate the guards current angle along y axis and add speed to it
			float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, guardRotationSpeed * Time.deltaTime);
			//rotate the guard and wait for each frame
			transform.eulerAngles = Vector3.up * angle;
			yield return null;
		}

	}
	
	



}
