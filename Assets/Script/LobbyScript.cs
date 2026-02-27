using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class TestLobby : MonoBehaviour
{
	public TMP_InputField joinCodeInput;
	public TextMeshProUGUI codeDisplayText;

	async void Start()
	{
		// මේ Script එක තියෙන Object එක Scene එක මාරු වෙද්දී මකන්න එපා කියලා කියනවා
		DontDestroyOnLoad(gameObject);
		// Text එක තියෙන Canvas එකත් මකන්න එපා කියන්න ඕනේ (පල්ලෙහා පියවර බලන්න)
		if (codeDisplayText != null) DontDestroyOnLoad(codeDisplayText.canvas.gameObject);

		await UnityServices.InitializeAsync();
		await AuthenticationService.Instance.SignInAnonymouslyAsync();
		Debug.Log("Unity සර්වර් එකට ලොග් වුණා!");
	}

	public async void CreateMatch()
	{
		try
		{
			Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
			string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

			// කෝඩ් එක පෙන්වනවා
			codeDisplayText.text = "Join Code: " + joinCode;

			// --- මෙන්න මේ පේළිය අලුතෙන් එකතු කරන්න ---
			// මේකෙන් කරන්නේ "Join Code" එක නැති අනිත් හැම UI එකක්ම (Buttons, InputFields) හංගන එකයි
			HideMenuUI();

			NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
				allocation.RelayServer.IpV4,
				(ushort)allocation.RelayServer.Port,
				allocation.AllocationIdBytes,
				allocation.Key,
				allocation.ConnectionData
			);

			if (NetworkManager.Singleton.StartHost())
			{
				NetworkManager.Singleton.SceneManager.LoadScene("GameArena", UnityEngine.SceneManagement.LoadSceneMode.Single);
			}
		}
		catch (System.Exception e) { Debug.Log(e); }
	}

	// අලුත් Function එකක් ලියමු UI හංගන්න
	void HideMenuUI()
	{
		// Canvas එක ඇතුළේ තියෙන පරණ බොත්තම් සහ InputField එක හොයාගෙන හංගනවා
		joinCodeInput.gameObject.SetActive(false);

		// Create Match සහ Join Match බොත්තම් තියෙන "Panel" එකක් හෝ පේරන්ට් කෙනෙක් ඉන්නවා නම් එයාව හංගන්න
		// උදාහරණයක් ලෙස:
		// GameObject.Find("CreateMatchButton").SetActive(false);
	}

	public async void JoinMatch()
	{
		try
		{
			string code = joinCodeInput.text;
			JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);

			NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
				joinAllocation.RelayServer.IpV4,
				(ushort)joinAllocation.RelayServer.Port,
				joinAllocation.AllocationIdBytes,
				joinAllocation.Key,
				joinAllocation.ConnectionData,
				joinAllocation.HostConnectionData
			);

			NetworkManager.Singleton.StartClient();

			// Client ජොයින් වුණාම Join Code එක පෙන්වන එක නවත්වන්න පුළුවන්
			codeDisplayText.text = "";
		}
		catch (System.Exception e)
		{
			Debug.Log("Join වෙන්න බැරි වුණා: " + e);
		}
	}
}