using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerManager : NetworkBehaviour
{
	public interface IListener {
		void onPlayersAvailable();
	}

	public IListener listener = null;
	private static PlayerManager m_instance = null;

	private List<PlayerController> players = new List<PlayerController>();
	public Transform[] spawnPositions;
	private int m_nextSpawnIndex = 0;
	public int playersBeforeStart = 1;
	public int maxPlayers = 4;

	public void OnEnable() {
		if( m_instance == null )
			m_instance = this;
	}
	private static PlayerManager Instance {
		get {
			return m_instance;
		}
	}

	public Transform getNextPlayerSpawnPosition() {
		Transform res = spawnPositions[m_nextSpawnIndex++];

		if( m_nextSpawnIndex >= spawnPositions.Length )
			m_nextSpawnIndex = 0;
		
		return res;
	}

    // [ServerRpc]
    // void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
    // {
    //     Position.Value = GetRandomPositionOnPlane();
    // }

	public static void addPlayer( PlayerController p ) {
		if( !NetworkManager.Singleton.IsServer )
			return;

		Instance._addPlayer( p );
	}

	private void _addPlayer( PlayerController p ) {
		if( players.Count < maxPlayers ) {
			Transform t = getNextPlayerSpawnPosition();
			p.transform.position = t.position;
			p.transform.rotation = t.rotation;

			players.Add( p );

			if( players.Count == playersBeforeStart && listener != null )
				listener.onPlayersAvailable();

		}
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
