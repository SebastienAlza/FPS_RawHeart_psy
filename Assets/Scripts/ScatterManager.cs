using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ScatterManager : MonoBehaviour
{
    [Header("Scatter Settings")]
    public GameObject[] prefabsToScatter; // Liste des prefabs à disperser
    public Vector2 scatterAreaSize = new Vector2(10, 10); // Taille de la zone de dispersion (x, z)
    public int numberOfObjects = 50; // Nombre d'objets à disperser

    [Header("Randomization")]
    public bool randomRotation = true; // Rotation aléatoire des objets
    public Vector2 randomScaleRange = new Vector2(1f, 1.5f); // Plage d'échelle aléatoire (min, max)

    [Header("Parenting")]
    public bool parentToScatterManager = true; // Les objets sont-ils enfants de ce GameObject ?

    private List<GameObject> scatteredObjects = new List<GameObject>(); // Liste des objets dispersés

	private void Start()
	{
        if(Application.isPlaying) Scatter();
	}
	public void Scatter()
    {
        // Supprimer les objets existants si on relance le scatter
        ClearScatter();

        for (int i = 0; i < numberOfObjects; i++)
        {
            // Choisir un prefab aléatoire
            GameObject prefab = prefabsToScatter[Random.Range(0, prefabsToScatter.Length)];
            if (prefab == null) continue;

            // Générer une position aléatoire dans la zone de dispersion
            Vector3 position = new Vector3(
                Random.Range(-scatterAreaSize.x / 2f, scatterAreaSize.x / 2f),
                0f, // Assumer un placement au sol
                Random.Range(-scatterAreaSize.y / 2f, scatterAreaSize.y / 2f)
            );

            // Instancier l'objet
            GameObject scatteredObject = Instantiate(prefab, transform.position + position, Quaternion.identity);

            // Appliquer une rotation aléatoire si activé
            if (randomRotation)
            {
                scatteredObject.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            }

            // Appliquer une échelle aléatoire
            float randomScale = Random.Range(randomScaleRange.x, randomScaleRange.y);
            scatteredObject.transform.localScale = Vector3.one * randomScale;

            // Optionnel : Rendre l'objet enfant de ce GameObject
            if (parentToScatterManager)
            {
                scatteredObject.transform.parent = this.transform;
            }

            // Ajouter l'objet à la liste
            scatteredObjects.Add(scatteredObject);
        }
    }

    public void ClearScatter()
    {
        // Supprimer tous les objets dispersés
        foreach (GameObject obj in scatteredObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        scatteredObjects.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        // Dessiner la zone de dispersion dans l'éditeur
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(scatterAreaSize.x, 0, scatterAreaSize.y));
    }
}
