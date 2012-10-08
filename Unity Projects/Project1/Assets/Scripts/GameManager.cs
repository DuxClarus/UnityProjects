using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour 
{
	public enum GameStates
	{
		GS_INITIALIZATION=0,
		GS_USER_INPUT,
		GS_GAME_IN_PROGRESS,
		GS_ENDGAME_VICTORY,
		GS_ENDGAME_DEFEAT
	};
	
	public GameStates theGameState = GameManager.GameStates.GS_USER_INPUT;
	public BallScript theBall;
	public GameObject theTarget;
	
	//some constants
	const float MAX_LAUNCH_VEL = 100.0f;
	const float MAX_TARGET_HEIGHT = 15.0f;
	const float MIN_TARGET_HEIGHT = 5.0f;
	public float targetWidth = 2.0f;
	
	// Use this for initialization
	void Start () 
	{
		//the initialization state avoids a race condition with the ball's Start() function
		theGameState = GameStates.GS_INITIALIZATION;
	}
	
	public void Restart()
	{
		theGameState = GameStates.GS_USER_INPUT;
		
		//reset the target's height
		if (theTarget != null)
		{
			Vector3 targetNewPosistion = new Vector3(theBall.initPosition.x, theBall.initPosition.y, theBall.initPosition.z);
			targetNewPosistion.y += UnityEngine.Random.Range(MIN_TARGET_HEIGHT, MAX_TARGET_HEIGHT);
			theTarget.transform.Translate(targetNewPosistion - theTarget.transform.position);
		}
		
		//reset the ball
		if (theBall != null)
		{
			theBall.Restart();
		}		
	}
	
	// Be careful!  OnGUI may be called many times a frame!  Therefore, we just update the display and let Update() handle user input
	void OnGUI()
	{
		if (theBall == null || theTarget == null)
		{
			//hint: if your game UI never displays, you might want to check this condition somehow...
			return;
		}
		
		switch (theGameState)
		{
		case GameStates.GS_USER_INPUT:
			
			//helpful text
			GUI.Box(new Rect(Screen.width/2 - 100, 10, 200, 25), "Hit SPACE to launch the ball!");
			GUI.Box(new Rect(Screen.width/2 - 180, Screen.height - 30, 360, 25), "Up and Down arrow keys adjust the ball's launch velocity.");
			
			//display the current launch velocity
			GUI.Box(new Rect(Screen.width - 110, Screen.height/2 - 35, 100, 25), "Launch Y Vel");
			GUI.Box(new Rect(Screen.width - 100, Screen.height/2, 80, 25), string.Format("{0:F2}m/s", theBall.velocity.y));
			
			//display the target's height
			GUI.Box(new Rect(10, Screen.height/2 - 35, 100, 25), "Target Height");
			GUI.Box(new Rect(20, Screen.height/2, 80, 25), string.Format("{0:F2}m", theTarget.transform.position.y - theBall.initPosition.y));
			break;
			
		case GameStates.GS_GAME_IN_PROGRESS:
			
			//display the current velocity
			GUI.Box(new Rect(Screen.width - 110, Screen.height/2 - 35, 100, 25), "Ball's Y Vel");
			GUI.Box(new Rect(Screen.width - 100, Screen.height/2, 80, 25), string.Format("{0:F2}m/s", theBall.velocity.y));
			
			//display the target's height
			GUI.Box(new Rect(10, Screen.height/2 - 35, 100, 25), "Target Height");
			GUI.Box(new Rect(20, Screen.height/2, 80, 25), string.Format("{0:F2}m", theTarget.transform.position.y - theBall.initPosition.y));
			break;
			
		case GameStates.GS_ENDGAME_VICTORY:
			
			//Victory text
			GUI.Box(new Rect(Screen.width/2, Screen.height/2, 100, 25), "YOU WIN!");
			GUI.Box(new Rect(Screen.width/2 - 100, Screen.height - 30, 200, 25), "Press SPACE to try again.");
			break;
			
		case GameStates.GS_ENDGAME_DEFEAT:
			
			//Defeat text
			GUI.Box(new Rect(Screen.width/2, Screen.height/2, 200, 25), "Better luck next time.");
			GUI.Box(new Rect(Screen.width/2 - 100, Screen.height - 30, 200, 25), "Press SPACE to try again.");
			break;
			
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (theBall == null)
		{
			//hint: if your game UI never displays, you might want to check this condition somehow...
			return;
		}
				
		switch (theGameState)
		{
		case GameStates.GS_INITIALIZATION:
			Restart();		//needed to avoid a race condition with the ball's Start() function
			break;
			
		case GameStates.GS_USER_INPUT:
		
			//increase or decrease the launch velocity at a rate of 2 meters per second
			if (Input.GetKey(KeyCode.UpArrow))
			{
				//Time.deltaTime tells me how much time has passed in seconds since the last frame.
				theBall.velocity.y = Mathf.Min(MAX_LAUNCH_VEL, theBall.velocity.y + 2.0f * Time.deltaTime);
			}
			if (Input.GetKey(KeyCode.DownArrow))
			{
				theBall.velocity.y = Mathf.Max(0.0f, theBall.velocity.y - 2.0f * Time.deltaTime);
			}
			
			//check for game start
			if (Input.GetKeyDown(KeyCode.Space))
			{
				theGameState = GameStates.GS_GAME_IN_PROGRESS;
			}
			break;
			
		case GameStates.GS_GAME_IN_PROGRESS:
			break;
			
		case GameStates.GS_ENDGAME_VICTORY:
			
			//check for new game
			if (Input.GetKeyDown(KeyCode.Space))
			{
				Restart();
			}
			break;
			
		case GameStates.GS_ENDGAME_DEFEAT:
			
			//check for new game
			if (Input.GetKeyDown(KeyCode.Space))
			{
				Restart();
			}
			break;
			
		}
	}
}
