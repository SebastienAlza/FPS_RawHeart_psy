using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
	public float maxHealth = 50f;
	private float currentHealth;

	public event Action OnEnemyDeath; // Événement déclenché lors de la mort

	[Header("Effects")]
	public GameObject deathEffectPrefab; // Préfab pour l'effet de destruction
	public Color hitColor = Color.red; // Couleur lorsque l'ennemi est touché
	public float hitFlashDuration = 0.1f; // Durée du flash

	private Renderer enemyRenderer;
	private Color originalColor;

	void Start()
	{
		Initialize();
	}

	public void TakeDamage(float damage)
	{
		if (!gameObject.activeInHierarchy)
		{
			Debug.LogWarning($"{gameObject.name} is inactive but received damage.");
			return;
		}

		currentHealth -= damage;

		if (enemyRenderer != null)
		{
			StartCoroutine(FlashHitEffect());
		}

		if (currentHealth <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		OnEnemyDeath?.Invoke();

		if (deathEffectPrefab != null)
		{
			GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
			Destroy(effect, 2f); // Détruire l'effet après 2 secondes
		}

		ResetEnemy();
	}

	public void Initialize()
	{
		currentHealth = maxHealth;

		if (enemyRenderer == null)
		{
			enemyRenderer = GetComponent<Renderer>();
			if (enemyRenderer != null)
			{
				originalColor = enemyRenderer.material.color;
			}
		}

		if (enemyRenderer != null)
		{
			enemyRenderer.material.color = originalColor;
		}

		ClearOnEnemyDeathEvent();
	}

	private void ResetEnemy()
	{
		currentHealth = maxHealth;

		if (enemyRenderer != null)
		{
			enemyRenderer.material.color = originalColor;
		}

		gameObject.SetActive(false); // Désactiver l'objet pour le pool
	}

	private System.Collections.IEnumerator FlashHitEffect()
	{
		if (enemyRenderer != null)
		{
			enemyRenderer.material.color = hitColor;
		}

		yield return new WaitForSeconds(hitFlashDuration);

		if (enemyRenderer != null)
		{
			enemyRenderer.material.color = originalColor;
		}
	}

	public void ClearOnEnemyDeathEvent()
	{
		OnEnemyDeath = null;
	}
}
