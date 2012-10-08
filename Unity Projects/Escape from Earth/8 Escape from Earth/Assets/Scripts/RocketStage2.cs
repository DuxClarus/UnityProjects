using UnityEngine;
using System.Collections;

public class RocketStage2 : RocketScript
{
	public RocketScript stage1;

	// Use this for initialization
	void Start () 
	{
		CROSS_SECTIONAL_AREA = 28.2f * Mathf.PI;
		MASS_OF_EMPTY_ROCKET_IN_KG = 20000.0f;
		MAX_AMOUNT_OF_FUEL = 400000.0f;			//in kg
		ROCKET_THRUST_WHEN_ON = 3000000.0f;		//in Newtons (kg*m/s^2)
		FUEL_USED_WHEN_ON = 500.0f;				//in kg/s
		initPosition = transform.position;
		velocity = Vector3.zero;
		if (stage1 != null)
		{
			theGameManager = stage1.theGameManager;
		}
	}
	
	void IgniteStage2()
	{
		isFiring = true;
		if (transform.Find("Nozzle") != null)
		{
			transform.Find("Nozzle").GetComponent<ParticleEmitter>().emit = true;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		base.Update();
	}
}
