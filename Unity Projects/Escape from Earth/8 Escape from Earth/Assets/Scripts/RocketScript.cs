using UnityEngine;
using System.Collections;

public class RocketScript : MonoBehaviour 
{
	public GameManager theGameManager;
	public float kgOfFuel = 0.0f;
	
	//these are no longer consts, since Stage 2 has its own values of these
	public float CROSS_SECTIONAL_AREA = 50.0f * Mathf.PI;	//in m^2
	public float MASS_OF_EMPTY_ROCKET_IN_KG = 50000.0f;
	public float MAX_AMOUNT_OF_FUEL = 750000.0f;			//in kg
	public float ROCKET_THRUST_WHEN_ON = 15000000.0f;		//in Newtons (kg*m/s^2)
	public float FUEL_USED_WHEN_ON = 1000.0f;				//in kg/s
	
	
	public Vector3 initPosition = new Vector3(0,0,0);
	public Vector3 velocity = new Vector3(0,0,0);
	public RocketStage2 stage2;
	
	public bool isFiring = false;

	// Use this for initialization
	void Start () 
	{
		initPosition = transform.position;
		velocity = Vector3.zero;
		isFiring = false;
		ROCKET_THRUST_WHEN_ON = 15000000.0f;
	}
	
	bool RocketIsOn()
	{
		return isFiring && kgOfFuel > 0.0f;
	}
	
	float ForceOfWindResistance()
	{
		const float SPACE = 100000.0f;
		const float dragCoefficient = 0.2f;							// rough approximation, dimensionless
		float height = transform.position.y - initPosition.y;		//in meters
		float rho = (height < SPACE) ? 1.2f * ((SPACE - height) / SPACE) : 0.0f;		//very simplified equation of air density in kg/m^3
		float forceOfWindResistance = 0.5f * rho * velocity.sqrMagnitude * dragCoefficient * CROSS_SECTIONAL_AREA;
		return forceOfWindResistance;
	}
	
	// Update is called once per frame
	public void Update () 
	{
		if (theGameManager == null)
		{
			return;
		}
		
		if (!isFiring)
		{
			return;
		}
		
		float height = transform.position.y - initPosition.y;
		float forceGravityEndOfLastFrame = theGameManager.ForceOfGravity(theGameManager.earthRadius + height);		//Newtons
		float massEndOfLastFrame = CurrentRocketMass();
		
		if ((theGameManager.theGameState == GameManager.GameStates.GS_GAME_IN_PROGRESS) ||
			(theGameManager.theGameState == GameManager.GameStates.GS_ENDGAME_DEFEAT))
		{
			kgOfFuel = Mathf.Max(0.0f, kgOfFuel - FUEL_USED_WHEN_ON * theGameManager.BoostedDeltaTime());
			
			//calculate my acceleration & velocity based on the average forces
			//HINT: Use theGameManager.BoostedDeltaTime() instead of Time.DeltaTime to take advantage of hitting Shift to speed up the game time
			/* YOUR CODE HERE */
			//velocity.y = 1000.0f;			//temp just to test this... REMOVE
			float newHeight = (velocity.y * theGameManager.BoostedDeltaTime()) + height;
			float newGravity = theGameManager.ForceOfGravity(theGameManager.earthRadius + newHeight);
			float thrust = 0.0f;
			if(RocketIsOn())
			{
				thrust = ROCKET_THRUST_WHEN_ON;
			}
			float sumOfForces = thrust - ((newGravity + forceGravityEndOfLastFrame + ForceOfWindResistance()) / 3);
			float newMass = CurrentRocketMass();
			float averageMass = (newMass + massEndOfLastFrame) / 2;
			float acceleration = sumOfForces / averageMass;
			velocity.y += acceleration * theGameManager.BoostedDeltaTime();
			
			//move
			Vector3 moveDelta = velocity * theGameManager.BoostedDeltaTime();
			transform.position += moveDelta;
			if (stage2 != null)
			{
				stage2.transform.position += moveDelta;
			}
			
			//Check for victory condition
			height = transform.position.y - initPosition.y;			//I moved, so look at my new height
			float distFromEarthCore = theGameManager.earthRadius + height;
			if (velocity.y > theGameManager.CalculateEscapeVelocityFromEarth(distFromEarthCore))
			{
				theGameManager.RecordVictory(kgOfFuel);
			}
			//Check for fail
			else if (theGameManager.theGameState == GameManager.GameStates.GS_GAME_IN_PROGRESS && !RocketIsOn())
			{
				//TODO: if stage2 != null, launch stage 2!
				if(stage2 != null)
				{
					isFiring = false;
					stage2.isFiring = true;
					stage2.velocity = velocity;
					if(stage2.transform.Find("Nozzle") != null)
					{
						stage2.transform.Find("Nozzle").GetComponent<ParticleEmitter>().emit = true;
					}
				}
				else
				{
					theGameManager.theGameState = GameManager.GameStates.GS_ENDGAME_DEFEAT;
				}
				if (transform.Find("Nozzle") != null)
				{
					transform.Find("Nozzle").GetComponent<ParticleEmitter>().emit = false;
				}
			}
			//Check for epic fail
			if (stage2 != null && stage2.transform.position.y < initPosition.y)
			{
				theGameManager.Restart();
			}
		}
		
		Vector3 newCamPosition = transform.position + new Vector3(0,100,-450.0f);
		Camera.main.transform.position = newCamPosition;
	}
	
	public void Restart()
	{
		transform.position = initPosition;
		velocity = Vector3.zero;
		isFiring = false;
		if (transform.Find("Nozzle") != null)
		{
			transform.Find("Nozzle").GetComponent<ParticleEmitter>().emit = true;
		}
		if (stage2 != null)
		{
			stage2.Restart();
			if (stage2.transform.Find("Nozzle") != null)
			{
				stage2.transform.Find("Nozzle").GetComponent<ParticleEmitter>().emit = false;
			}
		}
		Vector3 newCamPosition = transform.position + new Vector3(0,100,-450.0f);
		Camera.main.transform.position = newCamPosition;
	}
	
	public float CurrentRocketMass()
	{
		float massStage2 = 0.0f;
		if (stage2 != null)
		{
			massStage2 = stage2.CurrentRocketMass();
		}
		return MASS_OF_EMPTY_ROCKET_IN_KG + kgOfFuel + massStage2;
	}
}
