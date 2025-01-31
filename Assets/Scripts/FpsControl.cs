using UnityEngine;

public class FPSController : MonoBehaviour
{
	[Header("Rotation Settings")]
	public Transform HeadTr;

	[Header("Movement Settings")]
	public float moveSpeed = 5f;
	public float sprintSpeed = 8f;
	public float jumpForce = 5f;
	public float gravity = -9.8f;

	[Header("Mouse Settings")]
	public float mouseSensitivity = 100f;
	public float maxLookAngle = 85f;

	private CharacterController controller;
	private Vector3 velocity;
	private float verticalLookRotation = 0f;
	private Vector3 previousPosition;

	// Propriété publique pour accéder à la vitesse actuelle
	public float CurrentSpeed { get; private set; }

	private void Start()
	{
		controller = GetComponent<CharacterController>();
		previousPosition = transform.position; // Position initiale
		Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
	}

	private void Update()
	{
		HandleMovement();
		HandleMouseLook();
		ApplyGravity();

		// Calculer la vitesse en fonction de la différence de position
		Vector3 horizontalDisplacement = transform.position - previousPosition;
		horizontalDisplacement.y = 0; // Ignorer la composante verticale
		float calculatedSpeed = horizontalDisplacement.magnitude / Time.deltaTime;
		previousPosition = transform.position; // Mettre à jour la position précédente

		//Debug.Log("Vitesse calculée manuellement : " + calculatedSpeed);

		CurrentSpeed = calculatedSpeed;
	}


	private void HandleMovement()
	{
		float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;

		float moveX = Input.GetAxis("Horizontal");
		float moveZ = Input.GetAxis("Vertical");

		Vector3 move = transform.right * moveX + transform.forward * moveZ;
		controller.Move(move * speed * Time.deltaTime);

		if (controller.isGrounded && Input.GetButtonDown("Jump"))
		{
			velocity.y = jumpForce;
		}
	}

	private void HandleMouseLook()
	{
		float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
		float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

		// Rotate player horizontally
		transform.Rotate(Vector3.up * mouseX);

		// Rotate camera vertically
		verticalLookRotation -= mouseY;
		verticalLookRotation = Mathf.Clamp(verticalLookRotation, -maxLookAngle, maxLookAngle);
		HeadTr.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);
	}

	private void ApplyGravity()
	{
		if (controller.isGrounded && velocity.y < 0)
		{
			velocity.y = -2f; // Small value to ensure the player stays grounded
		}

		velocity.y += gravity * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);
	}
}
