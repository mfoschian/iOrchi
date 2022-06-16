using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameLogic : NetworkBehaviour, EnemyBrain.IListener, PlayerManager.IListener, ConnectMenu.Listener
{
	public ConnectMenu Menu;
	private HordeGenerator m_hordeGenerator;
	private EnemyBrain m_brain;
	private PlayerManager m_playerManager;
	private IHUD m_hud;

	public Camera lobbyCamera = null;

	public bool debugStartHorde = true;

	// private int status = 0;

	public interface IHUD {
		void setEnemiesLeft(int n);
		void setEnemiesInCastle(int n);
	}

	public GameObject hud;
	public int enemiesToLose = 7;

	[System.Serializable]
	public class Spawnable {
		public string name = null;
		public GameObject prefab = null;
		public GameObject staticPrefab = null;
	}
	
	public Spawnable[] spawnables;

	private static GameLogic _instance = null;

	[ServerRpc(RequireOwnership=false)]
	private void spawnProjectileServerRpc(string name, Vector3 pos, Quaternion rot, float power) {
		_spawnProjectile(name,pos,rot,power);
	}

	private void _spawnProjectile(string name, Vector3 pos, Quaternion rot, float power) {
		if( !IsServer ) {
			spawnProjectileServerRpc(name,pos,rot,power);
			return;
		}

		Spawnable spw = null;
		for( int i=0; i<spawnables.Length; i++) {
			Spawnable s = spawnables[i];
			if( s.name == name ) {
				spw = s;
				break;
			}
		}
		if( spw == null ) {
			Debug.LogError("Spawnable not found: "+ name);
			return;
		}

		GameObject projectile = Instantiate(spw.prefab, pos, rot);
		iOrchi.Arrow arrow = projectile.GetComponent<iOrchi.Arrow>();
		if( arrow != null ) {
			arrow.setPower(power);
		}
		NetworkObject neto = projectile.GetComponent<NetworkObject>();
		if( neto != null ) {
			neto.Spawn();
		}

	}

	static public void spawnProjectile(string name, Vector3 pos, Quaternion rot, float power) {
		if( _instance == null )
			return;
		
		_instance._spawnProjectile(name,pos,rot,power);
		
	}

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

		if( _instance == null )
			_instance = this;
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
		if( debugStartHorde )
			startRound();
		else
			Debug.Log("Horde disabled");
	}

	public void onStart(string mode) {
		if( mode == "Server") {
			// TODO: start network server
		}
		else if( mode == "Host") {
			StartGame();

			bool ok = NetworkManager.Singleton.StartHost();
			if( ok )
				disableLobbyCam();
		}
		else if( mode == "Client") {
			StartGame();

			bool ok = NetworkManager.Singleton.StartClient();
			if( ok )
				disableLobbyCam();
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
