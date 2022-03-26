// MoveToClickPoint.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyBrain : MonoBehaviour, EnemyNavAgent.IEnemyObserver {

	public interface IListener {
		void onEnemyKilled(int enemiesLeft);
		void onEnemyOnTarget(int enemiesOnTarget);
	}

	public IListener listener;

    protected List<EnemyNavAgent> agents = new List<EnemyNavAgent>();

	private int m_enemiesCount = 0;
	private int m_enemiesOnTarget = 0;

	public int enemiesCount { get { return m_enemiesCount; } }
	public int enemiesOnTarget { get { return m_enemiesOnTarget; } }

	virtual public void add(EnemyNavAgent ag) {
		if( ag == null ) return;

		m_enemiesCount++;
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
		m_enemiesOnTarget++;
		if(listener != null)
			listener.onEnemyOnTarget(m_enemiesOnTarget);
	}

	virtual public void enemyKilled( EnemyNavAgent enemy ) {
		remove(enemy);
		m_enemiesCount = agents.Count;
		if(listener != null)
			listener.onEnemyKilled(m_enemiesCount);
	}
}