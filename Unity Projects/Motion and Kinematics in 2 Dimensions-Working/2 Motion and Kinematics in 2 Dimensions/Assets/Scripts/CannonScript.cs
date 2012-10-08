using UnityEngine;
using System.Collections;

public class CannonScript : MonoBehaviour 
{	
	public GameManager	theGameManager;
	
	//For those in GAM240: this is a preview of hierarchical animations... tho this is only 1 bone with 1 offset
	//how far is the center of the cannon's pivort point away from the cylinder's center?
	Vector3 pivotOffset = new Vector3(0.5f, 0.0f, 0.0f);
	Vector3 zAxis = new Vector3(0,0,1);
	public Vector3 pivotPoint;						//in world space
	
	public float localPivotAngle = 0.0f;
	public float initPivotAngle;
	
	const float CANNON_LENGTH = 1.5f;

	// Use this for initialization
	void Start () 
	{		
		pivotPoint = transform.position + pivotOffset;
		Restart();
	}
	
	public void Restart()
	{
		localPivotAngle = 0.0f;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (theGameManager == null)
		{
			//hint: if your cannon is doing nothing, you might want to check this condition somehow...
			return;
		}
		initPivotAngle = transform.rotation.eulerAngles.z;
		float newPivotAngle = (90.0f - localPivotAngle) - initPivotAngle;		// this will make a pivot of 90 degrees face upward, and 0 to the right
		transform.RotateAround(pivotPoint, zAxis, newPivotAngle);
	}
	
	public void SpawnBall()
	{
		if (theGameManager == null)
		{
			//hint: if your cannon is doing nothing, you might want to check this condition somehow...
			return;
		}
		
		Vector3 ballSpawnPos = new Vector3(0,0,0);
		float localPivotInRadians = Mathf.Deg2Rad * localPivotAngle;
		//Polar Coordinates is your friend!
		//we subtract x, because the way the arena's built, the cannon is firing into the NEGATIVE X direction!
		ballSpawnPos.x = pivotPoint.x - CANNON_LENGTH * Mathf.Cos(localPivotInRadians);
		ballSpawnPos.y = pivotPoint.y + CANNON_LENGTH * Mathf.Sin(localPivotInRadians);

		//now, move the ball!
		if (theGameManager.theBall != null)
		{
			theGameManager.theBall.transform.Translate(ballSpawnPos - theGameManager.theBall.transform.position);
		}		
	}
}
