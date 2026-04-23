using System.Collections;
using UnityEngine;

public class CoinCollectible : MonoBehaviour
{
    [Header("Score")]
    public int value = 20;

    [Header("Rotation")]
    public float rotateSpeed = 90f;

    [Header("Respawn")]
    public float respawnDelay = 3f;
    public float groundHalfSize = 4f;
    public float spawnHeight = 0.5f;

    // Les ennemis s'abonnent à cet événement pour se diriger vers le coin
    public static event System.Action<Vector3> OnCoinAppeared;

    private Renderer coinRenderer;
    private Collider coinCollider;

    void Start()
    {
        coinRenderer = GetComponent<Renderer>();
        coinCollider = GetComponent<Collider>();
        OnCoinAppeared?.Invoke(transform.position);
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager.Instance?.AddScore(value);
        StartCoroutine(RespawnAfterDelay());
    }

    IEnumerator RespawnAfterDelay()
    {
        if (coinRenderer != null) coinRenderer.enabled = false;
        if (coinCollider != null) coinCollider.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        float x = Random.Range(-groundHalfSize, groundHalfSize);
        float z = Random.Range(-groundHalfSize, groundHalfSize);
        transform.position = new Vector3(x, spawnHeight, z);

        if (coinRenderer != null) coinRenderer.enabled = true;
        if (coinCollider != null) coinCollider.enabled = true;

        // Prévenir les ennemis que le coin est réapparu
        OnCoinAppeared?.Invoke(transform.position);
    }
}
