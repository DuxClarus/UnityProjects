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
	
	//some constants to randomly calculate Terra Firma's mass relative to the mass of the Earth
	const float MIN_TF_MASS = 0.02f;
	const float MAX_TF_MASS = 0.3f;
	const float MASS_EARTH_IN_KG = 	5.9736e+24f;
	const float DENSITY_TF_IN_KG_PER_CUBIC_METER = 5515.0f;		//same as the Earth's for simplicity's sake
	const float G = 6.67300e-11f;				// Gravitational constant in m^3/(kg*s^2)
	
	//Terra Firma's information (for this run of the game)
	public float tfMass = 0.0f;			//in kg
	public float tfRadius = 0.0f;			//in meters
	public float tfEscape = 0.0f;			//in m/s
	
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
		
		//reset Terra Firma's mass
		tfMass = UnityEngine.Random.Range(MIN_TF_MASS, MAX_TF_MASS) * MASS_EARTH_IN_KG;
		tfRadius = Mathf.Pow((3.0f * tfMass) / (4.0f * Mathf.PI * DENSITY_TF_IN_KG_PER_CUBIC_METER), 1.0f/3.0f);		//in meters

		tfEscape = CalculateEscapeVelocityFromTerraFirma(tfRadius);
		
		print("tfMass = " + tfMass + "kg, tfRadius = " + tfRadius/1000.0f + "km, escape velocity = " + tfEscape + "m/s");
		
		//reset the rocket
		if (theRocket != null)
		{
			theRocket.Restart();
		}		
	}
	
	//given a radius in meters, returns m/s
	public float CalculateEscapeVelocityFromTerraFirma(float radiusFromCore)
	{
		return Mathf.Sqrt(2.0f * G * tfMass / radiusFromCore);
	}
	
	//given a radius in meters, returns Newtons (kg*m/s^2)
	public float ForceOfGravity(float radiusFromCore)
	{
		return G * tfMass * theRocket.CurrentRocketMass() / (radiusFromCore * radiusFromCore);
	}
	
	// Be careful!  OnGUI may be called many times a frame!  Therefore, we just update the display and let Update() handle user input
	void OnGUI()
	{
		GUIStyle normalStyle =  GUI.skin.box;
		GUIStyle warningStyle = new GUIStyle(normalStyle);
		warningStyle.normal.textColor = Color.red;
		
		if (theRocket == null)
		{
			//hint: if your game UI never displays, you might want to check this condition somehow...
			return;
		}
		
		float escapeVel = 0.0f;
		switch (theGameState)
		{
		case GameStates.GS_USER_INPUT:
			
			bool rocketCanLaunch = ForceOfGravity(tfRadius) <= RocketScript.ROCKET_THRUST_WHEN_ON;
			
			//helpful text
			if (rocketCanLaunch)
			{
				GUI.Box(new Rect(Screen.width/2 - 100, 10, 200, 25), "Hit SPACE to launch the rocket!");
			}
			else
			{
				GUI.Box(new Rect(Screen.width/2 - 100, 10, 200, 25), "Rocket cannot launch!", warningStyle);
			}
			GUI.Box(new Rect(Screen.width/2 - 240, Screen.height - 60, 480, 25), "Up and Down arrow keys adjust the amount of the rocket's initial fuel.");
			GUI.Box(new Rect(Screen.width/2 - 120, Screen.height - 30, 240, 25), "Shift key boosts the game's speed!");
			
			//display the current launch paramaters
			GUI.Box(new Rect(Screen.width - 150, Screen.height/2 - 35, 100, 25), "The Rocket");
			GUI.Box(new Rect(Screen.width - 180, Screen.height/2, 160, 25), string.Format("Fuel: {0:F2}kg", theRocket.kgOfFuel));
			GUIStyle weightStyle = (!rocketCanLaunch) ? warningStyle : normalStyle;
			GUI.Box(new Rect(Screen.width - 190, Screen.height/2+30, 180, 25), string.Format("Total Weight: {0:F2} kN", 
			                                                                                 ForceOfGravity(tfRadius) / 1000.0f), weightStyle);
			GUI.Box(new Rect(Screen.width - 190, Screen.height/2+60, 180, 25), string.Format("Liftoff Thrust: {0:F2} kN", 
			                                                                                 RocketScript.ROCKET_THRUST_WHEN_ON/1000.0f));
			
			//display the planet's attributes
			GUI.Box(new Rect(25, Screen.height/2 - 35, 160, 25), "Terra Firma");
			GUI.Box(new Rect(10, Screen.height/2, 190, 25), string.Format("{0:F2} x Mass of the Earth", tfMass / MASS_EARTH_IN_KG));
			GUI.Box(new Rect(10, Screen.height/2+30, 190, 25), string.Format("Radius: {0:F1} km", tfRadius / 1000.0f));
			GUI.Box(new Rect(10, Screen.height/2+60, 190, 25), string.Format("Surface Gravity: {0:F2} m/s^2", 
			                                                              ForceOfGravity(tfRadius) / (theRocket.CurrentRocketMass())));
			escapeVel = CalculateEscapeVelocityFromTerraFirma(tfRadius);
			if (escapeVel > 1000.0f)
			{
				GUI.Box(new Rect(10, Screen.height/2+90, 190, 25), string.Format("Escape Velocity: {0:F2} km/s", 
				                                                              CalculateEscapeVelocityFromTerraFirma(tfRadius)/1000.0f));
			}
			else
			{
				GUI.Box(new Rect(10, Screen.height/2+90, 190, 25), string.Format("Escape Velocity: {0:F2} m/s", 
				                                                              CalculateEscapeVelocityFromTerraFirma(tfRadius)));
			}
			break;
			
		case GameStates.GS_GAME_IN_PROGRESS:
			
			//display the current velocity
			GUI.Box(new Rect(Screen.width - 110, Screen.height/2 - 35, 100, 25), "Rocket's Y Vel");
			if (theRocket.velocity.y > 1000.0f)
			{
				GUI.Box(new Rect(Screen.width - 100, Screen.height/2, 80, 25), string.Format("{0:F2}km/s", theRocket.velocity.y/1000.0f));
			}
			else
			{
				GUI.Box(new Rect(Screen.width - 100, Screen.height/2, 80, 25), string.Format("{0:F2}m/s", theRocket.velocity.y));
			}
			GUI.Box(new Rect(Screen.width - 150, Screen.height/2+30, 180, 25), string.Format("Fuel: {0:F2}kg", theRocket.kgOfFuel));
			
			//display the target's height
			GUI.Box(new Rect(55, Screen.height/2 - 35, 100, 25), "Rocket Height");
			float height = theRocket.transform.position.y - theRocket.initPosition.y;
			if (height > 1000.0f)
			{
				GUI.Box(new Rect(20, Screen.height/2, 180, 25), string.Format("{0:F2}km", height/1000.0f));
			}
			else
			{
				GUI.Box(new Rect(20, Screen.height/2, 180, 25), string.Format("{0:F2}m", height));
			}
			float distFromTFCore = tfRadius + height;
			GUI.Box(new Rect(10, Screen.height/2+30, 200, 25), string.Format("Current Gravity: {0:F2} m/s^2", 
			                                                              ForceOfGravity(distFromTFCore) / (theRocket.CurrentRocketMass())));
			escapeVel = CalculateEscapeVelocityFromTerraFirma(tfRadius);
			if (escapeVel > 1000.0f)
			{
				GUI.Box(new Rect(10, Screen.height/2+60, 200, 25), string.Format("New Escape Velocity: {0:F2} km/s", 
				        													  CalculateEscapeVelocityFromTerraFirma(distFromTFCore)/1000.0f));
			}
			else
			{
				GUI.Box(new Rect(10, Screen.height/2+60, 200, 25), string.Format("New Escape Velocity: {0:F2} m/s", 
				        													  CalculateEscapeVelocityFromTerraFirma(distFromTFCore)));
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
			GUI.Box(new Rect(Screen.width - 110, Screen.height/2 - 35, 100, 25), "Rocket's Y Vel");
			if (theRocket.velocity.y > 1000.0f)
			{
				GUI.Box(new Rect(Screen.width - 100, Screen.height/2, 80, 25), string.Format("{0:F2}km/s", theRocket.velocity.y/1000.0f));
			}
			else
			{
				GUI.Box(new Rect(Screen.width - 100, Screen.height/2, 80, 25), string.Format("{0:F2}m/s", theRocket.velocity.y));
			}
			
			//display the target's height
			GUI.Box(new Rect(55, Screen.height/2 - 35, 100, 25), "Rocket Height");
			height = theRocket.transform.position.y - theRocket.initPosition.y;
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
		if (theRocket == null)
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
		
			bool rocketCanLaunch = ForceOfGravity(tfRadius) <= RocketScript.ROCKET_THRUST_WHEN_ON;
			
			//increase or decrease the launch velocity at a rate of 2 meters per second
			if (Input.GetKey(KeyCode.UpArrow))
			{
				//BoostedDeltaTime() tells me how much time has passed in seconds since the last frame... boosted by a factor of 10 if shift is held down!
				theRocket.kgOfFuel = Mathf.Min(RocketScript.MAX_AMOUNT_OF_FUEL, theRocket.kgOfFuel + 5000.0f * BoostedDeltaTime());
			}
			if (Input.GetKey(KeyCode.DownArrow))
			{
				theRocket.kgOfFuel = Mathf.Max(0.0f, theRocket.kgOfFuel - 5000.0f * BoostedDeltaTime());
			}
			
			//check for game start
			if (rocketCanLaunch && Input.GetKeyDown(KeyCode.Space))
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
