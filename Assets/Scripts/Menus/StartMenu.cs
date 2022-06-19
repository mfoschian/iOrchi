using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenu : MonoBehaviour
{
	public interface Listener {
		void onStartGame();
	}

	private Listener m_listener = null;

	public void setListener(Listener l) {
		m_listener = l;
	}

	public void StartGame() {
		Debug.Log( "Starting Server" );
		if(m_listener != null)
			m_listener.onStartGame();
	}

}
