using System.Collections.Generic;
using UnityEngine;

public class MeshCombiner
{
	private int maxTriangles;
	private string meshName;
	private int currentTriangleCount = 0;

	// Regroupe par matériau => Material -> liste de CombineInstance
	private Dictionary<Material, List<CombineInstance>> matToCombines
		= new Dictionary<Material, List<CombineInstance>>();

	public MeshCombiner(int maxTris, string name)
	{
		maxTriangles = maxTris;
		meshName = name;
	}

	/// <summary>
	/// Ajoute un segment (mesh + transform + tableau de mats).
	/// On découpe chaque submesh et on l'associe au dictionnaire par matériau.
	/// </summary>
	public bool TryAddSegment(Mesh mesh, Matrix4x4 transform, Material[] materials)
	{
		if (mesh == null || materials == null) return false;

		int triCount = mesh.triangles.Length / 3;
		if (currentTriangleCount + triCount > maxTriangles)
			return false;

		int subCount = mesh.subMeshCount;
		for (int subIndex = 0; subIndex < subCount; subIndex++)
		{
			Material mat = (subIndex < materials.Length) ? materials[subIndex] : null;
			if (mat == null) continue;

			// Extraire ce submesh en un mini-mesh
			Mesh subMesh = ExtractSubMesh(mesh, subIndex);

			CombineInstance ci = new CombineInstance
			{
				mesh = subMesh,
				transform = transform
			};

			// Ajouter au dico
			if (!matToCombines.TryGetValue(mat, out var listCI))
			{
				listCI = new List<CombineInstance>();
				matToCombines[mat] = listCI;
			}
			listCI.Add(ci);
		}

		currentTriangleCount += triCount;
		return true;
	}

	/// <summary>
	/// Construit le mesh final en fusionnant tous les submeshes qui partagent le même mat.
	/// Retourne un mesh multi-submeshes (1 par matériau) + le tableau de mats correspondants.
	/// </summary>
	public Mesh Build(out Material[] finalMaterials)
	{
		finalMaterials = null;
		if (matToCombines.Count == 0) return null;

		// On combine par matériau unique => partialMeshes + matList
		List<Mesh> partialMeshes = new List<Mesh>();
		List<Material> matList = new List<Material>();

		foreach (var kvp in matToCombines)
		{
			Material mat = kvp.Key;
			List<CombineInstance> ciList = kvp.Value;

			// Combine tous les mini-meshes pour ce matériau
			Mesh partMesh = new Mesh();
			// mergeSubMeshes = true => on regroupe tout en un seul submesh
			partMesh.CombineMeshes(ciList.ToArray(), mergeSubMeshes: true, useMatrices: true);

			partialMeshes.Add(partMesh);
			matList.Add(mat);
		}

		// Maintenant on a 1 partialMesh par matériau
		// => on assemble tout dans un mesh final multi-submeshes
		CombineInstance[] finalCombine = new CombineInstance[partialMeshes.Count];
		for (int i = 0; i < partialMeshes.Count; i++)
		{
			finalCombine[i].mesh = partialMeshes[i];
			finalCombine[i].transform = Matrix4x4.identity;
		}

		Mesh finalMesh = new Mesh();
		finalMesh.name = meshName;
		// mergeSubMeshes = false => 1 submesh par partial
		finalMesh.CombineMeshes(finalCombine, mergeSubMeshes: false, useMatrices: true);

		finalMesh.RecalculateBounds();

		finalMaterials = matList.ToArray();
		return finalMesh;
	}

	/// <summary>
	/// Indique si on n'a encore rien.
	/// </summary>
	public bool IsEmpty()
	{
		return matToCombines.Count == 0;
	}

	/// <summary>
	/// Extrait un submesh (subIndex) d'un mesh source en un mini Mesh indépendant.
	/// </summary>
	private Mesh ExtractSubMesh(Mesh source, int subIndex)
	{
		Mesh m = new Mesh();
		m.name = source.name + "_sub" + subIndex;

		int[] indices = source.GetTriangles(subIndex);
		Vector3[] allVerts = source.vertices;
		Vector2[] allUV = source.uv;
		Vector3[] allNormals = source.normals;

		m.vertices = allVerts;
		if (allUV != null && allUV.Length == allVerts.Length) m.uv = allUV;
		if (allNormals != null && allNormals.Length == allVerts.Length) m.normals = allNormals;

		m.SetTriangles(indices, 0);

		m.RecalculateBounds();
		return m;
	}
}
