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
	public AudioListener lobbyAudioListener = null;

	public bool debugStartHorde = true;

	public AudioSource hordeAudioSource;
	public AudioClip gameOverClip;

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
	private bool gameOverReached = false;

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

		cam.enabled = true;

		_instance.disableLobbyCam();
	}

	static public void turnOnAudioListener(AudioListener al) {
		if( _instance == null ) return;
		if( al == null ) return;

		al.enabled = true;

		_instance.disableLobbyAudioListener();
	}

	static public void turnOffCamera(Camera cam) {
		if( _instance == null ) return;
		_instance.enableLobbyCam();

		if( cam == null ) return;
		cam.enabled = false;
	}

	static public void turnOffAudioListener(AudioListener al) {
		if( _instance == null ) return;
		if( al == null ) return;

		al.enabled = false;
		
		_instance.enableLobbyAudioListener();

	}


	void startRound() {

		if( !NetworkManager.Singleton.IsServer )
			return;

		const float start_delay = 5f;
		Debug.Log("Starting Game Round in " + start_delay);
		playHordeAnnouncement(0);
		Invoke("createHorde", start_delay);
		Invoke("startHorde", start_delay + 1);
	}

	[ClientRpc]
	void playHordeAnnouncementClientRpc(int soundIndex) {
		if( !IsHost )
			playHordeAnnouncement(soundIndex);
	}

	void playHordeAnnouncement(int soundIndex) {
		bool isServer = NetworkManager.Singleton.IsServer;
		if(  isServer && !IsHost )
			// Server Only
			return;		

		if( isServer ) 
			playHordeAnnouncementClientRpc(soundIndex);

		if( hordeAudioSource != null )
			hordeAudioSource.Play();
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

	private void activateRoundMenu( string joinCode ) {
		if( StartMenu != null ) {
			StartMenu.setJoinCode( joinCode );
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

	private void nextRound() {
		if( gameOverReached ) {
			playGameOverClip();
			return;
		}
		// m_brain.clearEnemies();
		const float nextRoundDelay = 10.0f;
		Debug.Log($"Starting new round in {(int)nextRoundDelay} seconds");
		Invoke("startRound", nextRoundDelay);
	}

	private void playGameOverClip() {
		if( hordeAudioSource ) {
			hordeAudioSource.PlayOneShot(gameOverClip);
		}
	}

	public void onEnemyKilled(int enemiesLeft) {
		updateHUD();

		if( enemiesLeft <= 0 ) {
			Debug.Log( "You Win" );
		}

		if( m_brain.noMoreKillableEnemies )
			nextRound();
	}

	public void onEnemyOnTarget(int enemiesOnTarget) {
		updateHUD();

		if( enemiesOnTarget >= enemiesToLose && !gameOverReached ) {
			gameOverReached = true;
			Debug.Log( "Game Over" );
			playGameOverClip();
		}
		else {
			if( m_brain.noMoreKillableEnemies )
				nextRound();			
		}
	}

	private void disableLobbyCam() {
		if( lobbyCamera != null ) {
			lobbyCamera.enabled = false;
		}
	}

	private void disableLobbyAudioListener() {
		if( lobbyAudioListener != null ) {
			lobbyAudioListener.enabled = false;
		}
	}

	private void enableLobbyCam() {
		if( lobbyCamera != null ) {
			lobbyCamera.enabled = true;
		}
	}

	private void enableLobbyAudioListener() {
		if( lobbyAudioListener != null ) {
			lobbyAudioListener.enabled = true;
		}
	}

	public void OnNeededPlayersReached() {
		if( isHosted ) return;

		if( debugStartHorde )
			startRound();
		else
			Debug.Log("Horde disabled");
	}

	public async void onStart(string mode, string joinCode ) {
		if( mode == "Server") {
			StartGame();

			bool ok = NetworkManager.Singleton.StartServer();
		}
		else if( mode == "Host") {
			string _joinCode = null;
			if( RelayManager.Singleton.IsRelayEnabled ) {
				_joinCode = await RelayManager.Singleton.SetupRelay();
				Debug.Log($"Join code is {_joinCode}");
				ConnectMenu.showErrorMessage( RelayManager.Singleton.Error );
			}

			isHosted = true;
			StartGame();
			activateRoundMenu(_joinCode);

			bool ok = NetworkManager.Singleton.StartHost();
		}
		else if( mode == "Client") {
			if( RelayManager.Singleton.IsRelayEnabled ) {
				if( joinCode == null ) {
					Debug.Log("No join code");
					return;
				}
				bool ok = await RelayManager.Singleton.JoinRelay(joinCode);
				if( !ok ) {
					Debug.Log("Join Failed");
					ConnectMenu.showErrorMessage( RelayManager.Singleton.Error );
					return;
				}

				Debug.Log( "Joined" );
			}

			StartGame();
			activateHUD();

			NetworkManager.Singleton.StartClient();
		}
	}

	public void onStartGame() {
		deactivateRoundMenu();
		activateHUD();
		startRound();
	}

}
