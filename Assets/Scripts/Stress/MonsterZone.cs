using UnityEngine;

public class MonsterZone : MonoBehaviour
{
	public float detectionRadius = 10f; // Rayon de la zone
	public float maxStressRate = 5f; // Augmentation maximale du stress au centre
	private Transform player; // R�f�rence au joueur

	private void Start()
	{
		player = GameObject.FindGameObjectWithTag("Player").transform;
	}
	public float GetStressIncrease()
	{
		float distance = Vector3.Distance(player.position, transform.position);

		if (distance < detectionRadius)
		{
			// Calculer un facteur bas� sur la proximit� (0 � 1)
			float proximityFactor = 1 - (distance / detectionRadius);
			return Mathf.Lerp(0, maxStressRate, proximityFactor);
		}

		return 0; // Aucun stress si le joueur est hors de la zone
	}

	// Optionnel : pour d�buguer l'affichage du rayon dans la sc�ne
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, detectionRadius);
	}
}
