using UnityEngine;
using System.Collections;

public class CannonScript : MonoBehaviour 
{	
	public GameManager	theGameManager;
	
	//For those in GAM240: this is a preview of hierarchical animations... tho this is only 1 bone with 1 offset
	//how far is the center of the cannon's pivort point away from the cylinder's center?
	Vector3 pivotOffset = new Vector3(0.0f, -0.5f, 0.0f);
	Vector3 xAxis = new Vector3(1, 0, 0);
	Vector3 yAxis = new Vector3(0, 1, 0);
	Vector3 zAxis = new Vector3(0, 0, 1);
	public Vector3 pivotPoint;						//in world space
	public Vector3 initPoint;
	
	public float localElevationAngle = 15.0f;
	public float localRotationAngle = 0.0f;
	public float initPivotAngle;
	
	const float CANNON_LENGTH = 1.5f;

	// Use this for initialization
	void Start () 
	{
		initPoint = transform.position;
		pivotPoint = transform.position + pivotOffset;
		Restart();
	}
	
	public void Restart()
	{
		//localPivotAngle = 0.0f;
	}
	
	//no longer actually moves the ball; the ball will have to do that
	public void GetSpawnBallPosition(out Vector3 ballPos)
	{
		ballPos = Vector3.zero;
		
		float elevationInRadians = Mathf.Deg2Rad * localElevationAngle;
		float yawInRadians = Mathf.Deg2Rad * localRotationAngle;
		
		ballPos.x = CANNON_LENGTH * Mathf.Cos(elevationInRadians) * Mathf.Sin(yawInRadians);
		ballPos.y = CANNON_LENGTH * Mathf.Sin(elevationInRadians);
		ballPos.z = CANNON_LENGTH * Mathf.Cos(elevationInRadians) * Mathf.Cos(yawInRadians);
		ballPos += pivotPoint;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (theGameManager == null)
		{
			//hint: if your cannon is doing nothing, you might want to check this condition somehow...
			return;
		}
		
		//DEBUG TEST!
		//recalculate the pivotPoint
		pivotPoint = initPoint + pivotOffset;
		
		//first, reset my rotation & position
		transform.LookAt(transform.position + zAxis);
		transform.Translate(initPoint - transform.position);
		
		//now, apply the new rotation information
		float newElevationAngle = 90.0f - localElevationAngle;		// this will make a elevation of 90 degrees face upward, and 0 to the front
		//print("newElevationAngle = " + newElevationAngle);
		transform.RotateAround(pivotPoint, xAxis, newElevationAngle);
		transform.RotateAround(pivotPoint, yAxis, localRotationAngle);
	}
}
