using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
	void Awake()
	{
		// ප්ලේයර්ව දාන තැන පෙන්වන්න පොඩි ලකුණක් (Gizmo) දාමු
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, 0.5f);
	}
}