using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StressManager : MonoBehaviour
{
	public static StressManager Instance { get; private set; }

	public float stressLevel = 0f; // Niveau de stress (0-100)
	public Slider stressBar; // Barre de stress
	public Image stressImage;
	public AudioSource heartBeatSound; // Son de battement de c�ur

	public float baseStressIncreaseRate = 1f; // Augmentation normale dans les zones sombres
	public List<MonsterZone> monsterZones; // Liste des zones de monstres

	private float stressReductionRate = 0f; // Stocke temporairement la r�duction due aux zones calmes

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

		foreach (MonsterZone zone in monsterZones)
		{
			currentStressIncreaseRate += zone.GetStressIncrease();
		}

		// Appliquer l'augmentation du stress
		stressLevel += currentStressIncreaseRate * Time.deltaTime;

		// Appliquer la r�duction du stress accumul�e par les zones calmes
		if (stressReductionRate > 0)
		{
			stressLevel -= stressReductionRate * Time.deltaTime;
		}

		// Limiter le stress entre 0 et 100
		stressLevel = Mathf.Clamp(stressLevel, 0, 100);

		// R�initialiser la r�duction pour la prochaine frame
		stressReductionRate = 0;

		// Mettre � jour l'interface utilisateur
		UpdateUI();
	}

	public void ReduceStress(float reductionRate)
	{
		// Ajouter la r�duction (pour une application dans la frame suivante)
		stressReductionRate += reductionRate;
		Debug.Log($"R�duction du stress accumul�e : {stressReductionRate}, Niveau de stress actuel : {stressLevel}");
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
