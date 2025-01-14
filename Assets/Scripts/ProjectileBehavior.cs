using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
	public float lifetime = 5f; // Durée de vie maximale du projectile
	public float proximityRadius = 1f; // Rayon de détection autour du joueur
	public int damage = 10; // Dégâts infligés au joueur

	private Transform player; // Référence au joueur
	private Vector3 previousPosition; // Position précédente du projectile

	private void Start()
	{
		// Trouver le joueur par son tag
		GameObject playerObject = GameObject.FindWithTag("Player");
		if (playerObject != null)
		{
			player = playerObject.transform;
		}
		else
		{
			Debug.LogWarning("[ProjectileBehavior] Aucun joueur trouvé avec le tag 'Player'.");
		}

		// Détruire le projectile après un certain temps
		Destroy(gameObject, lifetime);

		// Initialiser la position précédente
		previousPosition = transform.position;
	}

	private void FixedUpdate()
	{
		if (player == null) return;

		// Vérifier la proximité avec un Raycast
		CheckProximityWithRaycast();

		// Mettre à jour la position précédente
		previousPosition = transform.position;
	}

	private void CheckProximityWithRaycast()
	{
		// Calculer la direction et la distance parcourue par le projectile
		Vector3 direction = transform.position - previousPosition;
		float distance = direction.magnitude;

		// Lancer un Raycast pour vérifier la proximité avec le joueur
		if (Physics.Raycast(previousPosition, direction.normalized, out RaycastHit hit, distance + proximityRadius))
		{
			if (hit.collider.CompareTag("Player"))
			{
				// Infliger des dégâts au joueur
				/*
				PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
				if (playerHealth != null)
				{
					playerHealth.TakeDamage(damage);
				}*/

				// Détruire le projectile
				Destroy(gameObject);
			}
		}
	}
}
