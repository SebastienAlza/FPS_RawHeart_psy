using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;

public class EnemySpawner : MonoBehaviour
{
	[Header("Spawn Settings")]
	public ObjectPool enemyPool;
	public Transform player;
	public Vector2 spawnRandomAngle = new Vector2(90, 180);
	public float spawnRadiusMin = 20f;
	public float spawnRadiusMax = 40f;
	public float spawnInterval = 2f;
	public int maxEnemies = 20;
	public int minEnemiesPerSpawn = 1;
	public int maxEnemiesPerSpawn = 5;

	[Header("Enemy Movement")]
	public Vector2 randomEnemySpeed = new Vector2(3, 5);

	private List<GameObject> activeEnemies = new List<GameObject>();

	void Start()
	{
		InvokeRepeating(nameof(SpawnEnemies), 0f, spawnInterval);
	}

	private void SpawnEnemies()
	{
		CleanActiveEnemies();

		int enemiesToSpawn = Mathf.Min(Random.Range(minEnemiesPerSpawn, maxEnemiesPerSpawn + 1), maxEnemies - activeEnemies.Count);
		if (enemiesToSpawn <= 0)
		{
			Debug.Log("No enemies to spawn.");
			return;
		}

		for (int i = 0; i < enemiesToSpawn; i++)
		{
			SpawnEnemy();
		}
	}

	private void SpawnEnemy()
	{
		if (activeEnemies.Count >= maxEnemies)
		{
			Debug.LogWarning("Cannot spawn enemy: max active enemies reached.");
			return;
		}

		GameObject enemy = enemyPool.GetObject();
		if (enemy == null)
		{
			Debug.LogError("Failed to spawn enemy: Pool is empty.");
			return; // Empêche l'exécution si aucun ennemi n'est disponible
		}

		// Configurer l'ennemi récupéré du pool
		enemy.transform.position = GetSpawnPosition();
		enemy.transform.rotation = Quaternion.identity;

		// Initialiser les composants de l'ennemi
		EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
		if (enemyHealth != null)
		{
			enemyHealth.Initialize();
			enemyHealth.OnEnemyDeath += () => ReturnEnemyToPool(enemy);
		}

		activeEnemies.Add(enemy);

		EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
		if (enemyAI != null)
		{
			enemyAI.target = player;
			enemyAI.speed = Random.Range(randomEnemySpeed.x, randomEnemySpeed.y);
			enemyAI.movementType = (EnemyAI.MovementType)Random.Range(0, System.Enum.GetValues(typeof(EnemyAI.MovementType)).Length);
			//enemyAI.movementType = EnemyAI.MovementType.Rotating;
			if (enemyAI.movementType == EnemyAI.MovementType.Rotating)
			{
				enemy.GetComponent<EnemyShooting>().activeShooting = true;
				enemyAI.size = 3;
				enemy.GetComponent<EnemyHealth>().maxHealth = 100;
				enemyAI.transform.position = new Vector3(enemyAI.transform.position.x, enemyAI.transform.position.y + 5, enemyAI.transform.position.z);
			}
			else
			{
				enemyAI.size = 1;
			}


		}

		Debug.Log("Enemy spawned. Active enemies count: " + activeEnemies.Count);
	}


	private Vector3 GetSpawnPosition()
	{
		float randomAngle = Random.Range(spawnRandomAngle.x, spawnRandomAngle.y) * Mathf.Deg2Rad;
		float spawnDistance = Random.Range(spawnRadiusMin, spawnRadiusMax);

		Vector3 direction = new Vector3(Mathf.Cos(randomAngle), 0, Mathf.Sin(randomAngle)).normalized;
		return player.position + direction * spawnDistance;
	}

	private void ReturnEnemyToPool(GameObject enemy)
	{
		if (activeEnemies.Contains(enemy))
		{
			activeEnemies.Remove(enemy);
			enemy.SetActive(false); // Désactiver l'ennemi
			enemyPool.ReturnObject(enemy); // Le remettre dans le pool
			Debug.Log("Enemy returned to pool. Active enemies count: " + activeEnemies.Count);
		}
		else
		{
			Debug.LogWarning("Attempted to return an enemy not in the active list.");
		}
	}

	private void CleanActiveEnemies()
	{
		for (int i = activeEnemies.Count - 1; i >= 0; i--)
		{
			if (!activeEnemies[i].activeInHierarchy)
			{
				ReturnEnemyToPool(activeEnemies[i]);
			}
		}
	}
}
