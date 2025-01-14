using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
	public bool activeShooting = false;
	public GameObject projectilePrefab; // Le prefab du projectile
	public Transform firePoint; // Le point de tir (canon de l'ennemi)
	public float projectileSpeed = 10f; // Vitesse du projectile

	private Transform player; // Le joueur (ou la cible)
	public float detectionRadius = 15f; // Rayon de détection
	public float fieldOfViewAngle = 90f; // Angle de vision (en degrés)
	public float shootCooldown = 2f; // Temps entre deux tirs

	private float lastShootTime; // Temps du dernier tir

	private void Start()
	{
		// Trouver le joueur automatiquement par son tag
		GameObject playerObject = GameObject.FindWithTag("Player");
		if (playerObject != null)
		{
			player = playerObject.transform;
		}
		else
		{
			Debug.LogWarning("[EnemyShooting] Aucun joueur trouvé avec le tag 'Player'.");
		}
	}

	void Update()
	{
		if (IsPlayerInSight() && activeShooting)
		{
			ShootAtPlayer();
		}
	}

	bool IsPlayerInSight()
	{
		if (player == null) return false;

		// Calculer la distance entre l'ennemi et le joueur
		float distanceToPlayer = Vector3.Distance(transform.position, player.position);
		if (distanceToPlayer > detectionRadius) return false;

		// Calculer l'angle entre la direction de l'ennemi et le joueur
		Vector3 directionToPlayer = (player.position - transform.position).normalized;
		float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

		// Vérifier si le joueur est dans le champ de vision
		if (angleToPlayer > fieldOfViewAngle / 2) return false;

		// Vérifier si aucun obstacle ne bloque la vue avec un Raycast
		if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, detectionRadius))
		{
			if (hit.transform == player)
			{
				return true; // Le joueur est visible
			}
		}

		return false;
	}

	void ShootAtPlayer()
	{
		// Vérifier si le cooldown est terminé
		if (Time.time - lastShootTime < shootCooldown) return;

		// Créer le projectile
		GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

		// Calculer la direction vers le joueur
		Vector3 direction = (player.position - firePoint.position).normalized;

		// Appliquer une force ou un mouvement au projectile
		Rigidbody rb = projectile.GetComponent<Rigidbody>();
		if (rb != null)
		{
			rb.linearVelocity = direction * projectileSpeed;
		}

		lastShootTime = Time.time; // Mettre à jour le temps du dernier tir
	}
}
