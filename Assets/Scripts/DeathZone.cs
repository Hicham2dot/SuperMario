using UnityEngine;

// Zone de mort placée sous le sol — déclenche un Game Over si le joueur y tombe
// Le Collider doit être en mode Trigger et plus grand que le sol
public class DeathZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("[DeathZone] Le joueur est tombé — Game Over !");
            GameManager.Instance?.GameOver();
        }
    }
}
