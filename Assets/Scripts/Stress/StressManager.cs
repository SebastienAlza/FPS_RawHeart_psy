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

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject); // Détruire les doublons
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject); // Préserve cet objet entre les scènes
	}

	void Update()
	{
		// Calculer l'augmentation du stress de base
		float currentStressIncreaseRate = baseStressIncreaseRate;

		// Ajouter l'effet de stress de chaque zone de monstre
		foreach (MonsterZone zone in monsterZones)
		{
			currentStressIncreaseRate += zone.GetStressIncrease();
		}

		// Appliquer l'augmentation du stress
		stressLevel += currentStressIncreaseRate * Time.deltaTime;

		// Limiter la jauge de stress
		stressLevel = Mathf.Clamp(stressLevel, 0, 100);

		// Mettre à jour la barre de stress
		if (stressBar != null)
		{
			stressBar.value = stressLevel / 100;
			stressImage.color = new Color (stressImage.color.r, stressImage.color.g, stressImage.color.b, stressLevel/255);
		}

		// Modifier le son du cœur en fonction du stress
		if (heartBeatSound != null)
			heartBeatSound.pitch = Mathf.Lerp(1.0f, 2.0f, stressLevel / 100.0f);
	}

	// Réduction du stress appelée par les CalmZones
	public void ReduceStress(float reductionRate)
	{
		stressLevel -= reductionRate * Time.deltaTime;
	}
}
