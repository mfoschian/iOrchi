using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class EnemyNavAgent : NetworkBehaviour
{
	public int vitalPoints = 10;
	public float targetTolerance = 7;

	public bool isOnTarget = false;

	public interface IEnemyObserver	{
		void enemyKilled(EnemyNavAgent enemy);
		void targetReached( Vector3 target, EnemyNavAgent agent );
	}

	private IEnemyObserver observer;
	private NavMeshAgent agent;
	private Animator animController;
	private Rigidbody rb;
	private bool startWalking = false;

    void Start() {
    }

	public override void OnNetworkSpawn() {
		Debug.Log( "Enemy spawned");

		animController = GetComponent<Animator>();
		rb = GetComponent<Rigidbody>();
		if( animController ) animController.SetInteger( "vitalPoints", vitalPoints );
		agent = GetComponent<NavMeshAgent>();
		
	}

	public void setObserver(IEnemyObserver obs) {
		observer = obs;
	}

	virtual public void setDestination( Vector3 destination ) {
		if( agent != null ) {
			SetPhysics(false);
			agent.destination = destination;
			agent.enabled = true;
			startWalking = true;
			// if( animController ) animController.SetBool("walking", true);
		}
	}

	private void stopMoving() {
		if(agent != null && agent.enabled) {
			agent.ResetPath();
			agent.enabled = false;
		}
	}

	virtual public void hit( int damage ) {
		vitalPoints -= damage;
		Debug.Log( "Vital Points: " + vitalPoints);
		if( vitalPoints < 0 )
			vitalPoints = 0;
		
		if( vitalPoints == 0 ) {
			if( observer != null ) {
				observer.enemyKilled(this);
				stopMoving();
				SetPhysics(true);
			}
		}
		// if( animController ) animController.SetInteger("vitalPoints", vitalPoints);
	}

	virtual public void targetReached() {
		isOnTarget = true;
		if( observer != null && agent != null ) {
			observer.targetReached( agent.destination, this );
		}
		stopMoving();
		if( animController ) {
			animController.SetBool("walking", false);
			int danceType = Random.Range(0,2);
			animController.SetInteger("danceType", danceType);
			animController.SetBool("dancing", true);
		}
	}

	private void FixedUpdate() {
		if( !IsServer ) return;

		if( animController ) animController.SetInteger("vitalPoints", vitalPoints);
		if( agent ) {
			if( agent.hasPath ) {
				if( agent.remainingDistance < targetTolerance ) {
					targetReached();
				}
				else if( startWalking == true && animController != null ) {
					startWalking = false;
					animController.SetBool("walking", true );
				}
			}
		}
	}

	private void SetPhysics(bool usePhysics)
	{
		if(rb != null) {
			rb.useGravity = usePhysics;
			rb.isKinematic = !usePhysics;
		}
	}

}
