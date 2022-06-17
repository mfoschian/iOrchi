using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace iOrchi {
	public class Arrow : NetworkBehaviour
	{
	    public float speed = 1000f;
	    public Transform tip;
		public int damage = 10;
		public GameObject clientArrowPrefab;

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

		private float power = 0;

		public void setPower(float p) {
			power = p;
		}

		public void setColor( Color c ) {
			if( !trailRenderer ) return;
			// Gradient g = trailRenderer.colorGradient;
			// g.colorKeys[0].color = c;
			trailRenderer.startColor = c;
		}

		void Start() {

		}

		public override void OnNetworkSpawn() {
			if( IsServer ) {
				Debug.Log($"Starting arrow fly with power: {power}");
				StartMotion(power);
			}
		}

		void Update() {
		    
		}

	    private void FixedUpdate()
	    {
	        if (IsServer && inAir)
	        {
	            CheckCollision();
				lastPosition = tip.position;
	        }
	    }

		private string getObjectPathName(Transform o) {
			string name = o.name;
			Transform parent = o.parent;
			if( parent != null ) {
				name = getObjectPathName(parent) + '/' + name;
			}
			return name;
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
						string _name = getObjectPathName(hitted.transform);
						Debug.Log("Hitted " + _name);
						Vector3 relPos = transform.position - hitted.transform.position;

						setParentClientRpc(_name, relPos);
						
						attachTo(hitted.transform, transform.position);
						StopMotion();

						Destroy(gameObject);
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

		private void attachTo(Transform hitted, Vector3 position) {
			if(clientArrowPrefab != null) {
				GameObject clientArrow = Instantiate(clientArrowPrefab, position, transform.rotation);
				clientArrow.transform.parent = hitted;
			}
		}

		[ClientRpc]
		public void setParentClientRpc(string objectPath, Vector3 relPos) {
			GameObject hitted = GameObject.Find(objectPath);
			if( hitted != null ) {
				Vector3 pos = hitted.transform.position + relPos;
				attachTo(hitted.transform, pos);
				
			}
			else {
				Debug.Log( "Object " + objectPath + " not found");
			}
		}

	    private void StopMotion() {
			inAir = false;
			landed = true;
			SetPhysics(false);

			ArrowParticles(false);
	        //ArrowSounds(hitClip, 1.5f, 2, .8f, -2);
	    }

		private void StartMotion(float value, PlayerController p = null ) {
			inAir = true;
			// transform.parent = null;
			startPoint = transform.position;
			owner = p;

			rb = GetComponent<Rigidbody>();

			SetPhysics(true);
			ApplyImpulse(value);
			StartCoroutine(RotateWithVelocity());

			lastPosition = tip.position;

			ArrowParticles(true);
			//ArrowSounds(launchClip, 4.2f + (.6f*value), 4.4f + (.6f*value),Mathf.Max(.7f,value), -1);
		}

	    public void Release(float value, PlayerController p = null)
	    {
			/*
			if( NetworkManager.Singleton.IsServer ) {			
				NetworkObject neto = gameObject.GetComponent<NetworkObject>();
				if( neto != null )
					neto.Spawn();

				StartMotion(value, p);
			}
			else {
				SetPhysics(false);
				releaseArrowServerRpc(value);
			}
			*/
	    }

	    private void SetPhysics(bool usePhysics)
	    {
			if(rb != null) {
				rb.useGravity = usePhysics;
				rb.isKinematic = !usePhysics;
			}
	    }

	    private void ApplyImpulse(float power) {
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
