using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{

	public float walkingSpeed = 7.5f;
	public float runningSpeed = 11.5f;
	public float jumpSpeed = 8.0f;
	public float gravity = 20.0f;
	public Camera playerCamera;
	public float lookSpeed = 2.0f;
	public float lookXLimit = 45.0f;
	public bool canJump = true;

	CharacterController characterController;
	Vector3 moveDirection = Vector3.zero;
	float rotationX = 0;

	[HideInInspector]
	public bool canMove = true;






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


	public override void OnNetworkSpawn() {
		Debug.Log( "Player spawned");
		PlayerManager.addPlayer( this );
	}

	void Start() {

		if( cameras.Length == 0 ) {
			// Add embedded camera
			cameras = GetComponentsInChildren<Camera>(true);
			if( cameras.Length > 0 ) {
				Camera cam = cameras[0];
				cam.gameObject.SetActive(true);
			}
		}

		characterController = GetComponent<CharacterController>();

		// Lock cursor
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

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

        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (canJump && Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
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
