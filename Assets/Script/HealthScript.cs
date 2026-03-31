using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI; // UI පාවිච්චි කරන්න මේක ඕනේ

public class HealthScript : NetworkBehaviour
{
	// ලේ ප්‍රමාණය NetworkVariable එකක් විදිහට ගන්නවා (අනිත් අයටත් පේන්න)
	public NetworkVariable<int> health = new NetworkVariable<int>(100);

	[Header("ලේ පටිය (Health Bar UI)")]
	public Slider healthSlider; // <--- මේක තමයි අලුතෙන් දැම්මේ

	void Update()
	{
		// UI Slider එක තියෙනවා නම්, ඒකේ අගය ප්ලේයර්ගේ HP එකට සමාන කරනවා
		if (healthSlider != null)
		{
			healthSlider.value = health.Value;
		}
	}

	public void TakeDamage(int damageAmount)
	{
		if (!IsServer) return; // ඩැමේජ් එක ගණන් හදන්නේ සර්වර් එක විතරයි
		health.Value -= damageAmount;

		if (health.Value <= 0)
		{
			Debug.Log("ප්ලේයර් අවුට්!");
			// මෙතනට ප්ලේයර්ව ආයේ ස්පෝන් කරන කෝඩ් එක පස්සේ දාමු
		}
	}
}