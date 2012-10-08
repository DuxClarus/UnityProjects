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
	public GameObject theHill;
	public CannonScript theCannon;
	
	//some constants
	const float HILL_UNSCALED_HALF_WIDTH = 5.0f;
	public float GROUND_Y = -200.0f;
	
	//calculate these
	float HILL_HALF_WIDTH = HILL_UNSCALED_HALF_WIDTH;

	
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
		if (theTarget != null && theHill != null)
		{
			Vector3 targetNewPosistion = new Vector3(theHill.transform.position.x, theHill.transform.position.y, theHill.transform.position.z);
			float hillXRotRadians = -Mathf.Deg2Rad * theHill.transform.rotation.eulerAngles.x;
			float distUpHill = UnityEngine.Random.Range(-1.0f, 1.0f) * HILL_UNSCALED_HALF_WIDTH * theHill.transform.localScale.z;
			
			//use polar coordinates & the hill's angle to randomly select a distance up the hill for the target
			targetNewPosistion.x += UnityEngine.Random.Range(-HILL_HALF_WIDTH, HILL_HALF_WIDTH) / 2.0f;
			targetNewPosistion.y += distUpHill * Mathf.Sin(hillXRotRadians);
			targetNewPosistion.z += distUpHill * Mathf.Cos(hillXRotRadians);
			/*print("targetNewPosition = " + targetNewPosistion + ", distUpHill = " + distUpHill + " hillXRotRadians = " + hillXRotRadians + 
			      "euler.x = " + theHill.transform.rotation.eulerAngles.x);*/
			theTarget.transform.Translate(targetNewPosistion - theTarget.transform.position);
		}
		
		//reset the ball
		if (theBall != null)
		{
			theBall.Restart();
		}
		
		//reset the cannon
		if (theCannon != null)
		{
			theCannon.Restart();
		}
	}
	
	// Be careful!  OnGUI may be called many times a frame!  Therefore, we just update the display and let Update() handle user input
	void OnGUI()
	{
		if (theBall == null || theTarget == null || theCannon == null || theHill == null)
		{
			//hint: if your game UI never displays, you might want to check this condition somehow...
			return;
		}
		
		switch (theGameState)
		{			
		case GameStates.GS_USER_INPUT:
			
			//helpful text
			GUI.Box(new Rect(Screen.width/2 - 100, 10, 200, 25), "Hit SPACE to launch the ball!");
			GUI.Box(new Rect(Screen.width/2 - 180, Screen.height - 60, 360, 25), "Up and Down arrow keys adjust the cannon's elevation.");
			GUI.Box(new Rect(Screen.width/2 - 180, Screen.height - 30, 360, 25), "Left and Right arrow keys adjust the cannon's angle.");
			
			//display the current launch speed & cannon angle
			GUI.Box(new Rect(Screen.width - 120, Screen.height/2 - 70, 120, 25), "Cannon Elevation");
			GUI.Box(new Rect(Screen.width - 110, Screen.height/2 - 35, 100, 25), string.Format("{0:F2} degrees", theCannon.localElevationAngle));
			GUI.Box(new Rect(Screen.width - 110, Screen.height/2 + 10, 100, 25), "Cannon Angle");
			GUI.Box(new Rect(Screen.width - 110, Screen.height/2 + 45, 100, 25), string.Format("{0:F2} degrees", theCannon.localRotationAngle));
			
			//display the target's height
			GUI.Box(new Rect(10, Screen.height/2 - 35, 100, 25), "Target Height");
			GUI.Box(new Rect(20, Screen.height/2, 80, 25), string.Format("{0:F2}m", theTarget.transform.position.y - theBall.initPosition.y));
			break;
			
		case GameStates.GS_GAME_IN_PROGRESS:
			
			//display the current velocity
			GUI.Box(new Rect(Screen.width - 110, Screen.height/2 - 35, 100, 25), "Ball's Speed");
			GUI.Box(new Rect(Screen.width - 100, Screen.height/2, 80, 25), string.Format("{0:F2}m/s", theBall.velocity.magnitude));
			
			//display the target's height
			GUI.Box(new Rect(10, Screen.height/2 - 35, 100, 25), "Target Height");
			GUI.Box(new Rect(20, Screen.height/2, 80, 25), string.Format("{0:F2}m", theTarget.transform.position.y - theBall.initPosition.y));
			break;
			
		case GameStates.GS_ENDGAME_VICTORY:
			
			//Victory text
			GUI.Box(new Rect(Screen.width/2 - 100, Screen.height/2, 200, 25), "YOU WIN!");
			GUI.Box(new Rect(Screen.width/2 - 100, Screen.height - 30, 200, 25), "Press SPACE to try again.");
			break;
			
		case GameStates.GS_ENDGAME_DEFEAT:
			
			//Defeat text
			GUI.Box(new Rect(Screen.width/2 - 100, Screen.height/2, 200, 25), "Better luck next time.");
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
			//calculate information about the hill			
			HILL_HALF_WIDTH = HILL_UNSCALED_HALF_WIDTH * theHill.transform.localScale.x;
			
			Restart();		//needed to avoid a race condition with the ball's Start() function
			break;
			
		case GameStates.GS_USER_INPUT:
			
			//increase or decreate the cannon's elevation by a rate of 10 degrees per second
			if (Input.GetKey(KeyCode.UpArrow))
			{
				theCannon.localElevationAngle = Mathf.Min(90.0f, theCannon.localElevationAngle + 10.0f * Time.deltaTime);
			}
			if (Input.GetKey(KeyCode.DownArrow))
			{
				theCannon.localElevationAngle = Mathf.Max(0.0f, theCannon.localElevationAngle - 10.0f * Time.deltaTime);
			}
			
			//increase or decreate the cannon's angle by a rate of 10 degrees per second
			if (Input.GetKey(KeyCode.RightArrow))
			{
				theCannon.localRotationAngle = Mathf.Min(60.0f, theCannon.localRotationAngle + 10.0f * Time.deltaTime);
			}
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				theCannon.localRotationAngle = Mathf.Max(-60.0f, theCannon.localRotationAngle - 10.0f * Time.deltaTime);
			}
			
			theBall.SimulateFlight(theCannon);
			
			//check for game start
			if (Input.GetKeyDown(KeyCode.Space))
			{
				Vector3 spawnPos = new Vector3(0,0,0);
				theCannon.GetSpawnBallPosition(out spawnPos);
				theBall.ApplyMovement(spawnPos);
				theBall.LaunchBall(theCannon.localElevationAngle, theCannon.localRotationAngle);
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
