using UnityEngine;
using UnityEngine.UI;

public class FPSGunRetro : MonoBehaviour
{
    [Header("Gun Settings")]
    public float damage = 10f; // Dégâts infligés par le tir
    public float range = 50f; // Portée maximale
    public float fireRate = 0.2f; // Cadence de tir
    public LayerMask targetLayer; // Couches détectées par le tir
    public Camera playerCamera; // Caméra du joueur pour viser

    [Header("Crosshair Settings")]
    public Image crosshair; // Image du viseur
    public Color defaultColor = Color.white; // Couleur normale
    public Color shootColor = Color.red; // Couleur lors du tir
    public Color hitColor = Color.green; // Couleur lorsqu'un ennemi est touché
    public float flashDuration = 0.1f; // Durée du flash
    public float scaleIncrease = 1.2f; // Agrandissement temporaire du viseur

    [Header("Effects")]
    public GameObject hitEffectPrefab; // Effet visuel à l’impact (optionnel)
    public AudioSource gunSound; // Son du tir (optionnel)

    private float nextFireTime = 0f;
    private Vector3 originalScale;

    void Start()
    {
        if (crosshair != null)
        {
            originalScale = crosshair.transform.localScale;
            crosshair.color = defaultColor;
        }
    }

    void Update()
    {
        HandleShooting();
    }

    private void HandleShooting()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        // Effet sonore du tir
        if (gunSound != null)
        {
            gunSound.Play();
        }

        // Gestion du raycast pour le tir
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Centre de l’écran
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range, targetLayer))
        {
            Debug.Log("Hit: " + hit.collider.name);

            // Vérifie si l'objet touché est un ennemi
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                // Feedback du viseur pour un hit réussi
                FlashCrosshair(hitColor);
            }

            // Effet visuel d'impact
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
        else
        {
            // Feedback visuel lors du tir sans toucher d'ennemi
            FlashCrosshair(shootColor);
        }
    }

    private void FlashCrosshair(Color flashColor)
    {
        if (crosshair == null) return;

        StopAllCoroutines(); // Arrête les feedbacks en cours
        StartCoroutine(CrosshairFeedback(flashColor));
    }

    private System.Collections.IEnumerator CrosshairFeedback(Color flashColor)
    {
        // Changer la couleur et la taille du viseur
        crosshair.color = flashColor;
        crosshair.transform.localScale = originalScale * scaleIncrease;

        yield return new WaitForSeconds(flashDuration);

        // Rétablir la couleur et la taille initiales
        crosshair.color = defaultColor;
        crosshair.transform.localScale = originalScale;
    }
}
