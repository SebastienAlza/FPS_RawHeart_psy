using UnityEngine;

public class FootstepSound : MonoBehaviour
{
	public AudioSource audioSource;
	public AudioClip[] defaultFootsteps;
	public AudioClip[] grassFootsteps;
	public AudioClip[] bushRustleSounds; // Sons de bruissement dans les broussailles

	public FPSController fpsController;
	public float baseStepRate = 0.5f;
	private float nextStepTime = 0f;
	private bool inBush = false;

	public LayerMask bushLayer; // Définit le Layer des broussailles pour l'optimisation

	private void Update()
	{
		float speed = fpsController.CurrentSpeed;

		if (speed > 0.1f && Time.time >= nextStepTime)
		{
			inBush = CheckIfInBush(); // Vérifie si le joueur est proche d'une broussaille
			PlayFootstepSound();
			nextStepTime = Time.time + (baseStepRate / Mathf.Max(speed, 1f));
		}
	}

	private void PlayFootstepSound()
	{
		AudioClip[] selectedFootsteps = defaultFootsteps;

		// Détection du sol
		RaycastHit hit;
		if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f))
		{
			if (hit.collider.CompareTag("Grass"))
				selectedFootsteps = grassFootsteps;
		}

		// Joue le son de pas
		if (selectedFootsteps.Length > 0)
		{
			audioSource.clip = selectedFootsteps[Random.Range(0, selectedFootsteps.Length)];
			audioSource.pitch = Random.Range(0.9f, 1.1f);
			audioSource.Play();
		}

		// Si dans les broussailles, joue un son de bruissement
		if (inBush && bushRustleSounds.Length > 0)
		{
			audioSource.PlayOneShot(bushRustleSounds[Random.Range(0, bushRustleSounds.Length)]);
		}
	}

	private bool CheckIfInBush()
	{
		Collider[] colliders = Physics.OverlapSphere(transform.position, 1f, bushLayer);
		return colliders.Length > 0; // Si un objet dans le Layer "Broussailles" est proche, retourne vrai
	}
}
