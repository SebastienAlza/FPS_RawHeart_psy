using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DynamicFOV : MonoBehaviour
{
	[Header("FOV Settings")]
	public CinemachineCamera cinemachineCamera; // La cam�ra principale
	public float baseFOV = 60f; // FOV par d�faut
	public float maxFOV = 90f; // FOV maximal pendant un sprint
	public float fovTransitionSpeed = 5f; // Vitesse de transition entre les FOV

	[Header("Effect Settings")]
	public Volume postProcessVolume; // Volume Post-Processing
	public float minAberrationIntensity = 0.25f;
	public float maxAberrationIntensity = 0.5f;

	[Header("Speed Settings")]
	public FPSController fpsController; // R�f�rence au script FPSController
	public float sprintSpeedThreshold = 6f; // Vitesse au-dessus de laquelle la FOV commence � augmenter

	private float targetFOV; // FOV vers lequel on veut aller
	private ChromaticAberration chromaticAberration;

	void Start()
	{
		targetFOV = baseFOV; // Initialiser avec le FOV de base
		cinemachineCamera.Lens.FieldOfView = baseFOV;

		if (postProcessVolume != null && postProcessVolume.profile.TryGet(out ChromaticAberration chromatic))
		{
			chromaticAberration = chromatic;
			chromaticAberration.intensity.value = minAberrationIntensity;
			Debug.Log("Chromatic Aberration r�cup�r� avec succ�s !");
		}
		else
		{
			Debug.LogError("Chromatic Aberration n'est pas configur� ou introuvable dans le Volume !");
		}
	}

	void Update()
	{
		// Obtenir la vitesse actuelle depuis le script FPSController
		float currentSpeed = fpsController.CurrentSpeed;

		// D�finir le FOV cible en fonction de la vitesse
		if (currentSpeed > sprintSpeedThreshold)
		{
			targetFOV = Mathf.Lerp(baseFOV, maxFOV, (currentSpeed - sprintSpeedThreshold) / (fpsController.sprintSpeed - sprintSpeedThreshold));

			if (chromaticAberration != null)
			{
				chromaticAberration.intensity.value = Mathf.Lerp(minAberrationIntensity, maxAberrationIntensity, (currentSpeed - sprintSpeedThreshold) / (fpsController.sprintSpeed - sprintSpeedThreshold));
				Debug.Log($"Intensit� Aberration en Sprint : {chromaticAberration.intensity.value}");
			}
		}
		else
		{
			targetFOV = baseFOV;

			if (chromaticAberration != null)
			{
				chromaticAberration.intensity.value = Mathf.Lerp(chromaticAberration.intensity.value, minAberrationIntensity, Time.deltaTime * fovTransitionSpeed);
				Debug.Log($"Intensit� Aberration en Marche : {chromaticAberration.intensity.value}");
			}
		}

		// Transition fluide vers le FOV cible
		float currentFov = cinemachineCamera.Lens.FieldOfView;
		cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(currentFov, targetFOV, Time.deltaTime * fovTransitionSpeed);
	}
}
