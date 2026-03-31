using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using UnityEngine.UI;

public class TestLobby : NetworkBehaviour
{
	[Header("මෙනු කොටස් (Panels)")]
	public GameObject mainMenuPanel;
	public GameObject roomPanel;

	[Header("UI ලින්ක්")]
	public TMP_InputField joinCodeInput;
	public TextMeshProUGUI codeDisplayText;
	public Button startGameButton;
	public Button readyButton;
	public TMP_InputField playerNameInput;

	// Ready වුණු ගණන හැමෝටම පේන විදිහට හදමු
	public NetworkVariable<int> playersReadyCount = new NetworkVariable<int>(0);

	async void Start()
	{
		await UnityServices.InitializeAsync();
		await AuthenticationService.Instance.SignInAnonymouslyAsync();

		roomPanel.SetActive(false);
		mainMenuPanel.SetActive(true);
	}

	public override void OnNetworkSpawn()
	{
		// Ready වුණු ගණන වෙනස් වෙන හැම වෙලාවකම මේක වැඩ කරනවා
		playersReadyCount.OnValueChanged += (oldVal, newVal) => {
			CheckIfEveryoneIsReady();
		};
	}

	public async void CreateMatch()
	{
		try
		{
			Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
			string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
			codeDisplayText.text = "Join Code: " + joinCode;

			NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
				allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
				allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData
			);

			if (NetworkManager.Singleton.StartHost())
			{
				mainMenuPanel.SetActive(false);
				roomPanel.SetActive(true);
				startGameButton.gameObject.SetActive(true);
				readyButton.gameObject.SetActive(false);
				startGameButton.interactable = false; // මුලින්ම ඕෆ් කරලා තියෙන්නේ
			}
		}
		catch (System.Exception e) { Debug.Log(e); }
	}

	public async void JoinMatch()
	{
		try
		{
			string code = joinCodeInput.text;
			JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);

			NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
				joinAllocation.RelayServer.IpV4, (ushort)joinAllocation.RelayServer.Port,
				joinAllocation.AllocationIdBytes, joinAllocation.Key,
				joinAllocation.ConnectionData, joinAllocation.HostConnectionData
			);

			if (NetworkManager.Singleton.StartClient())
			{
				mainMenuPanel.SetActive(false);
				roomPanel.SetActive(true);
				startGameButton.gameObject.SetActive(false);
				readyButton.gameObject.SetActive(true);
			}
		}
		catch (System.Exception e) { Debug.Log(e); }
	}

	public void OnReadyButtonClicked()
	{
		SavePlayerDetails();
		readyButton.interactable = false;
		SendReadyServerRpc();
	}

	[ServerRpc(RequireOwnership = false)]
	private void SendReadyServerRpc()
	{
		playersReadyCount.Value++;
	}

	// හැමෝම රෙඩි ද කියලා බලන කොටස
	private void CheckIfEveryoneIsReady()
	{
		if (!IsServer) return; // සර්වර් එක විතරයි මේක තීරණය කරන්නේ

		// ජොයින් වෙලා ඉන්න යාළුවෝ ගණන (Host නැතුව)
		int connectedClients = NetworkManager.Singleton.ConnectedClientsList.Count - 1;

		// යාළුවෝ ඉන්නවා නම් සහ ඒ හැමෝම රෙඩි නම් විතරක් Start Game බටන් එක දෙනවා
		if (connectedClients > 0 && playersReadyCount.Value >= connectedClients)
		{
			startGameButton.interactable = true;
		}
		else
		{
			startGameButton.interactable = false;
		}
	}

	public void SavePlayerDetails()
	{
		string name = (playerNameInput != null && playerNameInput.text != "") ? playerNameInput.text : "Player" + Random.Range(10, 99);
		PlayerPrefs.SetString("PlayerName", name);
	}

	public void StartGameScene()
	{
		if (IsServer && startGameButton.interactable)
		{
			SavePlayerDetails();
			NetworkManager.Singleton.SceneManager.LoadScene("GameArena", UnityEngine.SceneManagement.LoadSceneMode.Single);
		}
	}
}