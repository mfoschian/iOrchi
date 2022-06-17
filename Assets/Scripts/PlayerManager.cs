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

	public Color[] playersColor = { Color.green, Color.blue, Color.red, Color.yellow, Color.white, Color.black };


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

	private Color getPlayerColor( int playerPos ) {
		Color c;

		if( playerPos < playersColor.Length )
			c = playersColor[playerPos];
		else
			// random color
			c = new Color( Random.Range(0.0f,1.0f), Random.Range(0.0f,1.0f), Random.Range(0.0f,1.0f) );

		return c;
	}

	private void _addPlayer( PlayerController p ) {
		if( players.Count < maxPlayers ) {
			Transform t = getNextPlayerSpawnPosition();
			p.transform.position = t.position;
			p.transform.rotation = t.rotation;

			Color c = getPlayerColor( players.Count );
			p.setColor( c );

			players.Add( p );

			if( players.Count == playersBeforeStart && listener != null )
				listener.onPlayersAvailable();

		}
	}

}
