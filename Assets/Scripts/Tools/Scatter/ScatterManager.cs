using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class ScatterManager : MonoBehaviour
{
	[Header("Zone de dispersion (X,Z)")]
	public Vector2 scatterAreaSize = new Vector2(10, 10);

	[Header("Catégories à instancier")]
	public List<ScatterCategory> categories;

	[Header("Paramètres Raycast Globaux")]
	public float raycastHeight = 10f;      // Hauteur de départ pour le raycast
	public float maxRaycastDistance = 20f; // Distance max du raycast

	[Header("Parenting")]
	public bool parentToScatterManager = true;

	// Liste interne pour stocker les objets créés
	private List<GameObject> scatteredObjects = new List<GameObject>();

	private void Start()
	{
		// Facultatif : en mode Play, générer automatiquement si vous le désirez
		if (Application.isPlaying)
		{
			Scatter();
		}
	}

	/// <summary>
	/// Méthode principale pour générer tous les objets de toutes les catégories.
	/// </summary>
	public void Scatter()
	{
		// On commence par nettoyer les objets déjà en scène.
		ClearScatter();

		// Pour chaque catégorie configurée
		foreach (ScatterCategory category in categories)
		{
			if (category == null || category.prefabs == null || category.prefabs.Length == 0)
				continue;

			for (int i = 0; i < category.numberOfObjects; i++)
			{
				// Choix aléatoire d'un prefab dans la catégorie
				GameObject prefab = category.prefabs[Random.Range(0, category.prefabs.Length)];
				if (prefab == null) continue;

				// Position aléatoire dans la zone (X,Z)
				Vector3 randomPos = new Vector3(
					Random.Range(-scatterAreaSize.x / 2f, scatterAreaSize.x / 2f),
					raycastHeight,
					Random.Range(-scatterAreaSize.y / 2f, scatterAreaSize.y / 2f)
				);

				Vector3 worldPosition = transform.position + randomPos;

				// Combinaison des couches du sol et des couches interdites
				LayerMask combinedLayerMask = category.groundLayer | category.forbiddenLayers;

				// Raycast vers le bas pour trouver la position sur le sol ou une couche interdite
				if (Physics.Raycast(worldPosition, Vector3.down, out RaycastHit hit, maxRaycastDistance, combinedLayerMask))
				{
					// Vérifier si le premier hit est sur une couche interdite
					if (((1 << hit.collider.gameObject.layer) & category.forbiddenLayers) != 0)
					{
						// Si le hit est sur une couche interdite, ignorer la dispersion
						Debug.LogWarning($"Dispersion interdite pour la catégorie '{category.categoryName}' " +
										 $"et le prefab '{prefab.name}' à la position {hit.point} car une couche interdite est détectée.");
						continue;
					}

					// Si le hit est sur le groundLayer, procéder
					if (((1 << hit.collider.gameObject.layer) & category.groundLayer) != 0)
					{
						Vector3 finalPos = hit.point;

						// Appliquer l'offset Y paramétré dans la catégorie
						finalPos.y += category.offsetY;

						// Vérifier si des objets sur les couches interdites sont présents autour de finalPos
						Collider[] forbiddenColliders = Physics.OverlapSphere(finalPos, category.checkRadius, category.forbiddenLayers);
						if (forbiddenColliders.Length == 0)
						{
							// Instancier le prefab
							GameObject scatteredObject = Instantiate(prefab, finalPos, Quaternion.identity);

							// Random rotation si activé pour cette catégorie
							if (category.randomRotation)
							{
								scatteredObject.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
							}

							// Scale aléatoire par axe
							Vector3 randomScale = new Vector3(
								Random.Range(category.randomScaleRangeMin.x, category.randomScaleRangeMax.x),
								Random.Range(category.randomScaleRangeMin.y, category.randomScaleRangeMax.y),
								Random.Range(category.randomScaleRangeMin.z, category.randomScaleRangeMax.z)
							);
							scatteredObject.transform.localScale = randomScale;

							// Option : le rendre enfant de ce GameObject (pour regrouper dans la Hiérarchie)
							if (parentToScatterManager)
							{
								scatteredObject.transform.parent = this.transform;
							}

							// On mémorise l'objet pour pouvoir le Clear plus tard
							scatteredObjects.Add(scatteredObject);
						}
						else
						{
							Debug.LogWarning($"Dispersion interdite pour la catégorie '{category.categoryName}' " +
											 $"et le prefab '{prefab.name}' à la position {finalPos} car un objet sur une couche interdite est présent.");
						}
					}
				}
				else
				{
					Debug.LogWarning($"Aucun sol ou couche interdite détecté (Raycast) pour la catégorie '{category.categoryName}' " +
									 $"et le prefab '{prefab.name}' à la position {worldPosition}.");
				}
			}
		}
	}

	/// <summary>
	/// Supprime tous les objets déjà générés.
	/// </summary>
	public void ClearScatter()
	{
		foreach (var obj in scatteredObjects)
		{
			if (obj != null)
			{
				// DestroyImmediate pour mode Éditeur
				DestroyImmediate(obj);
			}
		}
		scatteredObjects.Clear();
	}

	private void OnDrawGizmosSelected()
	{
		// Dessin de la zone de scatter
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(transform.position, new Vector3(scatterAreaSize.x, 0, scatterAreaSize.y));
	}

#if UNITY_EDITOR
	// -- Partie éditeur : Custom Editor pour ajouter deux boutons --

	[CustomEditor(typeof(ScatterManager))]
	public class ScatterManagerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			// Affiche toutes les propriétés (scatterAreaSize, categories, etc.)
			base.OnInspectorGUI();

			// Récupération du script
			ScatterManager manager = (ScatterManager)target;

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Editor Actions", EditorStyles.boldLabel);

			// Bouton Generate Scatter
			if (GUILayout.Button("Generate Scatter"))
			{
				manager.Scatter();
				EditorUtility.SetDirty(manager);
			}

			// Bouton Clear Scatter
			if (GUILayout.Button("Clear Scatter"))
			{
				manager.ClearScatter();
				EditorUtility.SetDirty(manager);
			}
		}
	}
#endif
}
