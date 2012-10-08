using UnityEngine;
using System.Collections;

public class RocketScript : MonoBehaviour 
{
	public GameManager theGameManager;
	public float kgOfFuel = 0.0f;
	public const float MASS_OF_EMPTY_ROCKET_IN_KG = 50000.0f;
	public const float MAX_AMOUNT_OF_FUEL = 750000.0f;			//in kg
	public const float ROCKET_THRUST_WHEN_ON = 5000000.0f;		//in Newtons (kg*m/s^2)
	public const float FUEL_USED_WHEN_ON = 1000.0f;				//in kg/s
	
	
	public Vector3 initPosition = new Vector3(0,0,0);
	public Vector3 velocity = new Vector3(0,0,0);

	// Use this for initialization
	void Start () 
	{
		initPosition = transform.position;
		velocity = Vector3.zero;
	}
	
	bool RocketIsOn()
	{
		return kgOfFuel > 0.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (theGameManager == null)
		{
			return;
		}
		
		float height = transform.position.y - initPosition.y;
		float forceGravityEndOfLastFrame = theGameManager.ForceOfGravity(theGameManager.tfRadius + height);		//Newtons
		float massEndOfLastFrame = CurrentRocketMass();
		
		if ((theGameManager.theGameState == GameManager.GameStates.GS_GAME_IN_PROGRESS) ||
			(theGameManager.theGameState == GameManager.GameStates.GS_ENDGAME_DEFEAT))
		{
			kgOfFuel = Mathf.Max(0.0f, kgOfFuel - FUEL_USED_WHEN_ON * theGameManager.BoostedDeltaTime());
			
			//calculate my acceleration & velcoity based on the average forces
			//HINT: Use theGameManager.BoostedDeltaTime() instead of Time.DeltaTime to take advantage of hitting Shift to speed up the game time
			/* YOUR CODE HERE */
			float thrust = 0;
			if(kgOfFuel > 0)
			{
				thrust = ROCKET_THRUST_WHEN_ON;
			}
			float acceleration = theGameManager.BoostedDeltaTime() * ((thrust - forceGravityEndOfLastFrame) / massEndOfLastFrame);
			velocity.y += acceleration;
			
			//move
			Vector3 moveDelta = velocity * theGameManager.BoostedDeltaTime();
			transform.position += moveDelta;
			
			//Check for victory condition
			height = transform.position.y - initPosition.y;			//I moved, so look at my new height
			float distFromTFCore = theGameManager.tfRadius + height;
			if (velocity.y > theGameManager.CalculateEscapeVelocityFromTerraFirma(distFromTFCore))
			{
				theGameManager.RecordVictory(kgOfFuel);
			}
			//Check for fail
			else if (theGameManager.theGameState == GameManager.GameStates.GS_GAME_IN_PROGRESS && !RocketIsOn())
			{
				theGameManager.theGameState = GameManager.GameStates.GS_ENDGAME_DEFEAT;
				if (transform.Find("Nozzle") != null)
				{
					transform.Find("Nozzle").GetComponent<ParticleEmitter>().emit = false;
				}
			}
			//Check for epic fail
			if (transform.position.y < initPosition.y)
			{
				theGameManager.Restart();
			}
		}
		
		Vector3 newCamPosition = transform.position + new Vector3(0,0,-300.0f);
		Camera.main.transform.position = newCamPosition;	
	}
	
	public void Restart()
	{
		transform.position = initPosition;
		velocity = Vector3.zero;
		if (transform.Find("Nozzle") != null)
		{
			transform.Find("Nozzle").GetComponent<ParticleEmitter>().emit = true;
		}
	}
	
	public float CurrentRocketMass()
	{
		return MASS_OF_EMPTY_ROCKET_IN_KG + kgOfFuel;
	}
}
