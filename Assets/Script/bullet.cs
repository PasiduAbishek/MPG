using UnityEngine;
using Unity.Netcode;
using System.Collections; // Coroutine පාවිච්චි කරන්න මේක ඕනේ

public class BulletScript : NetworkBehaviour
{
	public float lifeTime = 5f;

	public override void OnNetworkSpawn()
	{
		if (IsServer)
		{
			// සර්වර් එකේදී විතරක් කාලය මනින්න පටන් ගන්නවා
			StartCoroutine(DestroyAfterTime());
		}
	}

	private IEnumerator DestroyAfterTime()
	{
		yield return new WaitForSeconds(lifeTime);
		// තත්පර 5කට පස්සේ බෝලේ තාමත් තියෙනවා නම් නෙට්වර්ක් එකෙන් අයින් කරනවා
		if (IsSpawned)
		{
			GetComponent<NetworkObject>().Despawn();
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!IsServer || !IsSpawned) return;

		if (collision.gameObject.CompareTag("Player"))
		{
			if (collision.gameObject.TryGetComponent(out HealthScript health))
			{
				health.TakeDamage(10);
			}

			// ප්ලේයර්ගේ වැදුණු ගමන් බෝලය අයින් කරනවා
			GetComponent<NetworkObject>().Despawn();
		}
	}
}