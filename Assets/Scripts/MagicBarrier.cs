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
		Rigidbody[] arrows = GetComponentsInChildren<Rigidbody>();
		for( int i=0; i<arrows.Length; i++ ) {
			Rigidbody rb = arrows[i];
			// rb.useGravity = true;
			// rb.isKinematic = false;
			GameObject arrow = rb.gameObject;

			GameObject boom = Instantiate(effect, arrow.transform.position, arrow.transform.rotation);

			Destroy(rb.gameObject, 1f);
			Destroy(boom, 2f);
		}
	}

    void Update() {
    }
}
