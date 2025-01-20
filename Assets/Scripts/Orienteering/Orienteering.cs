using UnityEngine;
using UnityEngine.UI;

public class Orienteering : MonoBehaviour
{
	public Transform player; // Référence au joueur
	public DynamicCalmZone CalmZone; // Liste des zones calmes dans la scène
	public Text compassText; // Texte de la boussole (UI)

	void Update()
	{
		Transform closestZone = GetClosestCalmZone();

		if (closestZone != null)
		{
			// Calculer la direction vers la zone calme
			Vector3 directionToZone = closestZone.position - player.position;

			// Convertir la direction en un angle
			float angle = Mathf.Atan2(directionToZone.z, directionToZone.x) * Mathf.Rad2Deg;

			// Convertir l'angle en une direction cardinale approximative
			string direction = GetCardinalDirection(angle);

			// Afficher la direction dans le texte
			compassText.text = "Dirigez-vous vers : " + direction;
		}
	}

	// Trouver la zone calme la plus proche
	Transform GetClosestCalmZone()
	{
		Transform closestZone = null;
		float shortestDistance = Mathf.Infinity;


			float distance = Vector3.Distance(player.position, CalmZone.transform.position);
			if (distance < shortestDistance)
			{
				shortestDistance = distance;
				closestZone = CalmZone.transform;
			}

		return closestZone;
	}

	// Convertir un angle en une direction cardinale
	string GetCardinalDirection(float angle)
	{
		if (angle < 0) angle += 360;

		if (angle >= 337.5 || angle < 22.5)
			return "Nord";
		else if (angle >= 22.5 && angle < 67.5)
			return "Nord-Est";
		else if (angle >= 67.5 && angle < 112.5)
			return "Est";
		else if (angle >= 112.5 && angle < 157.5)
			return "Sud-Est";
		else if (angle >= 157.5 && angle < 202.5)
			return "Sud";
		else if (angle >= 202.5 && angle < 247.5)
			return "Sud-Ouest";
		else if (angle >= 247.5 && angle < 292.5)
			return "Ouest";
		else
			return "Nord-Ouest";
	}
}
