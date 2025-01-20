using UnityEngine;

public class DynamicCalmZone : MonoBehaviour
{
	public float calmZoneReductionRate = 10f; // Taux de réduction de stress par seconde
	public float relocationDelay = 3f; // Temps d'attente avant de déplacer la zone
	public Vector2 mapBoundsX = new Vector2(-50f, 50f); // Limites de déplacement sur l'axe X
	public Vector2 mapBoundsZ = new Vector2(-50f, 50f); // Limites de déplacement sur l'axe Z

	private bool isRelocating = false;

	private void OnTriggerStay(Collider other)
	{
		if (other.CompareTag("Player") && !isRelocating)
		{

				StressManager.Instance.ReduceStress(calmZoneReductionRate * Time.deltaTime);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player") && !isRelocating)
		{
			Debug.Log("Zone calme trouvée");

			// Démarrer le déplacement aléatoire après un délai
			StartCoroutine(RelocateZoneAfterDelay());
		}
	}

	private System.Collections.IEnumerator RelocateZoneAfterDelay()
	{
		isRelocating = true;

		// Attendre avant de déplacer
		yield return new WaitForSeconds(relocationDelay);

		// Générer une nouvelle position aléatoire dans les limites
		float newX = Random.Range(mapBoundsX.x, mapBoundsX.y);
		float newZ = Random.Range(mapBoundsZ.x, mapBoundsZ.y);

		// Déplacer la zone calme à la nouvelle position
		transform.position = new Vector3(newX, transform.position.y, newZ);

		isRelocating = false;
	}

	// Optionnel : Afficher les limites dans la vue Scène
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(
			new Vector3((mapBoundsX.x + mapBoundsX.y) / 2, transform.position.y, (mapBoundsZ.x + mapBoundsZ.y) / 2),
			new Vector3(mapBoundsX.y - mapBoundsX.x, 1, mapBoundsZ.y - mapBoundsZ.x)
		);
	}
}
