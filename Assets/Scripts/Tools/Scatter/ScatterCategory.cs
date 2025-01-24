using UnityEngine;

[CreateAssetMenu(fileName = "New Scatter Category", menuName = "Scatter/Category", order = 1)]
public class ScatterCategory : ScriptableObject
{
	[Header("Catégorie")]
	public string categoryName = "New Category";

	[Header("Prefabs de cette catégorie")]
	public GameObject[] prefabs;

	[Header("Paramètres de scatter")]
	public float offsetY = 0f; // Décalage en Y (vertical) pour cette catégorie
	public bool randomRotation = true; // Rotation aléatoire
	public int numberOfObjects = 10; // Nombre d'objets à instancier pour cette catégorie

	[Header("Échelle Aléatoire")]
	public Vector3 randomScaleRangeMin = new Vector3(1f, 1f, 1f); // Échelle minimale (x, y, z)
	public Vector3 randomScaleRangeMax = new Vector3(1f, 1f, 1f); // Échelle maximale (x, y, z)

	[Header("Paramètres Raycast")]
	public LayerMask groundLayer; // Couches utilisées pour détecter le sol spécifique à cette catégorie

	[Header("Couches Interdites")]
	public LayerMask forbiddenLayers; // Couches où la dispersion est interdite
	public float checkRadius = 0.5f; // Rayon de vérification autour du point de dispersion
}
