using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectMenu : MonoBehaviour
{
	public InputField joinCodeBox;
	public Button connectButton;
	public ErrorBox errorMessageBox;

	public interface Listener {
		void onStart(string mode, string joinCode);
	}

	private Listener m_listener = null;

	public void setListener(Listener l) {
		m_listener = l;
	}

	public void showErrorMessage( string msg ) {
		if( errorMessageBox != null ) {
			errorMessageBox.setMessage( msg );
			errorMessageBox.gameObject.SetActive(true);
		}
	}

	public void StartServer() {
		Debug.Log( "Starting Server" );
		if(m_listener != null)
			m_listener.onStart("Server", null);

	}

	public void StartLocalHost() {
		Debug.Log( "Starting Local Host" );
		if(m_listener != null)
			m_listener.onStart("LocalHost", null);
	}

	public void StartHost() {
		Debug.Log( "Starting Host" );
		if(m_listener != null)
			m_listener.onStart("Host", null);
	}

	public void StartClient() {
		Debug.Log( "Starting Client" );		
		if(m_listener != null && joinCodeBox != null && isValidCode(joinCodeBox.text)) {
			string joinCode = joinCodeBox.text;
			m_listener.onStart("Client", joinCode );
		}
	}

	public void ExitGame() {
		Debug.Log( "Quitting Game, Bye !" );
		Application.Quit();
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

	private bool isValidCode(string s) {
		return !string.IsNullOrEmpty(joinCodeBox.text);
	}

    // Update is called once per frame
    void Update()
    {
        if( joinCodeBox != null && connectButton != null ) {
			connectButton.enabled = isValidCode(joinCodeBox.text);
		}
    }
}
