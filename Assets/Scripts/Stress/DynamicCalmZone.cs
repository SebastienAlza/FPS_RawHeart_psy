using UnityEngine;

public class DynamicCalmZone : MonoBehaviour
{
	public float calmZoneReductionRate = 10f; // Taux de r�duction de stress par seconde
	public float relocationDelay = 3f; // Temps d'attente avant de d�placer la zone
	public Vector2 mapBoundsX = new Vector2(-50f, 50f); // Limites de d�placement sur l'axe X
	public Vector2 mapBoundsZ = new Vector2(-50f, 50f); // Limites de d�placement sur l'axe Z

	private bool isRelocating = false;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			// Indiquer que le joueur est dans une zone calme
			StressManager.Instance.EnterCalmZone(calmZoneReductionRate);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			// Indiquer que le joueur quitte la zone calme
			StressManager.Instance.ExitCalmZone();

			if (!isRelocating)
			{
				StartCoroutine(RelocateZoneAfterDelay());
			}
		}
	}

	private System.Collections.IEnumerator RelocateZoneAfterDelay()
	{
		isRelocating = true;

		// Attendre avant de d�placer
		yield return new WaitForSeconds(relocationDelay);

		// G�n�rer une nouvelle position al�atoire dans les limites
		float newX = Random.Range(mapBoundsX.x, mapBoundsX.y);
		float newZ = Random.Range(mapBoundsZ.x, mapBoundsZ.y);

		// D�placer la zone calme � la nouvelle position
		transform.position = new Vector3(newX, transform.position.y, newZ);

		isRelocating = false;
	}

	// Optionnel : Afficher les limites dans la vue Sc�ne
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(
			new Vector3((mapBoundsX.x + mapBoundsX.y) / 2, transform.position.y, (mapBoundsZ.x + mapBoundsZ.y) / 2),
			new Vector3(mapBoundsX.y - mapBoundsX.x, 1, mapBoundsZ.y - mapBoundsZ.x)
		);
	}
}
