using UnityEngine;

public class CompassVisual : MonoBehaviour
{
	public RectTransform compassImage; // Image circulaire de la boussole
	private Transform player;
	public float compassOffset = 0f; // Décalage en degrés pour corriger l'alignement

	private void Start()
	{
		// Trouver le joueur dans la scène
		player = GameObject.FindGameObjectWithTag("Player").transform;
	}

	void Update()
	{
		if (player == null)
			return;

		// Obtenir la rotation du joueur autour de l'axe Y
		float playerRotationY = player.eulerAngles.y;

		// Ajuster la rotation de la boussole pour pointer le Nord
		compassImage.localEulerAngles = new Vector3(0, 0, -(playerRotationY + compassOffset));
	}
}
