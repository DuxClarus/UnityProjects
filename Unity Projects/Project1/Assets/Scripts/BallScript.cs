using UnityEngine;
using System.Collections;

public class BallScript : MonoBehaviour 
{
	public GameManager	theGameManager;
	
	public Vector3 velocity = new Vector3(0,0,0);
	public Vector3 initPosition = new Vector3(0,0,0);
	
	//some consts
	const float gravity = 9.80665f;

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
	
	public bool checkTarget()
	{
		float targetHeight = theGameManager.theTarget.transform.position.y;
		float deltaY = targetHeight - transform.position.y;
		if(Mathf.Abs(deltaY) <= (theGameManager.targetWidth / 2.0f))
			return true;
		else
			return false;
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
				break;
			
			case GameManager.GameStates.GS_GAME_IN_PROGRESS:
				//your code here!  Update the ball's position and velocity and check for game ending conditions
				Vector3 deltaMove = new Vector3(0,0,0);
				float previousVelocity = velocity.y;
			
				//modify the velocity by gravity this frame
				velocity.y = velocity.y - (gravity * Time.deltaTime);
			
				//move the ball
				deltaMove.y = (velocity.y * Time.deltaTime);
				transform.Translate(deltaMove);
				//Vy = Vy - (gravity * time.deltaTime)
				//d = Vy * deltaT + d
				if((previousVelocity > 0) && (velocity.y <= 0.0f))
				{
					if(checkTarget() == true)
						theGameManager.theGameState = GameManager.GameStates.GS_ENDGAME_VICTORY;
				}
				else if(velocity.y < 0 && transform.position.y <= initPosition.y)
				{
					transform.position = initPosition;
					theGameManager.theGameState = GameManager.GameStates.GS_ENDGAME_DEFEAT;
				}
				break;
			
			case GameManager.GameStates.GS_ENDGAME_VICTORY:
				break;
			
			case GameManager.GameStates.GS_ENDGAME_DEFEAT:
				break;
			
		}
	
	}
}
