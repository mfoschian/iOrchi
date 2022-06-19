using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class PlayerManager : NetworkBehaviour
{
	private List<PlayerController> players = new List<PlayerController>();
	public Transform[] spawnPositions;
	private int m_nextSpawnIndex = 0;
	public int playersBeforeStart = 1;
	public int maxPlayers = 4;

	public delegate void OnNeededPlayersReachedDelegate();
	static private void _OnNeededPlayersReached() {
		Debug.Log("Needed players reached: ");
	}
	public OnNeededPlayersReachedDelegate OnNeededPlayersReached = new OnNeededPlayersReachedDelegate(_OnNeededPlayersReached);

	public Color[] playersColor = { Color.green, Color.red, Color.blue, Color.yellow, Color.white, Color.black };

	
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

	private Color getPlayerColor( int playerPos ) {
		Color c;

		if( playerPos < playersColor.Length )
			c = playersColor[playerPos];
		else
			// random color
			c = new Color( Random.Range(0.0f,1.0f), Random.Range(0.0f,1.0f), Random.Range(0.0f,1.0f) );

		return c;
	}

	public void addPlayer( PlayerController p ) {
		if( players.Count < maxPlayers ) {
			Transform t = getNextPlayerSpawnPosition();
			Color c = getPlayerColor( players.Count );

			// Server authoritative changes
			p.setColor( c );

			// Client authoritative changes
			NetworkObject nob = p.gameObject.GetComponent<NetworkObject>();
			if( nob ) {
				Debug.Log($"Server send start position to {nob.OwnerClientId}");
				ClientRpcParams sendOnlyToOwner = new ClientRpcParams() {
					Send = new ClientRpcSendParams() { TargetClientIds = new []{nob.OwnerClientId} }
				};
				p.setStartPositionClientRpc(t.position, t.rotation, sendOnlyToOwner );
			}

			players.Add( p );

			Debug.Log($"Players {players.Count} / {playersBeforeStart}");
			if( players.Count >= playersBeforeStart )
				OnNeededPlayersReached();
		}
	}

}
