using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{

    public float speed = 1000f;
    public Transform tip;
    public bool inAir = false;
    Vector3 lastPosition = Vector3.zero;
    private Rigidbody rb;
    public Collider sphereCollider;

    [Header("Particles")]
    public ParticleSystem trailParticle;
    public ParticleSystem hitParticle;
    public TrailRenderer trailRenderer;

    [Header("Sound")]
    public AudioClip launchClip;
    public AudioClip hitClip;

	void Start()
	{
	    rb = GetComponent<Rigidbody>();
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
            if(hitInfo.transform.TryGetComponent(out Rigidbody body))
            {
                /*
				if (body.TryGetComponent<Lantern>(out Lantern lantern))
                    lantern.TurnOn();

                if (body.TryGetComponent<Potion>(out Potion potion))
                {
                    potion.BreakPotion();
                    return;
                }
				*/
                rb.interpolation = RigidbodyInterpolation.None;
                transform.parent = hitInfo.transform;
                body.AddForce(rb.velocity, ForceMode.Impulse);
            }
            Stop();
        }
    }
    private void Stop()
    {
        inAir = false;
        SetPhysics(false);

        ArrowParticles(false);
        ArrowSounds(hitClip, 1.5f, 2, .8f, -2);
    }

    public void Release(float value)
    {
        inAir = true;
        SetPhysics(true);
        MaskAndFire(value);
        StartCoroutine(RotateWithVelocity());

        lastPosition = tip.position;

        ArrowParticles(true);
        ArrowSounds(launchClip, 4.2f + (.6f*value), 4.4f + (.6f*value),Mathf.Max(.7f,value), -1);
    }

    private void SetPhysics(bool usePhysics)
    {
        rb.useGravity = usePhysics;
        rb.isKinematic = !usePhysics;
    }

    private void MaskAndFire(float power)
    {
        // colliders[0].enabled = false;
        // interactionLayerMask = 1 << LayerMask.NameToLayer("Ignore");
        Vector3 force = transform.forward * power * speed;
        rb.AddForce(force, ForceMode.Impulse);
    }
    private IEnumerator RotateWithVelocity()
    {
        yield return new WaitForFixedUpdate();
        while (inAir)
        {
            Quaternion newRotation = Quaternion.LookRotation(rb.velocity, transform.up);
            transform.rotation = newRotation;
            yield return null;
        }
    }


    void ArrowParticles(bool release)
    {/*
        if (release)
        {
            trailParticle.Play();
            trailRenderer.emitting = true;
        }
        else
        {
            trailParticle.Stop(); 
            hitParticle.Play();
            trailRenderer.emitting = false;
        }*/
    }

    void ArrowSounds(AudioClip clip, float minPitch, float maxPitch,float volume, int id)
    {/*
        SFXPlayer.Instance.PlaySFX(clip, transform.position, new SFXPlayer.PlayParameters()
        {
            Pitch = Random.Range(minPitch, maxPitch),
            Volume = volume,
            SourceID = id
        });*/
    }

}
