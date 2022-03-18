using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class TargetDrivenEnemyBrain : EnemyBrain {
	public Transform target;

	override public void add( EnemyNavAgent a ) {
		base.add(a);
		// agents.Add(a);
		if( target != null ) {
			setDestination( target.position );
		}
	}
}
