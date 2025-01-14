using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class ZombieHordeSpawner : MonoBehaviour
{
	[Header("Points de spawn")]
	public List<Transform> spawnPoints; // Points de spawn prédéfinis

	[Header("Paramètres de la horde")]
	public GameObject zombiePrefab; // Préfab du zombie
	public int hordeSize = 5; // Nombre de zombies par horde
	public float spawnInterval = 10f; // Temps entre les spawns (en secondes)

	[Header("Zone de détection du joueur")]
	public Transform player;
	public float detectionRadius = 20f; // Rayon dans lequel les zombies peuvent être spawnés

	private bool spawning = false; // État pour éviter les multiples spawns en parallèle

	void Start()
	{
		if (player == null)
		{
			player = GameObject.FindGameObjectWithTag("Player")?.transform;
			if (player == null)
			{
				Debug.LogError("Joueur introuvable ! Assurez-vous qu'un GameObject avec le tag 'Player' existe dans la scène.");
				enabled = false;
				return;
			}
		}

		if (spawnPoints == null || spawnPoints.Count == 0)
		{
			Debug.LogError("Aucun point de spawn défini ! Ajoutez des points de spawn dans la liste.");
			enabled = false;
			return;
		}

		if (zombiePrefab == null)
		{
			Debug.LogError("Aucun prefab de zombie assigné ! Assignez un prefab de zombie dans l'inspecteur.");
			enabled = false;
			return;
		}

		StartCoroutine(SpawnHordeRoutine());
	}

	private IEnumerator SpawnHordeRoutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(spawnInterval);

			if (!spawning)
			{
				spawning = true;
				SpawnHorde();
				spawning = false;
			}
		}
	}

	private void SpawnHorde()
	{
		if (player != null && IsAnySpawnPointInRadius())
		{
			Debug.Log("Joueur détecté, spawn d'une horde !");
			for (int i = 0; i < hordeSize; i++)
			{
				Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

				GameObject zombie = Instantiate(zombiePrefab, randomSpawnPoint.position, Quaternion.identity);
				AIDestinationSetter aiDestination = zombie.GetComponent<AIDestinationSetter>();
				if (aiDestination != null)
				{
					aiDestination.target = player;
				}
				else
				{
					Debug.LogWarning("AIDestinationSetter manquant sur le prefab de zombie !");
				}
			}
		}
		else
		{
			Debug.Log("Aucun point de spawn dans le rayon du joueur.");
		}
	}

	private bool IsAnySpawnPointInRadius()
	{
		foreach (Transform spawnPoint in spawnPoints)
		{
			if (Vector3.Distance(player.position, spawnPoint.position) <= detectionRadius)
			{
				return true;
			}
		}
		return false;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		if (player != null)
		{
			Gizmos.DrawWireSphere(player.position, detectionRadius);
		}
	}
}
