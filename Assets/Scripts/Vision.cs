using UnityEngine;

[ExecuteAlways]
public class Vision : MonoBehaviour
{
    public float radiusVision = 100f;

	// Update is called once per frame
	void Update()
    {
		Shader.SetGlobalFloat("_radiusVision", radiusVision);

		Shader.SetGlobalVector("_posUpdate",transform.position);
    }
}
