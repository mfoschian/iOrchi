using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorBox : MonoBehaviour
{
	public Text errorMessageBox;

	public void setMessage(string msg) {
		if( errorMessageBox != null ) {
			errorMessageBox.text = msg;
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
