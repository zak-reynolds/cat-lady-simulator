using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatMovement : MonoBehaviour {
    public Rigidbody rigidbody;
    float TurnTimer = 0;
    float TurnRate = 0;
    public float speed = 30;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {

        TurnTimer -= Time.deltaTime;
        if(TurnTimer < 0)
        {
            TurnTimer = Random.Range(0.5f, 3f);
            TurnRate = Random.Range(-180, 180);
        }

        TurnAtRate(TurnRate);
        rigidbody.AddForce(transform.forward * speed, ForceMode.Acceleration);
	}

    private void TurnAtRate(float rate)
    {
        Quaternion goal = new Quaternion();
        goal = transform.rotation * Quaternion.Euler(0,rate/50,0);
        rigidbody.MoveRotation(goal);
        
    }
}
