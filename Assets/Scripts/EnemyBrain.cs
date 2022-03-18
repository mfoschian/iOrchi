// MoveToClickPoint.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyBrain : MonoBehaviour {

    protected List<EnemyNavAgent> agents = new List<EnemyNavAgent>();
    
	virtual public void add( EnemyNavAgent a ) {
		agents.Add(a);
	}

	public void setDestination(Vector3 destination) {
		foreach (EnemyNavAgent a in agents) {
			a.setDestination(destination);
		}
    }

	public void remove(EnemyNavAgent a) {
		agents.Remove(a);
	}

}