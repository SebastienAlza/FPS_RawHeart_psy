using UnityEngine;

[ExecuteAlways]
public class test : MonoBehaviour
{
    public float alphavideo;
    private Material material;
    private MaterialPropertyBlock materialPropertyBlock;
	MeshRenderer meshRenderer;

	void Start()
    {
        material = GetComponent<MeshRenderer>().material;
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
		materialPropertyBlock = new MaterialPropertyBlock();

	}

    // Update is called once per frame
    void Update()
    {
		//material.SetFloat("_opactityVideo", alphavideo);

		meshRenderer.GetPropertyBlock(materialPropertyBlock); // R�cup�re les propri�t�s actuelles
		materialPropertyBlock.SetFloat("_opacityVideo", alphavideo); // Change la valeur de la propri�t�
		meshRenderer.SetPropertyBlock(materialPropertyBlock);
	}
}
