using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StressManager : MonoBehaviour
{
	public static StressManager Instance { get; private set; }

	public float stressLevel = 0f; // Niveau de stress (0-100)
	public Slider stressBar; // Barre de stress
	public Image stressImage;
	public AudioSource heartBeatSound; // Son de battement de cœur

	public float baseStressIncreaseRate = 1f; // Augmentation normale dans les zones sombres
	public List<MonsterZone> monsterZones; // Liste des zones de monstres

	private float stressReductionRate = 0f; // Réduction actuelle due aux zones calmes
	private bool isInCalmZone = false; // Indique si le joueur est dans une zone calme

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	void Update()
	{
		// Calcul de l'augmentation du stress
		float currentStressIncreaseRate = baseStressIncreaseRate;

		if (isInCalmZone)
		{
			// Appliquer la réduction du stress accumulée par les zones calmes
			if (stressReductionRate > 0)
			{
				stressLevel -= stressReductionRate * Time.deltaTime;
			}
		}
		else
		{
			foreach (MonsterZone zone in monsterZones)
			{
				currentStressIncreaseRate += zone.GetStressIncrease();
			}

			// Appliquer l'augmentation du stress
			stressLevel += currentStressIncreaseRate * Time.deltaTime;
		}

		// Limiter le stress entre 0 et 100
		stressLevel = Mathf.Clamp(stressLevel, 0, 100);

		// Mettre à jour l'interface utilisateur
		UpdateUI();
	}

	public void EnterCalmZone(float reductionRate)
	{
		// Définir le joueur dans une zone calme
		isInCalmZone = true;
		stressReductionRate = reductionRate;
	}

	public void ExitCalmZone()
	{
		// Sortir de la zone calme
		isInCalmZone = false;
		stressReductionRate = 0f;
	}

	private void UpdateUI()
	{
		if (stressBar != null)
		{
			stressBar.value = stressLevel / 100;
			stressImage.color = new Color(stressImage.color.r, stressImage.color.g, stressImage.color.b, stressLevel / 255);
		}

		if (heartBeatSound != null)
		{
			heartBeatSound.pitch = Mathf.Lerp(1.0f, 2.0f, stressLevel / 100.0f);
		}
	}
}
