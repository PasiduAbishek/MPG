using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class NetworkHandler : MonoBehaviour
{
	public GameObject playerPrefab; // මෙතනට palyer ප්‍රිෆැබ් එක දාන්න

	private void Start()
	{
		// මැප් එක ලෝඩ් වෙලා ඉවර වුණාම මේක වැඩ කරනවා
		NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoaded;
	}

	private void SceneLoaded(string sceneName, LoadSceneMode loadSceneMode, System.Collections.Generic.List<ulong> clientsCompleted, System.Collections.Generic.List<ulong> clientsTimedOut)
	{
		// සර්වර් එකේදී විතරයි ප්ලේයර්ස්ලාව ස්පෝන් කරන්නේ
		if (NetworkManager.Singleton.IsServer)
		{
			foreach (ulong clientId in clientsCompleted)
			{
				SpawnPlayer(clientId);
			}
		}
	}

	private void SpawnPlayer(ulong clientId)
	{
		// Spawn Points හොයාගන්නවා
		GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
		Vector3 spawnPos = Vector3.up * 2; // Spawn points නැත්නම් මෙතන ස්පෝන් වෙනවා

		if (spawnPoints.Length > 0)
		{
			spawnPos = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
		}

		// ප්ලේයර්ව ගේනවා (Instantiate)
		GameObject playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

		// ප්ලේයර්ව නෙට්වර්ක් එකට සම්බන්ධ කරනවා
		playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
	}
}