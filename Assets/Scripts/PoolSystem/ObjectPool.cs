using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
	public GameObject prefab; // Préfab à instancier
	public int initialSize = 10; // Taille initiale du pool

	private Queue<GameObject> pool = new Queue<GameObject>();

	void Start()
	{
		// Remplir le pool au démarrage
		for (int i = 0; i < initialSize; i++)
		{
			CreateNewObject();
		}
	}

	private void CreateNewObject()
	{
		GameObject obj = Instantiate(prefab, transform); // Parenté au GameObject du pool
		obj.SetActive(false); // Désactiver l'objet
		pool.Enqueue(obj); // Ajouter au pool
	}


	public GameObject GetObject()
	{
		if (pool.Count > 0)
		{
			GameObject obj = pool.Dequeue();
			obj.SetActive(true);
			Debug.Log($"[ObjectPool] Retrieved object from pool. Remaining in pool: {pool.Count}");
			return obj;
		}

		Debug.LogWarning("[ObjectPool] Pool is empty! No objects available.");
		return null;
	}


	public void ReturnObject(GameObject obj)
	{
		obj.SetActive(false);
		obj.transform.SetParent(transform); // Re-parenté au GameObject du pool
		pool.Enqueue(obj);
	}

}
