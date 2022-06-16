using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectMenu : MonoBehaviour
{
	public interface Listener {
		void onStart(string mode);
	}

	private Listener m_listener = null;

	public void setListener(Listener l) {
		m_listener = l;
	}

	public void StartServer() {
		Debug.Log( "Starting Server" );
		if(m_listener != null)
			m_listener.onStart("Server");

	}

	public void StartHost() {
		Debug.Log( "Starting Host" );
		if(m_listener != null)
			m_listener.onStart("Host");
	}

	public void StartClient() {
		Debug.Log( "Starting Client" );		
		if(m_listener != null)
			m_listener.onStart("Client");
	}

	public void ExitGame() {
		Debug.Log( "Quitting Game, Bye !" );
		Application.Quit();
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
