using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

[ExecuteAlways]
[RequireComponent(typeof(Rigidbody))]
public class SplineFollowerWithRigidbody : MonoBehaviour
{
	[Header("Spline Settings")]
	public SplineContainer splineContainer; // Le conteneur de spline � suivre
	public float speed = 5f; // Vitesse de d�placement sur la spline
	public float followHeightOffset = 0.5f; // D�calage de hauteur au-dessus du sol

	[Header("Fade Settings")]
	public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Courbe de fade in/out

	[Header("Light")]
	public Light light;

	private Rigidbody rb; // Le Rigidbody de l'objet
	private Material material; // Renderer pour acc�der au mat�riau
	private MaterialPropertyBlock propertyBlock; // Pour affecter les propri�t�s du mat�riau
	private float splinePosition = 0f; // Position actuelle sur la spline (0 � 1)

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		material = GetComponent<MeshRenderer>().material;

		if (splineContainer == null)
		{
			Debug.LogError("Spline Container n'est pas assign�.");
		}

		// D�sactiver la gravit� si l'objet est guid� uniquement par la spline
		rb.useGravity = false;
	}

	private void FixedUpdate()
	{
		if (splineContainer == null || rb == null) return;

		splinePosition += (speed * Time.fixedDeltaTime) / splineContainer.CalculateLength();
		if (splinePosition > 1f) splinePosition -= 1f;

		if (splineContainer.Spline.Evaluate(splinePosition, out float3 position, out float3 tangent, out float3 upVector))
		{
			Vector3 worldPosition = splineContainer.transform.TransformPoint((Vector3)position);
			if (Physics.Raycast(worldPosition + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f))
			{
				worldPosition = hit.point + Vector3.up * followHeightOffset;
			}

			Vector3 direction = new Vector3(worldPosition.x, 0, worldPosition.z) - rb.position;
			rb.MovePosition(rb.position + direction);

			Quaternion targetRotation = Quaternion.Euler(0f, Quaternion.LookRotation((Vector3)tangent, Vector3.up).eulerAngles.y, 0f);
			rb.MoveRotation(Quaternion.Lerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));

			// Calculer l'opacit�
			float opacity = fadeCurve.Evaluate(splinePosition);

			// Affecter l'opacit� au MaterialPropertyBlock
			material.SetFloat("_opactityVideo", opacity);
			light.intensity = opacity;
		}
	}
}
