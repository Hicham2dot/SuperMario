using UnityEngine;

// Pièce collectable : ajoute des points et réapparaît à une position aléatoire sur le plan
public class CoinCollectible : MonoBehaviour
{
    [Header("Score")]
    public int value = 20;

    [Header("Rotation")]
    public float rotateSpeed = 90f;

    [Header("Respawn")]
    public bool respawnOnCollect = true;    // true = réapparaît, false = disparaît définitivement
    public float spawnAreaSize = 8f;        // demi-côté de la zone de spawn (carré centré sur 0,0)
    public float spawnHeight = 0.5f;        // hauteur Y de la pièce après respawn

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager.Instance?.AddScore(value);
        Debug.Log($"[Pièce] Collectée ! +{value} points");

        if (respawnOnCollect)
            Respawn();
        else
            Destroy(gameObject);
    }

    void Respawn()
    {
        float x = Random.Range(-spawnAreaSize, spawnAreaSize);
        float z = Random.Range(-spawnAreaSize, spawnAreaSize);
        transform.position = new Vector3(x, spawnHeight, z);
        Debug.Log($"[Pièce] Respawn à ({x:F1}, {spawnHeight}, {z:F1})");
    }
}
