using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameLogic : NetworkBehaviour, EnemyBrain.IListener, PlayerManager.IListener, MainMenu.Listener
{
	public MainMenu Menu;
	public NetworkManager Net;

	private HordeGenerator m_hordeGenerator;
	private EnemyBrain m_brain;
	private PlayerManager m_playerManager;
	private IHUD m_hud;

	public Camera lobbyCamera = null;

	// private int status = 0;

	public interface IHUD {
		void setEnemiesLeft(int n);
		void setEnemiesInCastle(int n);
	}

	public GameObject hud;
	public int enemiesToLose = 7;

	void startRound() {

		if( !NetworkManager.Singleton.IsServer ) 
			return;

		const float start_delay = 5f;
		Debug.Log("Starting Game Round in " + start_delay);
		Invoke("createHorde", start_delay);
		Invoke("startHorde", start_delay + 1);
	}

    // Start is called before the first frame update
    void Start() {
		if( Menu != null ) {
			// Menu.SetActive(true);
			Menu.setListener(this);
			if( hud != null )
				hud.SetActive(false);
		}
	}
    void StartGame()
    {
		Debug.Log("Starting Game");

		if( Menu != null ) {
			Menu.gameObject.SetActive(false);
		}
		if( hud != null ) {
			m_hud = hud.GetComponent<IHUD>();
			hud.SetActive(true);
		}
		m_playerManager = GetComponent<PlayerManager>();
		if( m_playerManager != null )
			m_playerManager.listener = this;

		m_hordeGenerator = GetComponent<HordeGenerator>();
		m_brain = GetComponent<EnemyBrain>();
		if( m_brain != null )
			m_brain.listener = this;

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

	private void disableLobbyCam() {
		if( lobbyCamera != null ) {
			lobbyCamera.gameObject.SetActive(false);
		}
	}

	public void onPlayersAvailable() {
		startRound();
	}

	public void onStart(string mode) {
		if( mode == "Server") {
			// TODO: start network server
		}
		else if( mode == "Host") {
			StartGame();

			if( Net != null ) {
				bool ok = Net.StartHost();
				if( ok )
					disableLobbyCam();
			}
		}
		else if( mode == "Client") {
			StartGame();

			if( Net != null ) {
				bool ok = Net.StartClient();
				if( ok )
					disableLobbyCam();
			}
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
