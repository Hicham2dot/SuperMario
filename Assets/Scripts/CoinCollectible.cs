using System.Collections;
using UnityEngine;

public class CoinCollectible : MonoBehaviour
{
    [Header("Score")]
    public int value = 30;

    [Header("Rotation")]
    public float rotateSpeed = 90f;

    [Header("Respawn")]
    public float respawnDelay = 3f;
    public float groundHalfSize = 4f;
    public float groundOffset = 1f;   // ajuster si la pièce flotte encore

    // Les ennemis s'abonnent à cet événement pour se diriger vers le coin
    public static event System.Action<Vector3> OnCoinAppeared;

    [Header("Visuel enfant")]
    public GameObject coinVisual;   // glisser "Gold Coin" ici dans l'Inspector

    private Renderer coinRenderer;
    private Collider coinCollider;

    void Start()
    {
        // Si coinVisual non assigné, chercher automatiquement le premier enfant
        if (coinVisual == null && transform.childCount > 0)
            coinVisual = transform.GetChild(0).gameObject;

        coinRenderer = coinVisual != null ? coinVisual.GetComponent<Renderer>() : null;
        coinCollider = GetComponent<Collider>();

        // Cacher le renderer du parent s'il en a un
        Renderer parentRenderer = GetComponent<Renderer>();
        if (parentRenderer != null) parentRenderer.enabled = false;

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
        float groundY = Physics.Raycast(new Vector3(x, 50f, z), Vector3.down, out RaycastHit hit, 100f)
                        ? hit.point.y : 0f;
        transform.position = new Vector3(x, groundY + groundOffset, z);

        if (coinRenderer != null) coinRenderer.enabled = true;
        if (coinCollider != null) coinCollider.enabled = true;

        // Prévenir les ennemis que le coin est réapparu
        OnCoinAppeared?.Invoke(transform.position);
    }
}
