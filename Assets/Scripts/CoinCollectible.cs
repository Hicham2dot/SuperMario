using UnityEngine;

public class CoinCollectible : MonoBehaviour
{
    public int value = 10;
    public float rotateSpeed = 90f;

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance?.AddScore(value);
            Destroy(gameObject);
        }
    }
}
