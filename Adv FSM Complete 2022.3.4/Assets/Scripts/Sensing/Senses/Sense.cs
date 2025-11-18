using UnityEngine;
using System.Collections;

public class Sense : MonoBehaviour {
	public bool bDebug = true;
	public Entity.entityType entityName = Entity.entityType.Enemy; //this sense belongs to an Enemy by default
	public float detectionRate = 1.0f;

	protected float elapsedTime = 0.0f;

	protected virtual void Initialise() { } //for child classes to override
	protected virtual void UpdateSense() { }

	// Use this for initialization
	void Start () {
		elapsedTime = 0.0f;
		Initialise(); //call the child class Initialise function that is overridden instead of overridiing start
	}
	
	// Update is called once per frame
	void Update () {
		UpdateSense();
	}
}
