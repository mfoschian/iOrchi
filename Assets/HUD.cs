using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour, GameLogic.IHUD
{
	public Text enemiesNumBox;


	public void setEnemiesLeft(int n) {
		if( enemiesNumBox != null )
			enemiesNumBox.text = "" + n;
	}

	public void setEnemiesInCastle(int n) {

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
