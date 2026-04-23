using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    [Header("Déplacement")]
    public float moveSpeed = 3f;
    public float chaseSpeedMultiplier = 1.5f;

    [Header("Détection joueur")]
    public float detectionRange = 5.5f;
    public float circleRadius = 2.8f;

    [Header("Physique")]
    public float mass = 8f;

    [Header("Limites du sol")]
    public float groundHalfSize = 4.5f;
    public int totalEnemies = 8;

    [Header("Dégâts joueur")]
    public int damagePoints = 5;
    public float damageCooldown = 1.5f;

    private enum State { Patrol, Chase, Circle, Pause }

    private Rigidbody rb;
    private Transform player;
    private State currentState;
    private Vector3 moveDirection;
    private Vector3 spawnPosition;

    private float stateTimer;
    private float circleAngle;
    private float circleDir;    // +1 ou -1 : sens d'orbite

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

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // Distribution sur le ground
        int myIndex = s_nextIndex++;
        int cols = Mathf.CeilToInt(Mathf.Sqrt(totalEnemies));
        int rows = Mathf.CeilToInt((float)totalEnemies / cols);
        int col = myIndex % cols;
        int row = myIndex / cols;

        float cellW = groundHalfSize * 2f / cols;
        float cellH = groundHalfSize * 2f / rows;
        float spawnX = -groundHalfSize + cellW * (col + 0.5f) + Random.Range(-cellW * 0.2f, cellW * 0.2f);
        float spawnZ = -groundHalfSize + cellH * (row + 0.5f) + Random.Range(-cellH * 0.2f, cellH * 0.2f);

        spawnPosition = new Vector3(spawnX, 0.5f, spawnZ);
        transform.position = spawnPosition;

        // Démarrage décalé pour que tous les ennemis ne soient pas synchronisés
        stateTimer = Random.Range(0f, 1f);
        EnterState(State.Patrol);
    }

    void Update()
    {
        stateTimer -= Time.deltaTime;

        float distToPlayer = DistToPlayer();
        UpdateState(distToPlayer);
    }

    void FixedUpdate()
    {
        ClampToBounds();

        float speed = currentState switch
        {
            State.Chase  => moveSpeed * chaseSpeedMultiplier,
            State.Pause  => 0f,
            _            => moveSpeed
        };

        rb.linearVelocity = new Vector3(moveDirection.x * speed, 0f, moveDirection.z * speed);
    }

    // ──────────────────────────────────────────────
    //  États
    // ──────────────────────────────────────────────

    void UpdateState(float dist)
    {
        switch (currentState)
        {
            case State.Patrol: UpdatePatrol(dist);  break;
            case State.Chase:  UpdateChase(dist);   break;
            case State.Circle: UpdateCircle(dist);  break;
            case State.Pause:  UpdatePause(dist);   break;
        }
    }

    void UpdatePatrol(float dist)
    {
        // Détection immédiate si le joueur est très proche
        if (dist < detectionRange * 0.5f)
        {
            EnterState(State.Chase);
            return;
        }

        if (stateTimer > 0f) return;

        // Quand le timer expire : décision aléatoire
        if (dist < detectionRange)
        {
            float roll = Random.value;
            if (roll < 0.55f)      EnterState(State.Chase);
            else if (roll < 0.75f) EnterState(State.Pause);
            else                   PickPatrolDirection();
        }
        else
        {
            PickPatrolDirection();
        }
    }

    void UpdateChase(float dist)
    {
        if (player == null) { EnterState(State.Patrol); return; }

        // Diriger vers le joueur en permanence
        Vector3 dir = FlatDir(player.position - transform.position);
        if (dir.sqrMagnitude > 0.01f)
            SetMoveDir(dir.normalized);

        // Joueur trop loin → abandon
        if (dist > detectionRange * 1.6f)
        {
            EnterState(State.Patrol);
            return;
        }

        if (stateTimer > 0f) return;

        // Transition aléatoire après le timer
        float roll = Random.value;
        if (roll < 0.45f)      EnterState(State.Circle);
        else if (roll < 0.65f) EnterState(State.Pause);
        else                   EnterState(State.Chase);   // reprend la chasse
    }

    void UpdateCircle(float dist)
    {
        if (player == null) { EnterState(State.Patrol); return; }

        // Orbite autour du joueur
        circleAngle += circleDir * 80f * Time.deltaTime;
        float rad = circleAngle * Mathf.Deg2Rad;
        Vector3 target = new(
            player.position.x + Mathf.Cos(rad) * circleRadius,
            0f,
            player.position.z + Mathf.Sin(rad) * circleRadius
        );

        Vector3 dir = FlatDir(target - transform.position);
        if (dir.sqrMagnitude > 0.01f)
            SetMoveDir(dir.normalized);

        if (dist > detectionRange * 1.6f)
        {
            EnterState(State.Patrol);
            return;
        }

        if (stateTimer > 0f) return;

        float roll = Random.value;
        if (roll < 0.5f)       EnterState(State.Chase);
        else if (roll < 0.75f) EnterState(State.Pause);
        else                   EnterState(State.Circle);  // inverse le sens
    }

    void UpdatePause(float dist)
    {
        moveDirection = Vector3.zero;

        if (stateTimer > 0f) return;

        if (dist < detectionRange)
            EnterState(Random.value < 0.65f ? State.Chase : State.Circle);
        else
            EnterState(State.Patrol);
    }

    // ──────────────────────────────────────────────
    //  Transitions
    // ──────────────────────────────────────────────

    void EnterState(State next)
    {
        currentState = next;

        switch (next)
        {
            case State.Patrol:
                stateTimer = Random.Range(2f, 4f);
                PickPatrolDirection();
                break;

            case State.Chase:
                stateTimer = Random.Range(1.5f, 3f);
                break;

            case State.Circle:
                stateTimer = Random.Range(2f, 3.5f);
                circleAngle = Random.Range(0f, 360f);
                circleDir   = Random.value < 0.5f ? 1f : -1f;
                break;

            case State.Pause:
                stateTimer = Random.Range(0.4f, 1.2f);
                moveDirection = Vector3.zero;
                break;
        }
    }

    // ──────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────

    void PickPatrolDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        SetMoveDir(new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)));
        stateTimer = Random.Range(1.5f, 3f);
    }

    void SetMoveDir(Vector3 dir)
    {
        moveDirection = dir;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    static Vector3 FlatDir(Vector3 v) => new(v.x, 0f, v.z);

    float DistToPlayer()
    {
        if (player == null) return float.MaxValue;
        return Vector3.Distance(
            new Vector3(transform.position.x, 0f, transform.position.z),
            new Vector3(player.position.x,    0f, player.position.z));
    }

    void ClampToBounds()
    {
        Vector3 pos = transform.position;

        if (Mathf.Abs(pos.x) > groundHalfSize - 0.3f)
            moveDirection.x = -Mathf.Sign(pos.x) * Mathf.Abs(moveDirection.x < 0.1f ? 0.5f : moveDirection.x);
        if (Mathf.Abs(pos.z) > groundHalfSize - 0.3f)
            moveDirection.z = -Mathf.Sign(pos.z) * Mathf.Abs(moveDirection.z < 0.1f ? 0.5f : moveDirection.z);

        pos.x = Mathf.Clamp(pos.x, -groundHalfSize, groundHalfSize);
        pos.z = Mathf.Clamp(pos.z, -groundHalfSize, groundHalfSize);
        transform.position = pos;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Player"))
        {
            PickPatrolDirection();
            return;
        }

        if (Time.time - lastDamageTime >= damageCooldown)
        {
            lastDamageTime = Time.time;
            GameManager.Instance?.AddScore(-damagePoints);
        }
    }
}
