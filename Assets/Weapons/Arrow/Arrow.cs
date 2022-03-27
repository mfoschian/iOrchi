using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace iOrchi {
	public class Arrow : MonoBehaviour
	{
	    public float speed = 1000f;
	    public Transform tip;
		public int damage = 10;

		bool inAir = false;
		bool landed = false;
		public bool isLanded() { return landed; }

		Vector3 startPoint;
		PlayerController owner;

	    Vector3 lastPosition = Vector3.zero;
	    private Rigidbody rb;
	    // public Collider sphereCollider;

	    [Header("Particles")]
	    public ParticleSystem trailParticle;
	    public ParticleSystem hitParticle;
	    public TrailRenderer trailRenderer;

	    [Header("Sound")]
	    public AudioClip launchClip;
	    public AudioClip hitClip;

		void Start() {
			setup();
		}

		public void setup()
		{
		    rb = GetComponent<Rigidbody>();
			SetPhysics(false);
			ArrowParticles(false);			
		}

		void Update()
		{
		    
		}

	    private void FixedUpdate()
	    {
	        if (inAir)
	        {
	            CheckCollision();
				lastPosition = tip.position;
	        }
	    }

	    private void CheckCollision()
	    {
	        if (Physics.Linecast(lastPosition, tip.position, out RaycastHit hitInfo))
	        {
				Vector3 hitPoint = hitInfo.point;
				Rigidbody body = hitInfo.rigidbody;

				// Move the tip of the arrow to the hitpoint
				Vector3 diff = tip.position - transform.position;
				Vector3 newPos = hitPoint - diff;
				transform.position = newPos;

				GameObject hitted = null;
				bool applyForce = true;

				if( hitInfo.collider != null ) {
					hitted = hitInfo.collider.gameObject;
					if( hitted != null ) {
						EnemyNavAgent enemy = hitted.GetComponentInParent<EnemyNavAgent>();
						if( enemy != null ) {
							applyForce = false;
							enemy.hit(damage);
						}
						// Inform Player of launch result
						if( owner != null ) {
							float distance = Vector3.Distance( transform.position, startPoint );
							owner.enemyHitted(distance, enemy);
						}

						// Attach arrow to hitted body
						transform.parent = hitted.transform;
						StopMotion();

					}
				}

				if( body != null && applyForce )
	            {
					if(rb != null) rb.interpolation = RigidbodyInterpolation.None;
					// transform.position = newPos;
					transform.parent = hitInfo.transform;
					body.AddForce(hitPoint, ForceMode.Impulse);
					StopMotion();
	            }
	            // StopMotion();
	        }
	    }
	    private void StopMotion()
	    {
			inAir = false;
			landed = true;
			SetPhysics(false);

			ArrowParticles(false);
	        //ArrowSounds(hitClip, 1.5f, 2, .8f, -2);
	    }

	    public void Release(float value, PlayerController p = null)
	    {
	        inAir = true;
			transform.parent = null;
			startPoint = transform.position;
			owner = p;

	        SetPhysics(true);
	        MaskAndFire(value);
	        StartCoroutine(RotateWithVelocity());

	        lastPosition = tip.position;

	        ArrowParticles(true);
	        //ArrowSounds(launchClip, 4.2f + (.6f*value), 4.4f + (.6f*value),Mathf.Max(.7f,value), -1);
	    }

	    private void SetPhysics(bool usePhysics)
	    {
			if(rb != null) {
				rb.useGravity = usePhysics;
				rb.isKinematic = !usePhysics;
			}
	    }

	    private void MaskAndFire(float power)
	    {
	        Vector3 force = transform.forward * power * speed;
	        if(rb != null) rb.AddForce(force, ForceMode.Impulse);
	    }
	    private IEnumerator RotateWithVelocity()
	    {
	        yield return new WaitForFixedUpdate();
	        while (inAir && rb != null && rb.velocity.magnitude > 0)
	        {
	            Quaternion newRotation = Quaternion.LookRotation(rb.velocity, transform.up);
	            transform.rotation = newRotation;
	            yield return null;
	        }
	    }


	    void ArrowParticles(bool release)
	    {
	        if (release)
	        {
	            if(trailParticle != null) trailParticle.Play();
	            if(trailRenderer != null) {
					trailRenderer.emitting = true;
					trailRenderer.enabled = true;
				}
	        }
	        else
	        {
	            if(trailParticle != null) trailParticle.Stop(); 
	            if(hitParticle != null) hitParticle.Play();
	            if(trailRenderer != null) {
					trailRenderer.emitting = false;
					trailRenderer.enabled = false;
				}
	        }
	    }

/*
	    void ArrowSounds(AudioClip clip, float minPitch, float maxPitch,float volume, int id)
	    {
	        SFXPlayer.Instance.PlaySFX(clip, transform.position, new SFXPlayer.PlayParameters()
	        {
	            Pitch = Random.Range(minPitch, maxPitch),
	            Volume = volume,
	            SourceID = id
	        });
	    }
*/

/*
		private void OnDrawGizmos() {
			Gizmos.color = Color.red;
			Gizmos.DrawLine(lastPosition, tip.transform.position);
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(tip.transform.position, 0.1f);
		}
*/
	}

}
