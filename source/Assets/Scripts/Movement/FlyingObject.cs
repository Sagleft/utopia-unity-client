using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingObject : MonoBehaviour {

	public float velocity = 1.0f;

	// Use this for initialization
	void Start () {
		//rotateAroundVector = new Vector3(
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.Rotate (Vector3.up * velocity);
	}
}
