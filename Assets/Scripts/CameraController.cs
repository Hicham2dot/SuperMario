using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;

    [Header("Offset")]
    public float offsetX = 3f;    // look ahead in movement direction
    public float offsetY = 2f;
    public float offsetZ = -10f;

    [Header("Smoothing")]
    public float smoothX = 6f;
    public float smoothY = 4f;

    [Header("Bounds")]
    public float minX = 0f;       // camera never scrolls left of this point
    public bool useBounds = true;

    private float currentOffsetX;
    private int facingDirection = 1;

    void Start()
    {
        currentOffsetX = offsetX;
        if (target != null)
            transform.position = new Vector3(target.position.x + offsetX, target.position.y + offsetY, offsetZ);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Detect facing direction from player scale
        float scaleX = target.localScale.x;
        if (scaleX != 0f)
            facingDirection = scaleX > 0 ? 1 : -1;

        // Smoothly shift look-ahead based on direction
        currentOffsetX = Mathf.Lerp(currentOffsetX, offsetX * facingDirection, smoothX * Time.deltaTime);

        float targetX = target.position.x + currentOffsetX;
        float targetY = target.position.y + offsetY;

        // Clamp so camera never goes back left (Mario style)
        if (useBounds)
            targetX = Mathf.Max(targetX, minX);

        float newX = Mathf.Lerp(transform.position.x, targetX, smoothX * Time.deltaTime);
        float newY = Mathf.Lerp(transform.position.y, targetY, smoothY * Time.deltaTime);

        transform.position = new Vector3(newX, newY, offsetZ);
    }
}
