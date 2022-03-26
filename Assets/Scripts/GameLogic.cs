using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour, EnemyBrain.IListener
{
	private HordeGenerator m_hordeGenerator;
	private EnemyBrain m_brain;
	private IHUD m_hud;

	// private int status = 0;

	public interface IHUD {
		void setEnemiesLeft(int n);
		void setEnemiesInCastle(int n);
	}

	public GameObject hud;
	public int enemiesToLose = 7;

    // Start is called before the first frame update
    void Start()
    {
		Debug.Log("Starting Game");

		if( hud != null ) {
			m_hud = hud.GetComponent<IHUD>();
		}
		m_hordeGenerator = GetComponent<HordeGenerator>();
		m_brain = GetComponent<EnemyBrain>();
		if( m_brain != null )
			m_brain.listener = this;

		Invoke("createHorde", 5f);
		Invoke("startHorde", 6f);


		Debug.Log("Game Started");
    }

	private void updateHUD() {
		if( m_hud == null )
			return;

		m_hud.setEnemiesLeft( m_brain.enemiesCount );
		m_hud.setEnemiesInCastle( m_brain.enemiesOnTarget );
	}

	public void createHorde() {
		if( m_brain != null && m_hordeGenerator != null ) {
			List<GameObject> enemies = m_hordeGenerator.generate();
			m_brain.addHorde( enemies );
			updateHUD();
		}
	}

	public void startHorde() {
		if( m_brain != null && m_hordeGenerator != null ) {
			m_brain.startHorde();
		}
	}


	public void onEnemyKilled(int enemiesLeft) {
		updateHUD();

		if( enemiesLeft <= 0 ) {
		 Debug.Log( "You Win" );
		}
	}

	public void onEnemyOnTarget(int enemiesOnTarget) {
		updateHUD();

		if( enemiesOnTarget >= enemiesToLose ) {
			Debug.Log( "Game Over" );
		}
	}

	public void Update() {

		// if( Input.GetKeyDown(KeyCode.Backspace) ) {
		// 	if( status == 0 ) {
		// 		status++;
		// 		createHorde();
		// 	}
		// 	else if( status == 1 ) {
		// 		status++;
		// 		startHorde();
		// 	}
		// }
	}
}
