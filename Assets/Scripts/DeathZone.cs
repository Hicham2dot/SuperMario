using UnityEngine;

// Attach to an invisible plane below the level — kills Mario if he falls
public class DeathZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            GameManager.Instance?.LoseLife();
    }
}
