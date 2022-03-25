using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class TargetDrivenEnemyBrain : EnemyBrain {
	public Transform target;
	public float safeZoneRadius = 7f;
	public int enemiesToLose = 5;

	override public void add( EnemyNavAgent a ) {
		base.add(a);
		if( safeZoneRadius > 0 )
			a.targetTolerance = safeZoneRadius;

		// if( target != null ) {
		// 	setDestination( target.position );
		// }
	}

	override public void startHorde() {
		foreach( EnemyNavAgent ag in agents ) {
			ag.setDestination( target.position );
		}
	}

	override public void targetReached( Vector3 target, EnemyNavAgent agent ) {
		Debug.Log( "Enemy " + agent.name + " reached the target" );
		if( enemiesToLose > 0 ) {
			enemiesToLose--;
			if( enemiesToLose == 0 ) {
				// Last enemy entered: you lose
				Debug.Log( "You lose" );
			}
		}
	}
}
