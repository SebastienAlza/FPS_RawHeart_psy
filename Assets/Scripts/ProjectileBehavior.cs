using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
	public float lifetime = 5f; // Dur�e de vie maximale du projectile
	public float proximityRadius = 1f; // Rayon de d�tection autour du joueur
	public int damage = 10; // D�g�ts inflig�s au joueur

	private Transform player; // R�f�rence au joueur
	private Vector3 previousPosition; // Position pr�c�dente du projectile

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
			Debug.LogWarning("[ProjectileBehavior] Aucun joueur trouv� avec le tag 'Player'.");
		}

		// D�truire le projectile apr�s un certain temps
		Destroy(gameObject, lifetime);

		// Initialiser la position pr�c�dente
		previousPosition = transform.position;
	}

	private void FixedUpdate()
	{
		if (player == null) return;

		// V�rifier la proximit� avec un Raycast
		CheckProximityWithRaycast();

		// Mettre � jour la position pr�c�dente
		previousPosition = transform.position;
	}

	private void CheckProximityWithRaycast()
	{
		// Calculer la direction et la distance parcourue par le projectile
		Vector3 direction = transform.position - previousPosition;
		float distance = direction.magnitude;

		// Lancer un Raycast pour v�rifier la proximit� avec le joueur
		if (Physics.Raycast(previousPosition, direction.normalized, out RaycastHit hit, distance + proximityRadius))
		{
			if (hit.collider.CompareTag("Player"))
			{
				// Infliger des d�g�ts au joueur
				/*
				PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
				if (playerHealth != null)
				{
					playerHealth.TakeDamage(damage);
				}*/

				// D�truire le projectile
				Destroy(gameObject);
			}
		}
	}
}
