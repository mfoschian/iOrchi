using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavAgent : MonoBehaviour
{
	public int vitalPoints = 10;

	private EnemyBrain brain;
	private NavMeshAgent agent;
	private Animator animController;
	private Rigidbody rb;

    void Start() {
		animController = GetComponent<Animator>();
		rb = GetComponent<Rigidbody>();
		if( animController ) animController.SetInteger( "vitalPoints", vitalPoints );
		agent = GetComponent<NavMeshAgent>();
		GameObject g = GameObject.Find("EnemyBrain");
		if( g != null ) {
			brain = g.GetComponent<EnemyBrain>();
			if( brain != null && agent != null )
				brain.add(this);
		}
    }

	virtual public void setDestination( Vector3 destination ) {
		if( agent != null ) {
			SetPhysics(false);
			agent.destination = destination;
			if( animController ) animController.SetBool("walking", true);
		}
	}

	virtual public void hit( int damage ) {
		vitalPoints -= damage;
		Debug.Log( "Vital Points: " + vitalPoints);
		if( vitalPoints < 0 )
			vitalPoints = 0;
		
		if( vitalPoints == 0 ) {
			if( brain != null ) {
				brain.remove(this);
				if(agent != null && agent.enabled) {
					agent.ResetPath();
					agent.enabled = false;
				}
				SetPhysics(true);
			}
		}
		// if( animController ) animController.SetInteger("vitalPoints", vitalPoints);
	}

	private void FixedUpdate() {
		if( animController ) animController.SetInteger("vitalPoints", vitalPoints);
	}

	private void SetPhysics(bool usePhysics)
	{
		if(rb != null) {
			rb.useGravity = usePhysics;
			rb.isKinematic = !usePhysics;
		}
	}


}
