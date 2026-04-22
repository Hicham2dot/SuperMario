using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    [Header("Déplacement")]
    public float moveSpeed = 3f;
    public float changeDirectionInterval = 2f;

    [Header("Physique")]
    public float mass = 8f;

    [Header("Limites du sol")]
    public float groundHalfSize = 4.5f;
    public int totalEnemies = 8;

    [Header("Dégâts joueur")]
    public int damagePoints = 5;
    public float damageCooldown = 1.5f;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private float directionTimer;
    private float lastDamageTime = -99f;

    private static int s_nextIndex = 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ResetSpawnIndex() => s_nextIndex = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = mass;

        rb.constraints = RigidbodyConstraints.FreezePositionY
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;

        // Spawn en grille : chaque ennemi occupe une cellule unique → couverture totale du sol
        int myIndex = s_nextIndex++;
        int cols = Mathf.CeilToInt(Mathf.Sqrt(totalEnemies));
        int rows = Mathf.CeilToInt((float)totalEnemies / cols);
        int col = myIndex % cols;
        int row = myIndex / cols;

        float cellW = groundHalfSize * 2f / cols;
        float cellH = groundHalfSize * 2f / rows;

        float spawnX = -groundHalfSize + cellW * (col + 0.5f) + Random.Range(-cellW * 0.3f, cellW * 0.3f);
        float spawnZ = -groundHalfSize + cellH * (row + 0.5f) + Random.Range(-cellH * 0.3f, cellH * 0.3f);

        transform.position = new Vector3(spawnX, 0.5f, spawnZ);

        PickNewDirection();
    }

    void Update()
    {
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0f)
            PickNewDirection();
    }

    void FixedUpdate()
    {
        CheckBounds();

        // Déplacement uniquement sur X et Z (Y est gelé par les contraintes)
        rb.linearVelocity = new Vector3(
            moveDirection.x * moveSpeed,
            0f,
            moveDirection.z * moveSpeed
        );
    }

    // Vérifie si l'ennemi approche du bord du Plane et retourne dans la direction opposée
    void CheckBounds()
    {
        Vector3 pos = transform.position;
        bool hitX = Mathf.Abs(pos.x) >= groundHalfSize;
        bool hitZ = Mathf.Abs(pos.z) >= groundHalfSize;

        if (hitX)
        {
            // Réfléchir sur l'axe X
            moveDirection.x = -Mathf.Sign(pos.x) * Mathf.Abs(moveDirection.x == 0f ? 1f : moveDirection.x);
            moveDirection = moveDirection.normalized;

            // Ramener l'ennemi à l'intérieur du sol
            pos.x = Mathf.Clamp(pos.x, -groundHalfSize, groundHalfSize);
            transform.position = pos;
        }

        if (hitZ)
        {
            // Réfléchir sur l'axe Z
            moveDirection.z = -Mathf.Sign(pos.z) * Mathf.Abs(moveDirection.z == 0f ? 1f : moveDirection.z);
            moveDirection = moveDirection.normalized;

            pos.z = Mathf.Clamp(pos.z, -groundHalfSize, groundHalfSize);
            transform.position = pos;
        }

        if (hitX || hitZ)
            transform.rotation = Quaternion.LookRotation(moveDirection);
    }

    void PickNewDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        directionTimer = changeDirectionInterval + Random.Range(-0.5f, 0.5f);

        if (moveDirection.magnitude > 0.1f)
            transform.rotation = Quaternion.LookRotation(moveDirection);
    }

    void OnCollisionEnter(Collision collision)
    {
        PickNewDirection();

        if (!collision.collider.CompareTag("Player")) return;

        if (Time.time - lastDamageTime >= damageCooldown)
        {
            lastDamageTime = Time.time;
            GameManager.Instance?.AddScore(-damagePoints);
            Debug.Log($"[Ennemi] Collision joueur — -{damagePoints} pts");
        }
    }
}
