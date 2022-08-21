using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(AudioSource))]
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

	[HideInInspector]
	public bool canMove = true;

	public Camera[] cameras;
	private int m_activeCamera = 0;

	public string playerName = "Player 1";


	private CharacterController characterController;
	private Vector3 moveDirection = Vector3.zero;
	private Quaternion moveRotation = Quaternion.identity;
	private Quaternion camRotation = Quaternion.identity;
	private float rotationX = 0;
	private Color? playerColor = null;

	public GameObject weaponPrefab;
	public Transform weaponPosition;

	private AudioListener audioListener;
	private AudioSource soundSource;
	iOrchi.Weapon weapon = null;
	bool weaponReleased = false;

	private NetworkVariable<Vector3> ownerPos = new NetworkVariable<Vector3>(
		default,
		NetworkVariableBase.DefaultReadPerm, // Everyone
		NetworkVariableWritePermission.Owner
	);
	private NetworkVariable<Quaternion> ownerCamRot = new NetworkVariable<Quaternion>(
		default,
		NetworkVariableBase.DefaultReadPerm, // Everyone
		NetworkVariableWritePermission.Owner
	);
	private NetworkVariable<Quaternion> ownerRot = new NetworkVariable<Quaternion>(
		default,
		NetworkVariableBase.DefaultReadPerm, // Everyone
		NetworkVariableWritePermission.Owner
	);
	private NetworkVariable<Color> netPlayerColor = new NetworkVariable<Color>();

	private enum Status {
		idle,
		walking,
		running,
		jumping
	}

	private Status myStatus = Status.idle;
	private bool statusChanged = false;
	private NetworkVariable<Status> netStatus = new NetworkVariable<Status>(Status.idle,
		NetworkVariableBase.DefaultReadPerm, // Everyone
		NetworkVariableWritePermission.Owner
	);

	public void enemyHitted(float distance, EnemyNavAgent enemy) {
		Debug.Log( "Enemy hitted by " + playerName + " from " + distance + " meters");
	}

	public override void OnNetworkSpawn() {
		base.OnNetworkSpawn();

		characterController = GetComponent<CharacterController>();
		audioListener = gameObject.GetComponent<AudioListener>();
		soundSource = gameObject.GetComponent<AudioSource>();
	}

	public void setColor(Color c) {
		Renderer r = GetComponent<Renderer>();
		if( !r )
			return;

		Material m = r.material;
		if( m )
			m.color = c;

		playerColor = c;

		if( IsServer )
			netPlayerColor.Value = c;
	}

	public Color getColor() {
		if( playerColor == null ) {
			Renderer r = GetComponent<Renderer>();
			if( r != null && r.material != null )
				playerColor = r.material.color;
		}
		return playerColor.Value;
	}

	void Start() {

		// characterController = GetComponent<CharacterController>();
		// audioListener = gameObject.GetComponent<AudioListener>();

		// Renderer r = GetComponent<Renderer>();
		// if( r != null && r.material != null )
		// 	playerColor = r.material.color;

		if( ! IsLocalPlayer )
			return;

		if( cameras.Length == 0 ) {
			// Add embedded camera
			cameras = GetComponentsInChildren<Camera>(true);
			if( cameras.Length > 0 ) {
				Camera cam = cameras[0];
				// cam.enabled = true;
				// cam.gameObject.SetActive(true);
				// AudioListener l = cam.GetComponent<AudioListener>();
				// if( l != null )
				// 	l.SetActive(true);
			}
		}

		// // Lock cursor
		// Cursor.lockState = CursorLockMode.Locked;
		// Cursor.visible = false;

	}

	void nextCamera() {
		if( ! IsLocalPlayer )
			return;

		if( cameras.Length < 2 )
			return;

		int ix = m_activeCamera + 1;
		if( ix >= cameras.Length ) ix = 0;

		// cameras[m_activeCamera].gameObject.SetActive(false);
		// cameras[ix].gameObject.SetActive(true);
		cameras[ix].enabled = true;
		cameras[m_activeCamera].enabled = false;
		m_activeCamera = ix;
	}

	void armWeapon() {
		if( weaponPrefab != null ) {
			GameObject w = Instantiate(weaponPrefab, weaponPosition.position, weaponPosition.rotation);
			//projectile.GetComponent<NetworkObject>().Spawn();
			bool ok = w.TryGetComponent(out weapon);
			if(ok && weapon != null) {
				w.transform.parent = weaponPosition;
				weapon.Arm();
				Debug.Log("Armed weapon " + weapon.GetName() + " for " + playerName);
			}
		}
		else {
			Debug.Log("No weapon for " + playerName);
		}
	}

	void changeStatusSound() {
		// called only if status changed
		switch(myStatus) {
			case Status.idle:
				soundSource.Stop();
				break;
			case Status.jumping:
				soundSource.Stop();
				break;
			case Status.running:
				soundSource.Play();
				break;
			case Status.walking:
				soundSource.Play();
				break;
		}
	}

	void Update()
	{
		if( weapon == null && weaponPrefab != null ) {
			armWeapon();
		}

		if( playerColor == null || playerColor.Value != netPlayerColor.Value ) {
			setColor(netPlayerColor.Value);
		}

		if( IsLocalPlayer ) {
			CalculateMovement();
			MovePlayer();
			// Align other network instances
			ownerPos.Value = transform.position;
			ownerRot.Value = transform.rotation;
			if( playerCamera )
				ownerCamRot.Value = playerCamera.transform.localRotation;

			if( statusChanged ) {
				netStatus.Value = myStatus;
				changeStatusSound();
			}

		}
		else {
			// Align from owner movements
			transform.position = ownerPos.Value;
			transform.rotation = ownerRot.Value;
			if(playerCamera)
				playerCamera.transform.localRotation = ownerCamRot.Value;

			Status newStatus = netStatus.Value;
			if( newStatus != myStatus ) {
				myStatus = newStatus;
				changeStatusSound();
			}
		}

	}

	void MovePlayer()
	{
		if( characterController == null )
			return;

		characterController.Move(moveDirection * Time.deltaTime);

		transform.rotation *= moveRotation;
		if(playerCamera)
			playerCamera.transform.localRotation = camRotation;
	}

	void CalculateMovement()
	{
		if( weapon != null ) {
			if( Input.GetButtonUp("Fire1")  ) {
				weaponReleased = true;
			}

			if( Input.GetButtonUp("Fire2")  ) {
				weapon.Info();
			}

			if( (Input.GetButton("Fire1"))  ) {
				weapon.ChargeWeapon();
			}
		}

		if( Input.GetKeyDown( KeyCode.Backspace ) ) {
			nextCamera();
		}

		Status newStatus = myStatus;
		float movementDirectionY = moveDirection.y;

		if( newStatus != Status.jumping || characterController.isGrounded ) {
			// We are grounded, so recalculate move direction based on axes
			Vector3 forward = transform.TransformDirection(Vector3.forward);
			Vector3 right = transform.TransformDirection(Vector3.right);
			// Press Left Shift to run
			bool isRunning = Input.GetKey(KeyCode.LeftShift);
			newStatus = isRunning ? Status.running : Status.walking;
			float curSpeedX = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical") : 0;
			float curSpeedY = canMove ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal") : 0;
			
			moveDirection = (forward * curSpeedX) + (right * curSpeedY);

			if( curSpeedX == 0 && curSpeedY == 0)
				newStatus = Status.idle;
		}

        if (canJump && Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
			newStatus = Status.jumping;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

		statusChanged = (newStatus != myStatus);
		myStatus = newStatus;

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        // characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
			camRotation = Quaternion.Euler(rotationX, 0, 0);

            // transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
            moveRotation = Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

	void AfterUpdate() {
		// if( m_Camera ) {
		// 	arrowStartPosition.rotation = m_Camera.transform.rotation;
		// }
	}

	void FixedUpdate() {
		if( weaponReleased ) {
			weaponReleased = false;
			if( weapon != null )
				weapon.Release();
		}
	}

	[ClientRpc]
	public void setStartPositionClientRpc(Vector3 pos, Quaternion rot, ClientRpcParams clientRpcParams = default) {
		transform.position = pos;
		transform.rotation = rot;
		if( IsOwner ) {
			ownerPos.Value = pos;
			ownerRot.Value = rot;
			if(playerCamera) {
				GameLogic.turnOnCamera(playerCamera);
			}
			if(audioListener)
				GameLogic.turnOnAudioListener(audioListener);
		}
	}

}
