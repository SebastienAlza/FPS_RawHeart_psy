using UnityEngine;
using UnityEngine.Splines;
using UnityEditor;

[ExecuteAlways]
public class RailDeformer : MonoBehaviour
{
	public SplineContainer splineContainer; // Référence à la spline
	public GameObject railPrefab;           // Prefab du rail
	public float segmentLength = 2f;        // Longueur d'un segment
	public bool loop = false;               // Rails bouclés ?
	public bool deformRails = true;         // Déformer dynamiquement les rails ?

	private void OnValidate()
	{
		// Regénérer les rails dans l'éditeur à chaque modification des paramètres
		if (!Application.isPlaying)
		{
			ClearRails();
			if (splineContainer != null && railPrefab != null)
			{
				GenerateRails();
			}
		}
	}

	void GenerateRails()
	{
		// Obtenir la longueur totale de la spline
		float splineLength = splineContainer.CalculateLength();
		float progress = 0f;

		// Instancier des rails le long de la spline
		while (progress < splineLength)
		{
			// Proportion sur la spline (0 à 1)
			float startT = progress / splineLength;
			float endT = Mathf.Min((progress + segmentLength) / splineLength, 1f);

			// Position moyenne sur la spline
			Vector3 position = splineContainer.Spline.EvaluatePosition((startT + endT) / 2);

			// Calculer la rotation avec la tangente de la spline
			Vector3 tangent = splineContainer.Spline.EvaluateTangent((startT + endT) / 2);
			Quaternion rotation = Quaternion.LookRotation(tangent);

			// Créer un rail
			GameObject rail = Instantiate(railPrefab, position, rotation, transform);

			// Marquer comme objet temporaire pour l'éditeur
			rail.hideFlags = HideFlags.NotEditable | HideFlags.HideInHierarchy;

			// Déformer le mesh si activé
			if (deformRails)
			{
				DeformRailMesh(rail, startT, endT);
			}

			// Avancer dans la spline
			progress += segmentLength;
		}

		// Si en boucle, connecter le dernier segment au premier
		if (loop)
		{
			GameObject firstRail = transform.GetChild(0).gameObject;
			GameObject lastRail = transform.GetChild(transform.childCount - 1).gameObject;
			lastRail.transform.position = firstRail.transform.position;
		}
	}

	void DeformRailMesh(GameObject rail, float startT, float endT)
	{
		MeshFilter meshFilter = rail.GetComponent<MeshFilter>();
		if (meshFilter == null)
		{
			Debug.LogWarning($"Le prefab {rail.name} n'a pas de MeshFilter !");
			return;
		}

		Mesh mesh = meshFilter.sharedMesh;
		Vector3[] vertices = mesh.vertices;

		for (int i = 0; i < vertices.Length; i++)
		{
			// Position relative dans le mesh (local)
			float t = Mathf.InverseLerp(0, 1, vertices[i].z);

			// Recalculer la position sur la spline
			Vector3 splinePosition = splineContainer.Spline.EvaluatePosition(Mathf.Lerp(startT, endT, t));

			// Ajuster les positions locales du mesh
			vertices[i] = rail.transform.InverseTransformPoint(splinePosition);
		}

		// Appliquer les nouvelles positions
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
	}

	void ClearRails()
	{
		// Supprimer tous les enfants créés dynamiquement
		for (int i = transform.childCount - 1; i >= 0; i--)
		{
			DestroyImmediate(transform.GetChild(i).gameObject);
		}
	}
}
