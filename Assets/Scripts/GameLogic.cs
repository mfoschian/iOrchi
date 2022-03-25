using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
	private HordeGenerator m_hordeGenerator;
	private EnemyBrain m_brain;

	private int status = 0;

    // Start is called before the first frame update
    void Start()
    {
		Debug.Log("Starting Game");

		m_hordeGenerator = GetComponent<HordeGenerator>();
		m_brain = GetComponent<EnemyBrain>();

		Debug.Log("Game Started");
    }

	public void createHorde() {
		if( m_brain != null && m_hordeGenerator != null ) {
			m_brain.addHorde( m_hordeGenerator.generate() );
		}
	}

	public void startHorde() {
		if( m_brain != null && m_hordeGenerator != null ) {
			m_brain.startHorde();
		}
	}

	public void Update() {

		if( Input.GetKeyDown(KeyCode.Backspace) ) {
			if( status == 0 ) {
				status++;
				createHorde();
			}
			else if( status == 1 ) {
				status++;
				startHorde();
			}
		}
	}
}
