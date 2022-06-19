using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
	public interface Listener {
		void onStartGame();
	}

	private Listener m_listener = null;

	public Text joinCodeBox;

	public void setListener(Listener l) {
		m_listener = l;
	}

	public void StartGame() {
		Debug.Log( "Starting Server" );
		if(m_listener != null)
			m_listener.onStartGame();
	}

	public void setJoinCode( string joinCode ) {
		if( joinCodeBox != null )
			joinCodeBox.text = $"join code: {joinCode}";
	}

}
