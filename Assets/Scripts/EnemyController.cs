using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float patrolDistance = 3f;

    private Vector3 startPosition;
    private int direction = 1;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        Patrol();
    }

    void Patrol()
    {
        transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime);

        float distanceTraveled = transform.position.x - startPosition.x;

        if (distanceTraveled >= patrolDistance || distanceTraveled <= -patrolDistance)
        {
            direction *= -1;
            transform.localScale = new Vector3(-direction, 1f, 1f);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        // Check if player jumped on top of enemy
        bool playerAbove = collision.contacts[0].normal.y > 0.5f;

        if (playerAbove)
        {
            GameManager.Instance?.AddScore(100);
            Destroy(gameObject);
        }
        else
        {
            GameManager.Instance?.LoseLife();
        }
    }
}
