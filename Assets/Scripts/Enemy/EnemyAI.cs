using UnityEngine;

public class EnemyAI : MonoBehaviour
{
	public enum MovementType { Straight, SinusoidalHorizontal, SinusoidalVertical, SinusoidalCombined, Rotating }
	public MovementType movementType = MovementType.Straight;

	public float offset = 0f; // Décalage pour éviter un démarrage à 0 dans le mouvement sinusoïdal
	public float speed = 3f; // Vitesse de déplacement
	public float detectionRadius = 10f; // Rayon de détection
	public float stopRadius = 3f; // Distance minimale avant que l'IA s'arrête
	public float sinusoidalAmplitude = 2f; // Amplitude du mouvement sinusoïdal
	public float sinusoidalFrequency = 2f; // Fréquence du mouvement sinusoïdal
	public float rotationSpeed = 50f; // Vitesse de rotation pour le mouvement rotatif
	public Transform target; // Cible (le joueur)
	public float size = 1;


	public LayerMask groundLayer; // Le layer utilisé pour le mesh du sol
	private bool isPlayerInRange = false; // Indique si le joueur est dans la zone de détection

	void Start()
	{
		transform.localScale = new Vector3(size, size, size);
		// Trouver le joueur par son tag
		GameObject player = GameObject.FindWithTag("Player");
		if (player != null)
		{
			target = player.transform;
		}
		else
		{
			Debug.LogWarning("No GameObject with tag 'Player' found in the scene.");
		}
	}

	void Update()
	{
		if (target == null) return;

		// Calculer la distance au joueur
		float distanceToPlayer = Vector3.Distance(transform.position, target.position);

		if (distanceToPlayer <= detectionRadius)
		{
			if (!isPlayerInRange)
			{
				// Détecte que le joueur est entré dans la zone de détection
				isPlayerInRange = true;
				Debug.Log("Player entered detection radius!");
			}

			// Si le joueur est dans la zone mais trop proche, arrêter le mouvement
			if (distanceToPlayer <= stopRadius)
			{
				StopMoving();
			}
			else
			{
				// Se déplacer vers le joueur
				MoveTowardsPlayer();
			}
		}
		else
		{
			if (isPlayerInRange)
			{
				// Détecte que le joueur a quitté la zone de détection
				isPlayerInRange = false;
				Debug.Log("Player left detection radius!");
			}

			// Arrêter tout mouvement si le joueur est en dehors du rayon de détection
			StopMoving();
		}
	}

	private void MoveTowardsPlayer()
	{
		Vector3 direction = (target.position - transform.position).normalized;
		Vector3 movement = Vector3.zero;

		// Appliquer le type de déplacement
		switch (movementType)
		{
			case MovementType.Straight:
				movement = direction * speed * Time.deltaTime;
				break;

			case MovementType.SinusoidalHorizontal:
				movement = CalculateSinusoidalMovement(direction, Vector3.right);
				break;

			case MovementType.SinusoidalVertical:
				movement = CalculateSinusoidalMovement(direction, Vector3.up);
				break;

			case MovementType.SinusoidalCombined:
				movement = CalculateSinusoidalMovement(direction, Vector3.right) + CalculateSinusoidalMovement(direction, Vector3.up);
				break;

			case MovementType.Rotating:
				RotateInPlace();
				break;
		}

		// Appliquer le mouvement tout en respectant la surface du sol (utiliser un raycast)
		Vector3 newPosition = transform.position + movement;
		newPosition = AdjustToGround(newPosition); // Ajuster la position pour coller au mesh du sol
		transform.position = newPosition;

		// Orienter l'ennemi vers la cible
		if (movementType != MovementType.Rotating) // Ne pas orienter si l'ennemi tourne sur place
		{
			transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
		}
	}

	private void RotateInPlace()
	{
		// Faire tourner l'ennemi autour de son propre axe Y
		transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
	}

	private Vector3 CalculateSinusoidalMovement(Vector3 baseDirection, Vector3 sinusoidalAxis)
	{
		// Projeter la direction de base sur l'axe souhaité pour appliquer la sinusoïde
		Vector3 sinusoidalDirection = Vector3.Cross(baseDirection, sinusoidalAxis).normalized;
		float sinusoidalOffset = Mathf.Sin(Time.time * sinusoidalFrequency + offset) * sinusoidalAmplitude;

		return (baseDirection + sinusoidalDirection * sinusoidalOffset) * speed * Time.deltaTime;
	}

	private Vector3 AdjustToGround(Vector3 position)
	{
		RaycastHit hit;

		// Lancer un raycast vers le bas depuis une hauteur au-dessus de l'ennemi
		Vector3 rayOrigin = position + Vector3.up * 5f; // Ajustez la hauteur si nécessaire
		if (Physics.Raycast(rayOrigin, Vector3.down, out hit, Mathf.Infinity, groundLayer))
		{
			position.y = hit.point.y; // Ajuster la position Y en fonction du point d'impact
		}
		else
		{
			Debug.LogWarning($"[EnemyAI] Raycast didn't hit ground for {gameObject.name}.");
		}

		return position;
	}

	private void StopMoving()
	{
		// Ajouter une logique pour arrêter le mouvement (par exemple, une animation ou un état statique)
	}
}
