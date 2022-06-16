using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace iOrchi {

	public class Bow : MonoBehaviour, Weapon
	{
		public GameObject ArrowPrefab;
		public GameObject arrow;
		[Range(0,1)] public float arrowPower = 0.0f;

		iOrchi.Arrow arrowScript;
		// float timeT = 0.0f;
		private Vector3 handleRelPos = Vector3.zero;

		float bowExtension = 0.5f;
		float timeT = 0.0f;

		public float arrowLoadTime = 2.0f;
		public float rechargeTime = 1.0f;





		void Start() {
			if( arrow != null )
				handleRelPos = transform.position - arrow.transform.position;
				// arrowStartPosition = arrow.transform.TransformPoint(arrow.transform.position);
				// Debug.Log($"Bow arrow pos: {handleRelPos.x}, {handleRelPos.y}, {handleRelPos.z} ");
		}

		public void Arm() {
			// if( arrow != null )
			// 	arrowStartPosition = arrow.transform.position;
		}

		public void Info() {
			// if( arrow != null )
			// 	arrowStartPosition = arrow.transform.position;
			// Debug.Log($"Bow arrow pos: {handleRelPos.x}, {handleRelPos.y}, {handleRelPos.z} ");
		}



		public void Release() {
			if( arrow == null ) return;

			Vector3 projectilePosition = arrow.transform.position; // .TransformPoint(arrow.transform.position);
			Quaternion projectileRotation = arrow.transform.rotation;

			GameLogic.spawnProjectile("arrow", projectilePosition, projectileRotation, arrowPower);

			Invoke("recharged", rechargeTime);
		}

		void recharged() {
			arrow.SetActive( true );
			arrow.transform.position = transform.position + handleRelPos;
			timeT = 0.0f;
		}


		public void ChargeWeapon() {
			if( arrow == null )
				return;

			float dt = Time.deltaTime;
			timeT += dt;
			float perc = timeT / arrowLoadTime;
			if( arrowPower < 1.0f ) {
				arrowPower = perc;
			}
			if( perc <= 1.0f  && arrow != null ) {
				Vector3 pos = arrow.transform.position;
				pos -= arrow.transform.forward * (dt * bowExtension);
				// pos.z = arrowStartPosition.position.z - (perc * bowExtension);
				arrow.transform.position = pos;
			}			
		}

		public string GetName() {
			return "The Bow";
		}

	}

}
