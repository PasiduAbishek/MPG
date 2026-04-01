using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthScript : NetworkBehaviour
{
	public NetworkVariable<int> health = new NetworkVariable<int>(100);

	[Header("Blood Slider")]
	public Slider healthSlider;
	public GameObject respawnPanel; // New Respawn panel

	private PlayerController playerController;
	[Header("Escape මෙනුව")]
	public GameObject escapeMenuPanel;

	[Header("Connection Lost මෙනුව")]
	public GameObject connectionLostPanel;

	public override void OnNetworkSpawn()
	{
		// ප්ලේයර් ස්පෝන් වෙද්දී සර්වර් එකේ කනෙක්ෂන් එක ගැන අවධානයෙන් ඉන්න කියනවා
		if (IsOwner)
		{
			NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
		}
	}

	public override void OnNetworkDespawn()
	{
		// ප්ලේයර්ව මැකෙද්දී ඒක අයින් කරනවා (Error එන එක නවත්වන්න)
		if (NetworkManager != null && NetworkManager.Singleton != null)
		{
			NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
		}
	}

	void Start()
	{
		playerController = GetComponent<PlayerController>();

		// hide respawn menu in start game
		if (respawnPanel != null) respawnPanel.SetActive(false);
		if (escapeMenuPanel != null) escapeMenuPanel.SetActive(false);
		if (connectionLostPanel != null) connectionLostPanel.SetActive(false);
	}

	void Update()
	{
		if (healthSlider != null) healthSlider.value = health.Value;
		// ESC එබුවම මෙනුව පෙන්වන/හංගන කොටස
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ToggleEscapeMenu();
		}
	}

	public void TakeDamage(int damageAmount)
	{
		if (!IsServer) return;

		if (health.Value > 0)
		{
			health.Value -= damageAmount;
			if (health.Value <= 0)
			{
				health.Value = 0;
				ShowRespawnMenuClientRpc(); // මැරුණ කෙනාට මෙනුව පෙන්වන්න කියනවා
			}
		}
	}

	// සර්වර් එකෙන් ක්ලයන්ට්ට දෙන විධානයක්
	[ClientRpc]
	private void ShowRespawnMenuClientRpc()
	{
		if (IsOwner) // මේක මගේ ප්ලේයර් නම් විතරක්
		{
			if (respawnPanel != null) respawnPanel.SetActive(true);

			// මවුස් එක නිදහස් කරනවා බොත්තම් ඔබන්න
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			// ඇවිදින එකයි වෙඩි තියන එකයි නතර කරනවා
			if (playerController != null) playerController.enabled = false;
		}
	}

	// --- රී-ස්පෝන් බොත්තම එබුවම ---
	public void RespawnPlayer()
	{
		if (respawnPanel != null) respawnPanel.SetActive(false);

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		// 1. ප්ලේයර්ව අලුත් තැනකට දානවා (Teleport)
		GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
		if (spawnPoints.Length > 0)
		{
			Vector3 newPos = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;

			// Character Controller එක ඔන් කරන් තැන වෙනස් කරන්න බැරි නිසා තත්පරේකට ඒක ඕෆ් කරනවා
			GetComponent<CharacterController>().enabled = false;
			transform.position = newPos;
			GetComponent<CharacterController>().enabled = true;
		}

		// ආයෙත් ඇවිදින්න දෙනවා
		if (playerController != null) playerController.enabled = true;

		// 2. සර්වර් එකට කියනවා මගේ ලේ ආයේ 100 කරන්න කියලා
		RequestRespawnServerRpc();
	}

	[ServerRpc(RequireOwnership = false)]
	private void RequestRespawnServerRpc()
	{
		health.Value = 100; // ලේ 100 කරනවා (හැමෝටම පේන්න)
	}

	// --- Quit බොත්තම එබුවම ---
	public void QuitGame()
	{
		// නෙට්වර්ක් එකෙන් අයින් වෙලා MainMenu එකට යනවා
		NetworkManager.Singleton.Shutdown();
		SceneManager.LoadScene("MainMenu");
	}
	void ToggleEscapeMenu()
	{
		// පැනල් එක දැනට තියෙන තත්ත්වයේ විරුද්ධ පැත්තට හරවනවා (On නම් Off, Off නම් On)
		bool isActive = !escapeMenuPanel.activeSelf;
		escapeMenuPanel.SetActive(isActive);

		if (isActive)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		else
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	private void OnClientDisconnected(ulong clientId)
	{
		if (!IsOwner) return;

		// 0 කියන්නේ සර්වර් එක. සර්වර් එක ගියොත්, හරි මාව සර්වර් එකෙන් අයින් කළොත් හරි මේක පෙන්නනවා
		if (clientId == 0 || clientId == NetworkManager.Singleton.LocalClientId)
		{
			if (connectionLostPanel != null) connectionLostPanel.SetActive(true);

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}


}

