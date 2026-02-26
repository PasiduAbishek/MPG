using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
	[Header("පාලනය (Controls)")]
	public float moveSpeed = 5f;
	public float mouseSensitivity = 2f;
	public float jumpHeight = 1.5f;
	public float gravity = -15f;

	[Header("ලින්ක් (Setup)")]
	public GameObject bulletPrefab;
	public Transform shootPoint;
	public Transform playerCamera;

	private CharacterController controller;
	private Vector3 velocity;
	private float verticalRotation = 0f;
	private bool isGrounded;
	public float fireRate = 2f;
	private float nextfireTime = 0f;


	public override void OnNetworkSpawn()
	{
		controller = GetComponent<CharacterController>();

		if (IsOwner)
		{
			Cursor.lockState = CursorLockMode.Locked;

			if (playerCamera != null) playerCamera.GetComponent<Camera>().enabled = true;
			GetComponentInChildren<AudioListener>().enabled = true;

			// ප්ලේයර් වැටෙන එක නතර කරන්න මෙන්න මේක පටන් ගන්නවා
			StartCoroutine(EnableControllerAfterDelay());
		}
		else
		{
			if (playerCamera != null) playerCamera.GetComponent<Camera>().enabled = false;
			GetComponentInChildren<AudioListener>().enabled = false;
		}
	}

	// මල්ලි, මෙන්න මේ කොටස මම එකකට හැදුවා. තත්පර 0.5ක් ඇති මැප් එක ලෝඩ් වෙන්න.
	private System.Collections.IEnumerator EnableControllerAfterDelay()
	{
		controller.enabled = false;
		yield return new WaitForSeconds(0.5f);

		GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
		if (spawnPoints.Length > 0)
		{
			transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
		}

		controller.enabled = true;
	}

	void Update()
	{
		if (!IsOwner) return;

		// Controller එක Off වෙලා තියෙන වෙලාවට Update එක වැඩ කරන්නේ නැහැ
		if (controller == null || !controller.enabled) return;

		isGrounded = controller.isGrounded;
		if (isGrounded && velocity.y < 0)
		{
			velocity.y = -2f;
		}

		// 1. හැරවීම
		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

		transform.Rotate(Vector3.up * mouseX);
		verticalRotation -= mouseY;
		verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
		playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

		// 2. ඇවිදීම
		float x = Input.GetAxis("Horizontal");
		float z = Input.GetAxis("Vertical");

		Vector3 move = transform.right * x + transform.forward * z;
		controller.Move(move * moveSpeed * Time.deltaTime);

		// 3. උඩ පැනීම
		if (Input.GetButtonDown("Jump") && isGrounded)
		{
			velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
		}

		// 4. ගුරුත්වාකර්ෂණය
		velocity.y += gravity * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);

		// 5. වෙඩි තැබීම
		if (Input.GetButton("Fire1")&&Time.time>=nextfireTime)
		{
			if (bulletPrefab != null && shootPoint != null)
			{
				ShootServerRpc();
			}
		}
	}

	[ServerRpc]
	void ShootServerRpc()
	{
		GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
		bullet.GetComponent<NetworkObject>().Spawn();

		// අලුත් Unity version වල Rigidbody වලට linearVelocity තමයි පාවිච්චි කරන්නේ
		if (bullet.TryGetComponent<Rigidbody>(out Rigidbody rb))
		{
			rb.linearVelocity = shootPoint.forward * 50f;
		}
	}
}