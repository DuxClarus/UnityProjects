using UnityEngine;
using System.Collections;

public class BallScript : MonoBehaviour 
{
	public GameManager theGameManager;
	
	public Vector3 velocity = new Vector3(2,0,2);
	public Vector3 initPosition = new Vector3(0,0,0);
	public Vector3 wayOffScreen = new Vector3(1000.0f, 1000.0f, 0.0f);
	
	public float launchSpeed = 0.0f;
	public Vector3 initialVelocity = new Vector3(0,0,0);
	
	private GameObject targetArea;
	private int bounces;
	
	//some consts
	private const int MAX_BOUNCES = 3;
	const float gravity = 9.80665f;
	Vector3 gravityVector = new Vector3(0.0f, -gravity, 0.0f);		//simplify the math by making gravity a vector

	// Use this for initialization
	void Start () 
	{
		initPosition = transform.position;	//remember the ball's initial position
		velocity = Vector3.zero;
		initialVelocity = Vector3.zero;//sets the ball's velocity to (0,0,0)
		launchSpeed = 0.0f;
		
		targetArea = theGameManager.theTarget;
		//leftBorder = theGameManager.theLeftBorder;
	}
	
	public void Restart()
	{
		//this is how to move the ball from its current position back to its initial position
		transform.Translate(initPosition - transform.position);
		velocity = Vector3.zero;			//sets the ball's velocity back to (0,0,0)
		launchSpeed = 0.0f;
		bounces = 0;
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
				MotionAndCollisionDetection();
				checkWinConditions();
			break;
		
		case GameManager.GameStates.GS_ENDGAME_VICTORY:
			break;
		
		case GameManager.GameStates.GS_ENDGAME_DEFEAT:
			break;
			
		}
	
	}
	
	void MotionAndCollisionDetection()
	{
		Vector3 moveDelta = new Vector3(0,0,0);
		Vector3 currentPosition = new Vector3(0,0,0);
		Vector3 endPosition = new Vector3(0,0,0);
		float moveDistance = 0.0f;		
		
		if(velocity == Vector3.zero)
		{
			float x = launchSpeed * Mathf.Cos(Mathf.Deg2Rad * theGameManager.theCannon.localPivotAngle);
			float y = launchSpeed * Mathf.Sin(Mathf.Deg2Rad * theGameManager.theCannon.localPivotAngle);
			velocity = new Vector3(-x, y, 0);
		}
		currentPosition = transform.position;
		velocity = velocity + gravityVector * Time.deltaTime;
		moveDelta = velocity * Time.deltaTime;
		endPosition = transform.position + moveDelta;
		moveDistance = moveDelta.magnitude;
		
		while (moveDistance > 0)
		{
			float distanceMoved = 0.0f;
			float ratioMoved = 0.0f;
			if(endPosition.y <= theGameManager.ARENA_BOTTOM_Y)
			{
				ratioMoved = Mathf.Abs(currentPosition.y - theGameManager.ARENA_BOTTOM_Y) 
					/ Mathf.Abs(moveDelta.y);
				distanceMoved = moveDistance * ratioMoved;
				velocity.y *= -1.0f;
				currentPosition.x += moveDelta.x * ratioMoved;
				currentPosition.y = theGameManager.ARENA_BOTTOM_Y;
				moveDistance -= distanceMoved;
				moveDelta = velocity.normalized * moveDistance;
				endPosition = currentPosition + moveDelta;
				bounces++;
			}
			else if (endPosition.y >= theGameManager.ARENA_TOP_Y)
			{
				ratioMoved = Mathf.Abs(currentPosition.y - theGameManager.ARENA_TOP_Y)
					/ Mathf.Abs(moveDelta.y);
				distanceMoved = moveDistance  * ratioMoved;
				velocity.y *= -1.0f;
				currentPosition.x += moveDelta.x * ratioMoved;
				currentPosition.y = theGameManager.ARENA_TOP_Y;
				moveDistance -= distanceMoved;
				moveDelta = velocity.normalized * moveDistance;
				endPosition = currentPosition + moveDelta;
				bounces++;
			}
			else if(endPosition.x <= theGameManager.ARENA_RIGHT_X)
			{
				ratioMoved = Mathf.Abs(currentPosition.x - theGameManager.ARENA_RIGHT_X)
					/ Mathf.Abs(moveDelta.x);
				distanceMoved = moveDistance * ratioMoved;
				velocity.x *= -1.0f;
				currentPosition.x = theGameManager.ARENA_RIGHT_X;
				currentPosition.y += moveDelta.y * ratioMoved;
				moveDistance -= distanceMoved;
				moveDelta = velocity.normalized * moveDistance;
				endPosition = currentPosition + moveDelta;
				bounces++;
			}
			else if(endPosition.x >= theGameManager.ARENA_LEFT_X)
			{
				ratioMoved = Mathf.Abs(currentPosition.x - theGameManager.ARENA_LEFT_X)
					/ Mathf.Abs(moveDelta.x);
				distanceMoved = moveDistance * ratioMoved;
				velocity.x *= -1.0f;
				currentPosition.x = theGameManager.ARENA_LEFT_X;
				currentPosition.y += moveDelta.y * ratioMoved;
				moveDistance -= distanceMoved;
				moveDelta = velocity.normalized * moveDistance;
				endPosition = currentPosition + moveDelta;
				bounces++;
			}
			else {
				moveDistance =0.0f;
			}
		}
		moveDelta = endPosition - transform.position;
		transform.Translate(moveDelta);
	}
	
	void checkWinConditions()
    {
			float x = transform.position.x;
			float y = transform.position.y;
			
			float otherX = theGameManager.theTarget.transform.position.x;
			float otherY = theGameManager.theTarget.transform.position.y;
			float otherWidth = theGameManager.theTarget.transform.localScale.x;
			float otherHeight = theGameManager.theTarget.transform.localScale.y;
			if(x >= otherX - otherWidth / 2 && x <= otherX + otherWidth / 2)
			{
				if(y >= otherY - otherHeight && y <= otherY + otherHeight / 2)
				{
					theGameManager.theGameState = GameManager.GameStates.GS_ENDGAME_VICTORY;
				}
			}
			
			
			// Lose case
			if(bounces == MAX_BOUNCES)
			{
				theGameManager.theGameState = GameManager.GameStates.GS_ENDGAME_DEFEAT;
		}
    }
    
}
