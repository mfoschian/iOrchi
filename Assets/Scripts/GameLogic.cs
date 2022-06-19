using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameLogic : NetworkBehaviour, 
			EnemyBrain.IListener,
			ConnectMenu.Listener, StartMenu.Listener
{
	public ConnectMenu ConnectMenu;
	public StartMenu StartMenu;
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

	private bool isHosted = false;

	[ServerRpc(RequireOwnership=false)]
	private void spawnProjectileServerRpc(string name, Vector3 pos, Quaternion rot, float power, ulong clientId ) {
		_spawnProjectile(name,pos,rot,power,clientId);
	}

	private PlayerController getPlayerFromClientId( ulong clientId ) {
		NetworkObject po = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
		if( !po )
			return null;

		PlayerController pc = po.gameObject.GetComponent<PlayerController>();
		return pc;
	}

	private void _spawnProjectile(string name, Vector3 pos, Quaternion rot, float power, ulong clientId = 0) {
		if( !IsServer ) {
			ulong cid = clientId == 0 ? NetworkManager.Singleton.LocalClientId : clientId;
			spawnProjectileServerRpc(name,pos,rot,power,cid);
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
			Debug.Assert(spw != null, "Spawnable not found: "+ name);
			return;
		}

		GameObject projectile = Instantiate(spw.prefab, pos, rot);
		iOrchi.Arrow arrow = projectile.GetComponent<iOrchi.Arrow>();
		if( arrow != null ) {
			arrow.setPower(power);
			PlayerController pc = getPlayerFromClientId(clientId);
			if( pc != null ) {
				Color c = pc.getColor();
				arrow.setColor(c);
			}
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

	static public void turnOnCamera(Camera cam) {
		if( _instance == null ) return;
		if( cam == null ) return;

		cam.gameObject.SetActive(true);

		_instance.disableLobbyCam();
	}

	static public void turnOffCamera(Camera cam) {
		if( _instance == null ) return;
		_instance.enableLobbyCam();

		if( cam == null ) return;
		cam.gameObject.SetActive(false);
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
		if( ConnectMenu != null ) {
			ConnectMenu.setListener(this);
			if( hud != null )
				hud.SetActive(false);
		}
		if( StartMenu != null ) {
			StartMenu.setListener(this);
		}

		if( _instance == null )
			_instance = this;

		NetworkManager.Singleton.OnClientConnectedCallback += (id) => {
			OnClientConnection(id);
		};
		NetworkManager.Singleton.OnClientDisconnectCallback += (id) => {
			OnClientDisconnection(id);
		};
	}

	private void OnClientConnection( ulong clientId ) {
		if(!NetworkManager.Singleton.IsServer) return;
		Debug.Log( $"Player connected: {clientId}");
		PlayerController p = getPlayerFromClientId(clientId);
		m_playerManager.addPlayer(p);
	}
	
	private void OnClientDisconnection( ulong clientId ) {
		if(!NetworkManager.Singleton.IsServer) return;
		Debug.Log( "Player DISconnected");
		// PlayerController p = getPlayerFromClientId(clientId);
		// m_playerManager.addPlayer(p);
	}

	private void activateHUD() {
		if( hud != null ) {
			m_hud = hud.GetComponent<IHUD>();
			hud.SetActive(true);
		}
		// Lock cursor
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void activateRoundMenu() {
		if( StartMenu != null ) {
			StartMenu.gameObject.SetActive(true);
		}
	}

	private void deactivateRoundMenu() {
		if( StartMenu != null ) {
			StartMenu.gameObject.SetActive(false);
		}
	}

    void StartGame()
    {
		Debug.Log("Starting Game");

		if( ConnectMenu != null ) {
			ConnectMenu.gameObject.SetActive(false);
		}
		m_playerManager = GetComponent<PlayerManager>();
		m_playerManager.OnNeededPlayersReached += OnNeededPlayersReached;

		m_hordeGenerator = GetComponent<HordeGenerator>();
		m_brain = GetComponent<EnemyBrain>();
		if( m_brain != null )
			m_brain.listener = this;

		Debug.Log("Game Started");
    }

	private void updateHUD() {
		_updateHUD(m_brain.enemiesCount, m_brain.enemiesOnTarget);

		if( NetworkManager.Singleton.IsServer )
			updateClientHUDsClientRpc(m_brain.enemiesCount, m_brain.enemiesOnTarget);
	}

	private void _updateHUD(int enemies, int enOnTarget) {
		if( m_hud == null )
			return;

		m_hud.setEnemiesLeft( enemies );
		m_hud.setEnemiesInCastle( enOnTarget );
	}

	[ClientRpc]
	void updateClientHUDsClientRpc(int enemies, int enOnTarget) {
		_updateHUD(enemies, enOnTarget);
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

	private void enableLobbyCam() {
		if( lobbyCamera != null ) {
			lobbyCamera.gameObject.SetActive(true);
		}
	}

	public void OnNeededPlayersReached() {
		if( isHosted ) return;

		if( debugStartHorde )
			startRound();
		else
			Debug.Log("Horde disabled");
	}

	public void onStart(string mode) {
		if( mode == "Server") {
			StartGame();

			bool ok = NetworkManager.Singleton.StartServer();
		}
		else if( mode == "Host") {
			isHosted = true;
			StartGame();
			activateRoundMenu();

			bool ok = NetworkManager.Singleton.StartHost();
		}
		else if( mode == "Client") {
			StartGame();
			activateHUD();

			bool ok = NetworkManager.Singleton.StartClient();
		}
	}

	public void onStartGame() {
		deactivateRoundMenu();
		activateHUD();
		startRound();
	}

}
