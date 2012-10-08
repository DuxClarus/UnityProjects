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
	public RocketScript theRocket;
	
	//some constants 
	const float MASS_EARTH_IN_KG = 	5.9736e+24f;
	const float DENSITY_EARTH_IN_KG_PER_CUBIC_METER = 5515.0f;
	const float G = 6.67300e-11f;				// Gravitational constant in m^3/(kg*s^2)
	
	//Earths's information (for this run of the game)
	public float earthRadius = 0.0f;			//in meters
	public float earthEscape = 0.0f;			//in m/s
	
	public float timeBoost = 1.0f;			//Give the game time a BOOST if shift if held!
	int score = 0;
	int highScore = 0;
	
	// Use this for initialization
	void Start () 
	{
		//the initialization state avoids a race condition with the ball's Start() function
		theGameState = GameStates.GS_INITIALIZATION;
		highScore = 0;
		score = 0;
	}
	
	public void Restart()
	{
		theGameState = GameStates.GS_USER_INPUT;
		
		earthRadius = Mathf.Pow((3.0f * MASS_EARTH_IN_KG) / (4.0f * Mathf.PI * DENSITY_EARTH_IN_KG_PER_CUBIC_METER), 1.0f/3.0f);		//in meters
		earthEscape = CalculateEscapeVelocityFromEarth(earthRadius);
		
		print("Earth's Mass = " + MASS_EARTH_IN_KG + "kg, earthRadius = " + earthRadius/1000.0f + "km, escape velocity = " + earthEscape + "m/s");
		
		//reset the rocket
		if (theRocket != null)
		{
			theRocket.Restart();
		}
	}
	
	//given a radius in meters, returns m/s
	public float CalculateEscapeVelocityFromEarth(float radiusFromCore)
	{
		return Mathf.Sqrt(2.0f * G * MASS_EARTH_IN_KG / radiusFromCore);
	}
	
	//given a radius in meters, returns Newtons (kg*m/s^2)
	public float ForceOfGravity(float radiusFromCore)
	{
		return G * MASS_EARTH_IN_KG * theRocket.CurrentRocketMass() / (radiusFromCore * radiusFromCore);
	}
	
	// Be careful!  OnGUI may be called many times a frame!  Therefore, we just update the display and let Update() handle user input
	void OnGUI()
	{
		GUIStyle normalStyle =  GUI.skin.box;
		GUIStyle warningStyle = new GUIStyle(normalStyle);
		warningStyle.normal.textColor = Color.red;
		
		if (theRocket == null || theRocket.stage2 == null)
		{
			//hint: if your game UI never displays, you might want to check this condition somehow...
			return;
		}
		
		float escapeVel = 0.0f;
		switch (theGameState)
		{
		case GameStates.GS_USER_INPUT:
			
			bool rocketCanLaunch = ForceOfGravity(earthRadius) <= theRocket.ROCKET_THRUST_WHEN_ON;
			
			//helpful text
			if (rocketCanLaunch)
			{
				GUI.Box(new Rect(Screen.width/2 - 100, 10, 200, 25), "Hit SPACE to launch the rocket!");
			}
			else
			{
				GUI.Box(new Rect(Screen.width/2 - 100, 10, 200, 25), "Rocket cannot launch!", warningStyle);
			}
			GUI.Box(new Rect(Screen.width/2 - 240, Screen.height - 90, 480, 25), "Up and Down arrow keys adjust the amount of the rocket's stage 1 fuel.");
			GUI.Box(new Rect(Screen.width/2 - 240, Screen.height - 60, 480, 25), "W and S keys adjust the amount of the rocket's stage 2 fuel.");
			GUI.Box(new Rect(Screen.width/2 - 120, Screen.height - 30, 240, 25), "Shift key boosts the game's speed!");
			
			//display the current launch paramaters
			GUI.Box(new Rect(Screen.width - 150, Screen.height/2 - 65, 100, 25), "The Rocket");
			GUI.Box(new Rect(Screen.width - 180, Screen.height/2-30, 160, 25), string.Format("Stage 1 Fuel: {0:F2}kg", theRocket.kgOfFuel));
			GUI.Box(new Rect(Screen.width - 180, Screen.height/2, 160, 25), string.Format("Stage 2 Fuel: {0:F2}kg", theRocket.stage2.kgOfFuel));
			GUIStyle weightStyle = (!rocketCanLaunch) ? warningStyle : normalStyle;
			GUI.Box(new Rect(Screen.width - 190, Screen.height/2+30, 180, 25), string.Format("Total Weight: {0:F2} kN", 
			                                                                                 ForceOfGravity(earthRadius) / 1000.0f), weightStyle);
			GUI.Box(new Rect(Screen.width - 190, Screen.height/2+60, 180, 25), string.Format("Liftoff Thrust: {0:F2} kN", 
			                                                                                 theRocket.ROCKET_THRUST_WHEN_ON/1000.0f));
			
			//display the planet's attributes
			GUI.Box(new Rect(25, Screen.height/2 - 35, 160, 25), "Earth");
			GUI.Box(new Rect(10, Screen.height/2, 190, 25), string.Format("Radius: {0:F1} km", earthRadius / 1000.0f));
			GUI.Box(new Rect(10, Screen.height/2+30, 190, 25), string.Format("Surface Gravity: {0:F2} m/s^2", 
			                                                              ForceOfGravity(earthRadius) / (theRocket.CurrentRocketMass())));
			escapeVel = CalculateEscapeVelocityFromEarth(earthRadius);
			if (escapeVel > 1000.0f)
			{
				GUI.Box(new Rect(10, Screen.height/2+60, 190, 25), string.Format("Escape Velocity: {0:F2} km/s", 
				                                                              CalculateEscapeVelocityFromEarth(earthRadius)/1000.0f));
			}
			else
			{
				GUI.Box(new Rect(10, Screen.height/2+60, 190, 25), string.Format("Escape Velocity: {0:F2} m/s", 
				                                                              CalculateEscapeVelocityFromEarth(earthRadius)));
			}
			break;
			
		case GameStates.GS_GAME_IN_PROGRESS:
			
			//display the current velocity
			float speed = theRocket.velocity.y;
			float fuel = theRocket.kgOfFuel;
			if (theRocket.isFiring == false)
			{
				speed = theRocket.stage2.velocity.y;
				fuel = theRocket.stage2.kgOfFuel;
			}
			GUI.Box(new Rect(Screen.width - 110, Screen.height/2 - 35, 100, 25), "Rocket's Y Vel");
			if (speed > 1000.0f)
			{
				GUI.Box(new Rect(Screen.width - 100, Screen.height/2, 80, 25), string.Format("{0:F2}km/s", speed/1000.0f));
			}
			else
			{
				GUI.Box(new Rect(Screen.width - 100, Screen.height/2, 80, 25), string.Format("{0:F2}m/s", speed));
			}
			GUI.Box(new Rect(Screen.width - 150, Screen.height/2+30, 180, 25), string.Format("Fuel: {0:F2}kg", fuel));
			
			//display the target's height
			float height = theRocket.transform.position.y - theRocket.initPosition.y;
			if (theRocket.isFiring == false)
			{
				height = theRocket.stage2.transform.position.y - theRocket.initPosition.y;
			}
			GUI.Box(new Rect(55, Screen.height/2 - 35, 100, 25), "Rocket Height");
			
			if (height > 1000.0f)
			{
				GUI.Box(new Rect(20, Screen.height/2, 180, 25), string.Format("{0:F2}km", height/1000.0f));
			}
			else
			{
				GUI.Box(new Rect(20, Screen.height/2, 180, 25), string.Format("{0:F2}m", height));
			}
			float distFromEarthCore = earthRadius + height;
			GUI.Box(new Rect(10, Screen.height/2+30, 200, 25), string.Format("Current Gravity: {0:F2} m/s^2", 
			                                                              ForceOfGravity(distFromEarthCore) / (theRocket.CurrentRocketMass())));
			escapeVel = CalculateEscapeVelocityFromEarth(distFromEarthCore);
			if (escapeVel > 1000.0f)
			{
				GUI.Box(new Rect(10, Screen.height/2+60, 200, 25), string.Format("New Escape Velocity: {0:F2} km/s", 
				        													  CalculateEscapeVelocityFromEarth(distFromEarthCore)/1000.0f));
			}
			else
			{
				GUI.Box(new Rect(10, Screen.height/2+60, 200, 25), string.Format("New Escape Velocity: {0:F2} m/s", 
				        													  CalculateEscapeVelocityFromEarth(distFromEarthCore)));
			}
			break;
			
		case GameStates.GS_ENDGAME_VICTORY:
			
			//Victory text
			GUI.Box(new Rect(Screen.width/2 - 100, 20, 200, 25), "YOU WIN!");
			if (score == highScore)
			{
				GUI.Box(new Rect(Screen.width/2 - 100, Screen.height/2+30, 200, 25), "NEW HIGH SCORE!", warningStyle);
			}
			GUI.Box(new Rect(Screen.width/2 - 100, Screen.height/2, 200, 25), string.Format("Your Score: {0}", score));
			GUI.Box(new Rect(Screen.width/2 - 100, Screen.height - 30, 200, 25), "Press SPACE to try again.");
			break;
			
		case GameStates.GS_ENDGAME_DEFEAT:
			
			//Defeat text
			GUI.Box(new Rect(Screen.width/2 - 100, Screen.height/2, 200, 25), "Better luck next time.");
			GUI.Box(new Rect(Screen.width/2 - 100, Screen.height - 30, 200, 25), "Press SPACE to try again.");
			
			//display the current velocity
			speed = theRocket.velocity.y;
			if (theRocket.isFiring == false)
			{
				speed = theRocket.stage2.velocity.y;
			}
			GUI.Box(new Rect(Screen.width - 110, Screen.height/2 - 35, 100, 25), "Rocket's Y Vel");
			if (speed > 1000.0f)
			{
				GUI.Box(new Rect(Screen.width - 100, Screen.height/2, 80, 25), string.Format("{0:F2}km/s", speed/1000.0f));
			}
			else
			{
				GUI.Box(new Rect(Screen.width - 100, Screen.height/2, 80, 25), string.Format("{0:F2}m/s", speed));
			}
			
			//display the target's height
			height = theRocket.transform.position.y - theRocket.initPosition.y;
			if (theRocket.isFiring == false)
			{
				height = theRocket.stage2.transform.position.y - theRocket.initPosition.y;
			}
			GUI.Box(new Rect(55, Screen.height/2 - 35, 100, 25), "Rocket Height");
			if (height > 1000.0f)
			{
				GUI.Box(new Rect(20, Screen.height/2, 180, 25), string.Format("{0:F2}km", height/1000.0f));
			}
			else
			{
				GUI.Box(new Rect(20, Screen.height/2, 180, 25), string.Format("{0:F2}m", height));
			}
			break;
			
		}
	}
	
	public float BoostedDeltaTime()
	{
		return Time.deltaTime * timeBoost;
	}
	
	public void RecordVictory(float kgOfFuelLeft)
	{
		score = (int)Mathf.Max(1.0f, 250000.0f - kgOfFuelLeft);
		if (score > highScore)
		{
			highScore = score;
		}
		theGameState = GameStates.GS_ENDGAME_VICTORY;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (theRocket == null || theRocket.stage2 == null)
		{
			//hint: if your game UI never displays, you might want to check this condition somehow...
			return;
		}
		
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			timeBoost = 10.0f;
		}
		else
		{
			timeBoost = 1.0f;
		}
				
		switch (theGameState)
		{
		case GameStates.GS_INITIALIZATION:
			Restart();		//needed to avoid a race condition with the rocket's Start() function (yes, I know there's a better way to do this...)
			break;
			
		case GameStates.GS_USER_INPUT:
		
			bool rocketCanLaunch = ForceOfGravity(earthRadius) <= theRocket.ROCKET_THRUST_WHEN_ON;
			
			//increase or decrease the amount of fuel our rocket starts with
			if (Input.GetKey(KeyCode.UpArrow))
			{
				//BoostedDeltaTime() tells me how much time has passed in seconds since the last frame... boosted by a factor of 10 if shift is held down!
				theRocket.kgOfFuel = Mathf.Min(theRocket.MAX_AMOUNT_OF_FUEL, theRocket.kgOfFuel + 5000.0f * BoostedDeltaTime());
			}
			if (Input.GetKey(KeyCode.DownArrow))
			{
				theRocket.kgOfFuel = Mathf.Max(0.0f, theRocket.kgOfFuel - 5000.0f * BoostedDeltaTime());
			}
			
			//increase or decrease the amount of fuel our rocket starts with
			if (Input.GetKey(KeyCode.W))
			{
				//BoostedDeltaTime() tells me how much time has passed in seconds since the last frame... boosted by a factor of 10 if shift is held down!
				theRocket.stage2.kgOfFuel = Mathf.Min(theRocket.stage2.MAX_AMOUNT_OF_FUEL, theRocket.stage2.kgOfFuel + 5000.0f * BoostedDeltaTime());
			}
			if (Input.GetKey(KeyCode.S))	
			{
				theRocket.stage2.kgOfFuel = Mathf.Max(0.0f, theRocket.stage2.kgOfFuel - 5000.0f * BoostedDeltaTime());
			}
			
			//check for game start
			if (rocketCanLaunch && Input.GetKeyDown(KeyCode.Space))
			{
				theGameState = GameStates.GS_GAME_IN_PROGRESS;
				theRocket.isFiring = true;
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
