using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour {

	public Transform pathHolder;
	public float guardSpeed;
	public float guardWaitTime;

	private float waypointMarkerSize = 0.3f;
	Vector3[] waypoints;




	void Start()
	{
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

	void Update()
	{
		
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
	}
	
	//coroutine to move guard through it's path
	IEnumerator FollowPath (Vector3[] waypoints)
	{
		//set the guard's position to the first waypoint position
		transform.position = waypoints[0];

		//keep track of the target waypoint index and set the next target
		int targetWaypointIndex = 1;
		Vector3 targetWaypoint = waypoints[targetWaypointIndex];

        //keep moving the guard 
		while (true)
		{
			//move the guard to the next waypoint
			transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, guardSpeed * Time.deltaTime);

			//when we reach the targetted waypoint
			if (transform.position == targetWaypoint)
			{
				//set the next waypointindex by adding 1 and % with waypoint length --> if we reach the last waypointindex we will return to the first one
				targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
				targetWaypoint = waypoints[targetWaypointIndex];
				//have the guard wait for waitTime seconds
				yield return new WaitForSeconds(guardWaitTime);
			}
			//yield for one frame between each iteration of the while loop
			yield return null;
		}
	}



}
