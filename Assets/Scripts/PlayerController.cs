using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public GameObject arrowPrefab;
	public Transform arrowStartPosition;
	[Range(0,1)] public float arrowPower = 0.0f;
	public float arrowLoadTime = 2.0f;
	public float rechargeTime = 1.0f;

	public Camera[] cameras;
	private int m_activeCamera = 0;

	public string playerName = "Player 1";

	public enum ArrowStatus {
		noArrow,
		armed,
		starting,
		charging,
		landed
	}

	iOrchi.Arrow arrow;
	ArrowStatus arrowStatus = ArrowStatus.noArrow;
	float bowExtension = 0.5f;
	float timeT = 0.0f;

	public void enemyHitted(float distance, EnemyNavAgent enemy) {
		Debug.Log( "Enemy hitted by " + playerName + " from " + distance + " meters");
	}


	void Start() {
		// FPSController c = GetComponent<FPSController>();
		// if( c != null )
		// 	m_Camera = c.playerCamera;
		// m_cameras = GetComponentsInChildren<Camera>(true);
		// for( int i=0; i<m_cameras.Length; i++ ) {
		// 	Camera c = m_cameras[i];
		// 	if( c.gameObject.isActiveAndEnabled )
		// 		m_activeCamera = i;
		// }
	}

	void nextCamera() {
		if( cameras.Length < 2 )
			return;

		int ix = m_activeCamera + 1;
		if( ix >= cameras.Length ) ix = 0;

		cameras[m_activeCamera].gameObject.SetActive(false);
		cameras[ix].gameObject.SetActive(true);
		m_activeCamera = ix;
	}

	void armArrow() {
		GameObject projectile = Instantiate(arrowPrefab, arrowStartPosition.position, arrowStartPosition.rotation);
		bool ok = projectile.TryGetComponent(out arrow);
		if( ok ) {
			arrowPower = 0.0f;
			timeT = 0.0f;
			arrowStatus = ArrowStatus.armed;
			projectile.transform.parent = arrowStartPosition;
		}
	}

	void fireArrow() {
		arrowStatus = ArrowStatus.charging;
		arrow.Release(arrowPower);
		Invoke("recharged", rechargeTime);
	}

	void recharged() {
		arrowStatus = ArrowStatus.noArrow;
	}

    void Update()
    {
		if( arrowStatus == ArrowStatus.noArrow ) {
			armArrow();
		}

		if( Input.GetButtonUp("Fire1") && arrowStatus == ArrowStatus.armed ) {
			arrowStatus = ArrowStatus.starting;
		}

		if( (Input.GetButton("Fire1") || Input.GetKeyUp(KeyCode.Space)) && arrowStatus == ArrowStatus.armed ) {
			float dt = Time.deltaTime;
			timeT += dt;
			float perc = timeT / arrowLoadTime;
			if( arrowPower < 1.0f ) {
				arrowPower = perc;
			}
			if( perc <= 1.0f ) {
				Vector3 pos = arrow.transform.position;
				pos -= arrow.transform.forward * (dt * bowExtension);
				// pos.z = arrowStartPosition.position.z - (perc * bowExtension);
				arrow.transform.position = pos;
			}
		}

		if( Input.GetKeyDown( KeyCode.Backspace ) ) {
			nextCamera();
		}
    }

	void AfterUpdate() {
		// if( m_Camera ) {
		// 	arrowStartPosition.rotation = m_Camera.transform.rotation;
		// }
	}

	void FixedUpdate() {
		if( arrowStatus == ArrowStatus.starting ) {
			fireArrow();
		}
		// else if( arrowStatus == ArrowStatus.flying && arrow.isLanded() )
		// 	arrowStatus = ArrowStatus.noArrow;
	}
}
