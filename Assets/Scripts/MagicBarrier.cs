using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicBarrier : MonoBehaviour
{
	public GameObject effect;

    void Start()
    {
        InvokeRepeating("disableArrows", 2.0f, 2f);
    }

	void disableArrows() {
		Transform[] arrows = GetComponentsInChildren<Transform>();
		for( int i=0; i<arrows.Length; i++ ) {
			Transform child = arrows[i];
			GameObject arrow = child.gameObject;

			if( arrow != gameObject ) {

				GameObject boom = Instantiate(effect, arrow.transform.position, arrow.transform.rotation);

				Destroy(child.gameObject, 1f);
				Destroy(boom, 2f);
			}
		}
	}

    void Update() {
    }
}
