using UnityEngine;

// Caméra 3D qui suit le joueur avec un offset fixe (vue de dessus légèrement inclinée)
public class CameraController : MonoBehaviour
{
    [Header("Cible")]
    public Transform target;

    [Header("Offset (position relative au joueur)")]
    public float offsetX = 0f;
    public float offsetY = 10f;     // hauteur de la caméra au-dessus du joueur
    public float offsetZ = -7f;     // recul de la caméra derrière le joueur

    [Header("Lissage")]
    public float smoothSpeed = 6f;  // plus la valeur est élevée, plus le suivi est réactif

    private Vector3 offset;

    void Start()
    {
        offset = new Vector3(offsetX, offsetY, offsetZ);

        // Positionner immédiatement sans lissage au démarrage
        if (target != null)
            transform.position = target.position + offset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        // Interpolation douce vers la position cible
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // La caméra regarde toujours vers le joueur
        transform.LookAt(target);
    }
}
