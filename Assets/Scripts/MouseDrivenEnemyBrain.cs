// MoveToClickPoint.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class MouseDrivenEnemyBrain : EnemyBrain {

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100)) {
				foreach( EnemyNavAgent ag in agents ) {
					ag.setDestination( hit.point );
				}
            }
        }
    }
}