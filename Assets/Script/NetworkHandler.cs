using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NetworkHandler : MonoBehaviour
{
	public GameObject playerPrefab;


	private void Start()
	{
		// 1. සර්වර් එක Start වුණාට පස්සේ විතරක් ඉතුරු ටික කරන්න කියලා කියනවා
		NetworkManager.Singleton.OnServerStarted += OnServerStarted;

	}

	private void OnServerStarted()
	{
		// 2. සර්වර් එක ඔන් වුණාට පස්සේ, මැප් එක ලෝඩ් වෙලා ඉවර වුණාම ප්ලේයර්ව ස්පෝන් කරන්න කියනවා
		NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoaded;

		// 3. මැප් එක ලෝඩ් වුණාට පස්සේ පරක්කු වෙලා Join වෙන යාළුවන්ටත් ස්පෝන් වෙන්න මේක දානවා
		NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
	}

	private void SceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
	{
		// මැප් එක GameArena නම් විතරක් ස්පෝන් කරනවා
		if (NetworkManager.Singleton.IsServer && sceneName == "GameArena")
		{
			foreach (ulong clientId in clientsCompleted)
			{
				SpawnPlayer(clientId);
			}
		}
	}

	private void OnClientConnected(ulong clientId)
	{
		// යාළුවෙක් ජොයින් වෙද්දී දැනටමත් ඉන්නේ GameArena එකේ නම් එයාව ස්පෝන් කරනවා
		if (NetworkManager.Singleton.IsServer && SceneManager.GetActiveScene().name == "GameArena")
		{
			SpawnPlayer(clientId);
		}
	}

	private void SpawnPlayer(ulong clientId)
	{
		// ප්ලේයර් කෙනෙක් දැනටමත් ඉන්නවද බලනවා (එකම කෙනා දෙපාරක් ස්පෝන් වෙන එක නවත්වන්න)
		if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId) &&
			NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
		{
			return;
		}

		GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
		Vector3 spawnPos = Vector3.up * 2;

		if (spawnPoints.Length > 0)
		{
			spawnPos = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
		}

		// ප්ලේයර්ව ගේනවා (Instantiate)
		GameObject playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

		// ප්ලේයර්ව නෙට්වර්ක් එකට සම්බන්ධ කරනවා (මෙතනදී තමයි අර "None" කරපු එකේ අඩුව පුරවන්නේ)
		playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
	}

}