using UnityEngine;

[CreateAssetMenu(fileName = "New Scatter Category", menuName = "Scatter/Category", order = 1)]
public class ScatterCategory : ScriptableObject
{
	[Header("Cat�gorie")]
	public string categoryName = "New Category";

	[Header("Prefabs de cette cat�gorie")]
	public GameObject[] prefabs;

	[Header("Param�tres de scatter")]
	public float offsetY = 0f; // D�calage en Y (vertical) pour cette cat�gorie
	public bool randomRotation = true; // Rotation al�atoire
	public int numberOfObjects = 10; // Nombre d'objets � instancier pour cette cat�gorie

	[Header("�chelle Al�atoire")]
	public Vector3 randomScaleRangeMin = new Vector3(1f, 1f, 1f); // �chelle minimale (x, y, z)
	public Vector3 randomScaleRangeMax = new Vector3(1f, 1f, 1f); // �chelle maximale (x, y, z)

	[Header("Param�tres Raycast")]
	public LayerMask groundLayer; // Couches utilis�es pour d�tecter le sol sp�cifique � cette cat�gorie

	[Header("Couches Interdites")]
	public LayerMask forbiddenLayers; // Couches o� la dispersion est interdite
	public float checkRadius = 0.5f; // Rayon de v�rification autour du point de dispersion
}
