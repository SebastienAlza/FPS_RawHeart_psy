using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class RailDeformer : MonoBehaviour
{
	[Header("Génération Automatique")]
	public bool autoGenerate = true;
	public bool loop = false;

	[Header("Splines / Prefabs")]
	public SplineContainer splineContainer;       // Spline à suivre
	public List<GameObject> railPrefabs;          // Prefabs possibles
	public bool randomPrefabs = false;            // Choix aléatoire ?

	[Header("Paramètres de Segment")]
	public bool adaptSegmentLength = false;       // Calculer la longueur depuis bounding box ?
	public float segmentLength = 2f;              // Sinon, on utilise cette valeur

	[Header("Combiner / Perf")]
	public bool collapseMeshes = true;            // True => on combine les meshes
	public int maxTrianglesPerMesh = 16000;       // Limite de triangles par mesh combiné

	[Header("Rotation Options")]
	public bool applyExtraRotation = false;       // Ajouter un offset de rotation ?
	public bool randomRotation = false;           // Offset aléatoire ?
	public Vector3 fixedRotationOffset = Vector3.zero;  // Offset fixe (euler)
	public Vector3 minRandomRotation = Vector3.zero;    // Min euler random
	public Vector3 maxRandomRotation = new Vector3(0, 360, 0);

	[Header("Position Offset (Local)")]
	public bool applyPositionOffset = false;
	public bool randomPositionOffset = false;
	public Vector3 fixedPositionOffset = Vector3.zero;
	public Vector3 minRandomPositionOffset = Vector3.zero;
	public Vector3 maxRandomPositionOffset = Vector3.zero;

	[Header("Scale")]
	public bool applyScale = false;
	public bool randomScale = false;
	public Vector3 fixedScale = Vector3.one;
	public Vector3 minRandomScale = Vector3.one;
	public Vector3 maxRandomScale = Vector3.one * 2f;

	// Flag interne pour la génération auto
	private bool _needsRegenerate = false;

	// Si on combine, on utilise un ou plusieurs MeshCombiner (classe séparée)
	private List<MeshCombiner> _meshCombiners = new List<MeshCombiner>();
	private MeshCombiner _currentCombiner = null;

	// ------------------------------------------------------------------------
	//  CLEAR / GENERATE
	// ------------------------------------------------------------------------
	public void ClearRails()
	{
		for (int i = transform.childCount - 1; i >= 0; i--)
		{
			GameObject child = transform.GetChild(i).gameObject;
			if (Application.isPlaying) Destroy(child);
			else DestroyImmediate(child);
		}
	}

	public void GenerateRails()
	{
		// 1) Vider
		ClearRails();

		// 2) Vérifs de base
		if (splineContainer == null)
		{
			Debug.LogWarning("SplineContainer non assigné !");
			return;
		}
		if (railPrefabs == null || railPrefabs.Count == 0)
		{
			Debug.LogWarning("Aucun prefab dans 'railPrefabs' !");
			return;
		}

		float length = splineContainer.CalculateLength();
		if (length <= 0f)
		{
			Debug.LogWarning("Spline vide ou invalide !");
			return;
		}

		// 3) Calcul du segmentLength
		float finalSegmentLength = adaptSegmentLength ? ComputeAveragePrefabLength(railPrefabs)
													  : segmentLength;
		if (finalSegmentLength < 0.05f)
		{
			Debug.LogWarning("segmentLength trop petit, on force 0.1f");
			finalSegmentLength = 0.1f;
		}

		int maxSegments = Mathf.CeilToInt(length / finalSegmentLength);
		if (maxSegments > 10000)
		{
			Debug.LogError("Trop de segments ! Augmentez segmentLength.");
			return;
		}

		// 4) Si collapseMeshes = true, on initialise un MeshCombiner
		_meshCombiners.Clear();
		int combinerIndex = 0;
		if (collapseMeshes)
		{
			_currentCombiner = new MeshCombiner(maxTrianglesPerMesh, $"Rails_Combined_{combinerIndex}");
			_meshCombiners.Add(_currentCombiner);
		}

		// 5) Parcourir la spline
		float progress = 0f;
		while (progress < length)
		{
			float startT = progress / length;
			float endT = Mathf.Min((progress + finalSegmentLength) / length, 1f);

			float midT = 0.5f * (startT + endT);
			Vector3 splinePos = splineContainer.Spline.EvaluatePosition(midT);
			Vector3 tangent = splineContainer.Spline.EvaluateTangent(midT);

			// Rotation de base alignée sur la spline
			Quaternion splineRot = Quaternion.LookRotation(tangent);

			// Calcul offset de rotation (fixe ou random)
			Quaternion finalRot = ComputeFinalRotation(splineRot);

			// Calcul offset de position local + application
			Vector3 finalPos = ComputeFinalPosition(splinePos, finalRot);

			// Choix prefab
			GameObject chosen = GetPrefab();
			if (!chosen) break;

			// Instancier
			GameObject segGO = Instantiate(chosen, finalPos, finalRot);
			segGO.name = $"RailSegment_{progress:F2}";

			// Appliquer le scale (éventuellement aléatoire)
			if (applyScale)
			{
				segGO.transform.localScale = ComputeFinalScale();
			}

			// Cloner le mesh pour éviter de modifier l’asset
			MeshFilter mf = segGO.GetComponent<MeshFilter>();
			if (mf && mf.sharedMesh)
			{
				mf.sharedMesh = Instantiate(mf.sharedMesh);
			}
			Mesh finalMesh = mf ? mf.sharedMesh : null;

			// Récupérer matériaux
			Material[] finalMats = null;
			MeshRenderer mr = segGO.GetComponent<MeshRenderer>();
			if (mr) finalMats = mr.sharedMaterials;

			// 6) Combiner ou pas
			if (collapseMeshes && finalMesh != null && finalMats != null)
			{
				// On calcule la matrice globale
				Matrix4x4 worldMatrix = segGO.transform.localToWorldMatrix;

				// On essaie d'ajouter au combiner courant
				bool ok = _currentCombiner.TryAddSegment(finalMesh, worldMatrix, finalMats);
				if (!ok)
				{
					// Finaliser et en recréer un
					FinalizeCurrentCombiner();

					combinerIndex++;
					_currentCombiner = new MeshCombiner(maxTrianglesPerMesh, $"Rails_Combined_{combinerIndex}");
					_meshCombiners.Add(_currentCombiner);

					bool ok2 = _currentCombiner.TryAddSegment(finalMesh, worldMatrix, finalMats);
					if (!ok2)
					{
						Debug.LogWarning($"Un seul segment dépasse {maxTrianglesPerMesh} triangles ?");
					}
				}

				// Détruire l’objet (temporaire)
				DestroyImmediate(segGO);
			}
			else
			{
				// On garde l’objet tel quel (non combiné)
				// => On le parent en conservant la position/rotation monde
				segGO.transform.SetParent(transform, false);
			}

			// Avancer
			progress += finalSegmentLength;
		}

		// 7) Finaliser le dernier combiner
		if (collapseMeshes)
		{
			FinalizeCurrentCombiner();
		}

		// 8) Bouclage
		if (loop && transform.childCount > 1)
		{
			GameObject first = transform.GetChild(0).gameObject;
			GameObject last = transform.GetChild(transform.childCount - 1).gameObject;
			last.transform.position = first.transform.position;
		}
	}

	/// <summary>
	/// Ferme le combiner courant et crée un GameObject avec le mesh combiné.
	/// </summary>
	private void FinalizeCurrentCombiner()
	{
		if (_currentCombiner == null || _currentCombiner.IsEmpty())
			return;

		Mesh combined = _currentCombiner.Build(out Material[] finalMats);
		if (combined == null) return;

		// Créer un GO
		GameObject combGO = new GameObject(combined.name);
		combGO.transform.SetParent(transform, false);

		MeshFilter mf = combGO.AddComponent<MeshFilter>();
		mf.sharedMesh = combined;

		MeshRenderer mr = combGO.AddComponent<MeshRenderer>();
		mr.sharedMaterials = finalMats;
	}

	// ------------------------------------------------------------------------
	//  ROTATION HELPER
	// ------------------------------------------------------------------------
	/// <summary>
	/// Combine la rotation de la spline + un offset (fixe ou random) selon les réglages.
	/// </summary>
	private Quaternion ComputeFinalRotation(Quaternion splineRot)
	{
		if (!applyExtraRotation)
		{
			return splineRot;
		}

		// On calcule un euler offset
		Vector3 eulerOffset;
		if (!randomRotation)
		{
			eulerOffset = fixedRotationOffset;
		}
		else
		{
			float rx = Random.Range(minRandomRotation.x, maxRandomRotation.x);
			float ry = Random.Range(minRandomRotation.y, maxRandomRotation.y);
			float rz = Random.Range(minRandomRotation.z, maxRandomRotation.z);
			eulerOffset = new Vector3(rx, ry, rz);
		}
		Quaternion offsetRot = Quaternion.Euler(eulerOffset);

		// On applique l'offset par-dessus la spline
		return splineRot * offsetRot;
	}

	// ------------------------------------------------------------------------
	//  POSITION OFFSET HELPER
	// ------------------------------------------------------------------------
	/// <summary>
	/// Calcule la position finale en ajoutant un offset local (fixe ou random) 
	/// appliqué selon la rotation finale.
	/// </summary>
	private Vector3 ComputeFinalPosition(Vector3 basePos, Quaternion finalRot)
	{
		if (!applyPositionOffset)
		{
			return basePos;
		}

		Vector3 offset;
		if (!randomPositionOffset)
		{
			offset = fixedPositionOffset;
		}
		else
		{
			float ox = Random.Range(minRandomPositionOffset.x, maxRandomPositionOffset.x);
			float oy = Random.Range(minRandomPositionOffset.y, maxRandomPositionOffset.y);
			float oz = Random.Range(minRandomPositionOffset.z, maxRandomPositionOffset.z);
			offset = new Vector3(ox, oy, oz);
		}

		// Appliquer l'offset en local : on le tourne par la rotation finale
		Vector3 localOffset = finalRot * offset;

		// On ajoute ça à la position de base
		return basePos + localOffset;
	}

	// ------------------------------------------------------------------------
	//  SCALE HELPER
	// ------------------------------------------------------------------------
	/// <summary>
	/// Retourne le scale (fixe ou random). 
	/// </summary>
	private Vector3 ComputeFinalScale()
	{
		if (!applyScale)
		{
			// Normalement on n'appelle pas cette fonction si !applyScale,
			// mais par sécurité, on retourne (1,1,1).
			return Vector3.one;
		}

		if (!randomScale)
		{
			return fixedScale;
		}
		else
		{
			float sx = Random.Range(minRandomScale.x, maxRandomScale.x);
			float sy = Random.Range(minRandomScale.y, maxRandomScale.y);
			float sz = Random.Range(minRandomScale.z, maxRandomScale.z);
			return new Vector3(sx, sy, sz);
		}
	}

	// ------------------------------------------------------------------------
	//  OUTILS
	// ------------------------------------------------------------------------
	private GameObject GetPrefab()
	{
		if (railPrefabs.Count == 0) return null;
		if (!randomPrefabs || railPrefabs.Count == 1)
		{
			return railPrefabs[0];
		}
		int index = Random.Range(0, railPrefabs.Count);
		return railPrefabs[index];
	}

	private float ComputeAveragePrefabLength(List<GameObject> prefabs)
	{
		if (prefabs == null || prefabs.Count == 0) return 2f;

		float totalZ = 0f;
		int validCount = 0;

		foreach (GameObject prefab in prefabs)
		{
			if (!prefab) continue;
			float sizeZ = GetBoundingBoxZ(prefab);
			if (sizeZ > 0f)
			{
				totalZ += sizeZ;
				validCount++;
			}
		}

		if (validCount == 0) return 2f;
		return totalZ / validCount;
	}

	private float GetBoundingBoxZ(GameObject prefab)
	{
		GameObject temp = Instantiate(prefab);
		temp.hideFlags = HideFlags.HideAndDontSave;

		Bounds combined = new Bounds();
		bool hasBound = false;

		MeshRenderer[] renderers = temp.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer r in renderers)
		{
			if (!hasBound)
			{
				combined = r.bounds;
				hasBound = true;
			}
			else
			{
				combined.Encapsulate(r.bounds);
			}
		}

		if (Application.isPlaying) Destroy(temp);
		else DestroyImmediate(temp);

		return hasBound ? combined.size.z : 2f;
	}

	// ------------------------------------------------------------------------
	//  OnValidate + Update
	// ------------------------------------------------------------------------
	private void OnValidate()
	{
		if (autoGenerate)
		{
			_needsRegenerate = true;
		}
	}

	private void Update()
	{
		if (!Application.isPlaying && _needsRegenerate)
		{
			_needsRegenerate = false;
			GenerateRails();
		}
	}

	// ------------------------------------------------------------------------
	//  PARTIE ÉDITEUR : BOUTONS
	// ------------------------------------------------------------------------
#if UNITY_EDITOR
	[CustomEditor(typeof(RailDeformer))]
	public class RailDeformerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			var script = (RailDeformer)target;

			GUILayout.Space(10);
			EditorGUILayout.LabelField("Contrôles Manuels", EditorStyles.boldLabel);

			if (GUILayout.Button("Generate Rails (Manuel)"))
			{
				Undo.RecordObject(script, "Generate Rails");
				script.GenerateRails();
			}

			if (GUILayout.Button("Clear Rails (Manuel)"))
			{
				Undo.RecordObject(script, "Clear Rails");
				script.ClearRails();
			}
		}
	}
#endif
}
