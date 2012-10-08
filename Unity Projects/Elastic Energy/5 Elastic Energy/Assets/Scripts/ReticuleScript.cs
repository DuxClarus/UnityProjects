using UnityEngine;
using System.Collections;

public class ReticuleScript : MonoBehaviour 
{
	public Vector3 newPos = new Vector3(0,0,0);
	public Vector3 newOrientationInEulerAngles = new Vector3(0,0,0);

	// Use this for initialization
	void Start ()
	{
		newPos = transform.position;	
	}
	
	// Update is called once per frame
	void Update () 
	{
		Vector3 moveDelta = newPos - transform.position;
		
		transform.Translate(moveDelta);
		transform.eulerAngles = newOrientationInEulerAngles;
	}
}
