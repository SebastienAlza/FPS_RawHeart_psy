using UnityEngine;

public class HeadBobOnCharacter : MonoBehaviour
{
	public FPSController fpsController;
	public Transform head;
	[Header("Paramètres de Balancement")]
	public float walkBobSpeed = 8.0f;
	public Vector2 walkBobAmount = new Vector2(0.1f,0.2f);
	public float runBobSpeed = 12.0f;
	public Vector2 runBobAmount = new Vector2(0.1f, 0.2f);

	[Header("Transition Douce")]
	public float resetSpeed = 5.0f;

	private Vector3 initialPosition;
	private float timer = 0.0f;

	private void Start()
	{
		initialPosition = head.transform.localPosition;
	}

	private void Update()
	{
		float speed = fpsController.CurrentSpeed;
		bool isRunning = speed > fpsController.moveSpeed + 1f; // Petite marge pour détecter le sprint

		if (speed > 0.1f)
		{
			float bobSpeed = isRunning ? runBobSpeed : walkBobSpeed;
			float bobAmountX = isRunning ? runBobAmount.x : walkBobAmount.x;
			float bobAmountY = isRunning ? runBobAmount.y : walkBobAmount.y;

			timer += Time.deltaTime * bobSpeed;
			float offsetY = Mathf.Sin(timer) * bobAmountY;
			float offsetX = Mathf.Cos(timer * 0.5f) * bobAmountX;

			head.transform.localPosition = initialPosition + new Vector3(offsetX, offsetY, 0);
		}
		else
		{
			timer = 0;
			head.transform.localPosition = Vector3.Lerp(head.transform.localPosition, initialPosition, Time.deltaTime * resetSpeed);
		}
	}
}
