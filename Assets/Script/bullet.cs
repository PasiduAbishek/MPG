using UnityEngine;
using Unity.Netcode;

public class BulletScript : NetworkBehaviour
{
	public float lifeTime = 5f; // කිසිම දෙයක නොවැදුණොත් තත්පර 5කින් විනාශ වේ

	void Start()
	{
		if (IsServer)
		{
			// සර්වර් එකේදී විතරක් කාලය අවසන් වූ පසු විනාශ කරන්න
			Destroy(gameObject, lifeTime);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!IsServer || !IsSpawned) return;

		// 1. ප්ලේයර් කෙනෙක්ගේ වැදුණොත් විතරක් ඉක්මනින් විනාශ කරනවා
		if (collision.gameObject.CompareTag("Player"))
		{
			if (collision.gameObject.TryGetComponent(out HealthScript health))
			{
				health.TakeDamage(10); // ඩැමේජ් එක 10ක් දෙනවා
			}

			// ප්ලේයර්ගේ වැදුණු ගමන් බෝලය අයින් කරනවා
			NetworkObject.Despawn();
		}

		// 2. බිම (Floor) වැදුණොත් බෝලය එතනම නතර වෙලා තියෙන්න ඕනේ නම්, 
		// Despawn() එක මෙතන ලියන්න එපා. 
		// එතකොට උඩ Start එකේ තියෙන කාලය ඉවර වුණාම තමයි ඒක මැකෙන්නේ.
	}
}