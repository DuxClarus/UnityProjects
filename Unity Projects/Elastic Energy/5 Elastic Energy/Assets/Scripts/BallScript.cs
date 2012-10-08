using UnityEngine;
using System.Collections;

public class BallScript : MonoBehaviour 
{
	public GameManager	theGameManager;
	public ReticuleScript	theReticule;
	public Material		redReticuleMaterial;
	public Material		greenReticuleMaterial;
	
	public Vector3 velocity = new Vector3(0,0,0);
	public Vector3 initPosition = new Vector3(0,0,0);
	public Vector3 wayOffScreen = new Vector3(0,0,-10000);
	
	//float launchSpeed = 150.0f;
	public float ballMass = 1.0f;
	
	//some consts
	const float gravity = 9.80665f;
	Vector3 gravityVector = new Vector3(0.0f, -gravity, 0.0f);		//simplify the math by making gravity a vector

	// Use this for initialization
	void Start () 
	{
		initPosition = transform.position;	//remember the ball's initial position
		velocity = Vector3.zero;			//sets the ball's velocity to (0,0,0)
	}
	
	public void Restart()
	{
		//this is how to move the ball from its current position back to its initial position
		transform.Translate(initPosition - transform.position);
		velocity = Vector3.zero;			//sets the ball's velocity back to (0,0,0)
	}
	
	// Update is called once per frame
	void Update()
	{
		if (theGameManager == null)
		{
			//hint: if your ball is doing nothing, you might want to check this condition somehow...
			return;
		}
		
		switch (theGameManager.theGameState)
		{
		case GameManager.GameStates.GS_USER_INPUT:
			transform.Translate(wayOffScreen - transform.position);
			break;
		
		case GameManager.GameStates.GS_GAME_IN_PROGRESS:
			//your code here!  Update the ball's position and velocity and check for game ending conditions
			Vector3 endPosition = new Vector3(0,0,0);
			Vector3 moveDelta = new Vector3(0,0,0);
			Vector3 unusedNormal = new Vector3(0,0,0);		//used for reticule placement, not for actual flight
			bool hitTarget = false;
			bool hitGround = false;
			DoMotion(transform.position, Time.deltaTime, ref velocity, out moveDelta);
			DoCollisionDetection(transform.position, moveDelta, out endPosition, 
			                     out unusedNormal, out hitTarget, out hitGround);
			ApplyMovement(endPosition);
			
			if (hitTarget)
			{
				theGameManager.theGameState = GameManager.GameStates.GS_ENDGAME_VICTORY;
			}
			else if (hitGround)
			{
				theGameManager.theGameState = GameManager.GameStates.GS_ENDGAME_DEFEAT;
			}
			
			theReticule.newPos = wayOffScreen;
			
			break;
		
		case GameManager.GameStates.GS_ENDGAME_VICTORY:
			break;
		
		case GameManager.GameStates.GS_ENDGAME_DEFEAT:
			break;
			
		}	
	}
	
	void DoMotion(Vector3 curPosition, float deltaTime, ref Vector3 curVelocity, out Vector3 moveDelta)
	{
		moveDelta = Vector3.zero;
		
		//apply gravity
		Vector3 initVelocity = new Vector3(0,0,0);
		initVelocity = curVelocity;
		
		curVelocity = curVelocity + gravityVector * deltaTime;
		
		//dPos = v * deltaTime
		//I'm taking the average velocity for more accuracy
		moveDelta = ((initVelocity + curVelocity) / 2.0f) * deltaTime;
	}
	
	void DoCollisionDetection(Vector3 curPosition, Vector3 moveDelta, out Vector3 endPosition,
	                          out Vector3 hitNormal, out bool hitTarget, out bool hitGround)
	{		
		//set the defaults
		hitTarget = false;
		hitGround = false;
		
		//find out where we think we'll end up
		endPosition = curPosition + moveDelta;
		hitNormal = Vector3.up;
		
		RaycastHit hitInfo = new RaycastHit();
		if (Physics.Linecast(curPosition, endPosition, out hitInfo))
		{
			if (hitInfo.collider != null && hitInfo.collider.gameObject != null)
			{
				//check against the target & the hill
				if (hitInfo.collider.gameObject == theGameManager.theTarget)
				{
					hitTarget = true;
					endPosition = hitInfo.point;
					hitNormal = hitInfo.normal;
				}
				else if (hitInfo.collider.gameObject == theGameManager.theHill)
				{
					hitGround = true;
					endPosition = hitInfo.point;
					hitNormal = hitInfo.normal;
				}
			}
		}
		
		if (!hitTarget && endPosition.y < theGameManager.GROUND_Y)
		{
			hitGround = true;
		}
	}
	
	public void SimulateFlight(BallistaScript theBallista)
	{
		Vector3 startPos = new Vector3(0,0,0);
		Vector3 endPos = new Vector3(0,0,0);
		Vector3 moveDelta = new Vector3(0,0,0);
		Vector3 simVelocity = new Vector3(0,0,0);
		Vector3 hitSurfaceNormal = new Vector3(0,1,0);
		bool hitTarget = false;
		bool hitGround = false;
		
		
		int sanity = 0;
		
		if (theReticule == null)
		{
			return;
		}
		
		if (theBallista != null)
		{
			theBallista.GetSpawnBallPosition(out startPos);
			GetLaunchVelocity(theBallista.localElevationAngle, theBallista.localRotationAngle, out simVelocity);
			
			//print("StartPos = " + startPos + " simVelocity = " + simVelocity);
			
			//simulate the flight in 1/5 second increments
			while (!hitTarget && !hitGround)
			{
				DoMotion(startPos, 0.1f, ref simVelocity, out moveDelta);
				DoCollisionDetection(startPos, moveDelta, out endPos, 
				                     out hitSurfaceNormal, out hitTarget, out hitGround);
				
				//set our start position to be equal to the end position and try again
				startPos = endPos;
				//print("simVelocity = " + simVelocity);
				
				sanity++;
				if (sanity > 1000)
				{
					break;
				}
			}
		}
		
		if (hitTarget)
		{
			theReticule.renderer.material = greenReticuleMaterial;
		}
		else
		{
			theReticule.renderer.material = redReticuleMaterial;
		}
		
		//print("hitTarget - " + hitTarget + " endPos = " + endPos + " sanity = " + sanity);
		
		//where should the reticule go, and what direction should it face?
		theReticule.newPos = endPos;
		theReticule.newOrientationInEulerAngles = Vector3.zero;
		float angleFromVertical = Vector3.Angle(hitSurfaceNormal, Vector3.up);
		if ((angleFromVertical > 1.0f) && (angleFromVertical < 45.0f))
		{
			//	bit of a hack... I know the orientation of the hill, 
			//	so if the reticule is hitting anything other than flat ground, 
			//	set it to the orientation of the hill
			theReticule.newOrientationInEulerAngles = theGameManager.theHill.transform.eulerAngles;
		}
	}
	
	public void ApplyMovement(Vector3 endPosition)
	{		
		//now that I know where I will end up after collisions, apply that motion
		Vector3 moveDelta = new Vector3(0,0,0);
		moveDelta = endPosition - transform.position;
	
		//apply moveDelta
		transform.Translate(moveDelta);
	}
	
	void GetLaunchVelocity(float cannonPitch, float cannonYaw, out Vector3 launchVelocity)
	{
		float pitchInRadians = Mathf.Deg2Rad * cannonPitch;
		float yawInRadians = Mathf.Deg2Rad * cannonYaw;
		
		float launchSpeed = 0.0f;
		if (theGameManager != null && theGameManager.theBallista != null)
		{
			theGameManager.theBallista.GetLaunchSpeed(ballMass, out launchSpeed);
		}
		
		float velInXZPlane = launchSpeed * Mathf.Cos(pitchInRadians);
		launchVelocity.x = velInXZPlane * Mathf.Sin(yawInRadians);
		launchVelocity.y = launchSpeed * Mathf.Sin(pitchInRadians);
		launchVelocity.z = velInXZPlane * Mathf.Cos(yawInRadians);
	}
	
	public void LaunchBall(float cannonPitch, float cannonYaw)
	{
		GetLaunchVelocity(cannonPitch, cannonYaw, out velocity);
	}
}
