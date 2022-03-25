// MoveToClickPoint.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyBrain : MonoBehaviour, EnemyNavAgent.IEnemyObserver {

    protected List<EnemyNavAgent> agents = new List<EnemyNavAgent>();

	virtual public void add(EnemyNavAgent ag) {
		if( ag == null ) return;

		agents.Add( ag );
		ag.setObserver( this );
	}

	virtual public void addHorde( List<GameObject> enemies ) {
		foreach( GameObject enemy in enemies ) {
			EnemyNavAgent ag = enemy.GetComponent<EnemyNavAgent>();
			add( ag );
		}
	}

	virtual public void startHorde() {
	}

	public void remove(EnemyNavAgent a) {
		agents.Remove(a);
		a.setObserver(null);
	}

	virtual public void targetReached( Vector3 target, EnemyNavAgent agent ) {
		Debug.Log( "Enemy " + agent.name + " reached the target" );
	}

	virtual public void enemyKilled( EnemyNavAgent enemy ) {
		remove(enemy);
		if( agents.Count == 0 ) {
			Debug.Log( "All enemies killed" );
		}
	}
}