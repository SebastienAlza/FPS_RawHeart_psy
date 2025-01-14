using UnityEngine;

public class ObjectPoolDebug : MonoBehaviour
{
	public ObjectPool objectPool;

	private int activeCount = 0;
	private int inactiveCount = 0;

	void Start()
	{
		if (objectPool == null)
		{
			objectPool = GetComponent<ObjectPool>();
			if (objectPool == null)
			{
				Debug.LogError("No ObjectPool assigned or found on this GameObject!");
			}
		}
	}

	void OnGUI()
	{
		if (objectPool == null)
		{
			GUI.Label(new Rect(10, 10, 300, 20), "No ObjectPool assigned for debugging!");
			return;
		}

		// Compter les objets
		CountPoolObjects();

		// Afficher les informations
		GUI.Label(new Rect(10, 10, 300, 20), $"Pool Debug: {objectPool.prefab.name}");
		GUI.Label(new Rect(10, 30, 300, 20), $"Active Objects: {activeCount}");
		GUI.Label(new Rect(10, 50, 300, 20), $"Inactive Objects: {inactiveCount}");
		GUI.Label(new Rect(10, 70, 300, 20), $"Total Objects: {activeCount + inactiveCount}");
	}

	private void CountPoolObjects()
	{
		// Réinitialiser les compteurs
		activeCount = 0;
		inactiveCount = 0;

		// Parcourir les objets enfants (ou directement dans la queue du pool si nécessaire)
		foreach (Transform child in objectPool.transform)
		{
			if (child.gameObject.activeInHierarchy)
			{
				activeCount++;
			}
			else
			{
				inactiveCount++;
			}
		}
	}
}
