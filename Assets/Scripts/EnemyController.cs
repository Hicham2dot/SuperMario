using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    [Header("Déplacement")]
    public float moveSpeed = 3f;
    public float chaseSpeedMultiplier = 1.5f;

    [Header("Comportement")]
    public float circleRadius = 2.8f;
    public float coinGuardDuration = 7f;   // secondes à garder la pièce
    [Range(0f, 1f)]
    public float coinGuardChance = 0.45f;  // probabilité qu'un ennemi aille garder la pièce

    [Header("Physique")]
    public float mass = 8f;

    [Header("Limites du sol")]
    public float groundHalfSize = 4.5f;
    public int totalEnemies = 8;
    public float groundOffset = 0f;

    [Header("Dégâts joueur")]
    public int damagePoints = 5;
    public float damageCooldown = 1.5f;

    private enum State { Chase, Circle, Pause, GuardCoin }

    private Rigidbody rb;
    private Transform player;
    private State currentState;
    private Vector3 moveDirection;
    private Vector3 spawnPosition;
    private Vector3 coinGuardPos;

    private float stateTimer;
    private float circleAngle;
    private float circleDir;

    private float lastDamageTime = -99f;
    private static int s_nextIndex = 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ResetSpawnIndex() => s_nextIndex = 0;

    void OnEnable()  => CoinCollectible.OnCoinAppeared += OnCoinAppeared;
    void OnDisable() => CoinCollectible.OnCoinAppeared -= OnCoinAppeared;

    void OnCoinAppeared(Vector3 coinPos)
    {
        // Chaque ennemi a une chance aléatoire d'aller bloquer la pièce
        if (Random.value < coinGuardChance)
        {
            coinGuardPos = new Vector3(coinPos.x, transform.position.y, coinPos.z);
            EnterState(State.GuardCoin);
        }
    }

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
        int col  = myIndex % cols;
        int row  = myIndex / cols;

        float cellW  = groundHalfSize * 2f / cols;
        float cellH  = groundHalfSize * 2f / rows;
        float spawnX = -groundHalfSize + cellW * (col + 0.5f) + Random.Range(-cellW * 0.2f, cellW * 0.2f);
        float spawnZ = -groundHalfSize + cellH * (row + 0.5f) + Random.Range(-cellH * 0.2f, cellH * 0.2f);

        float groundY = SnapToGround(spawnX, spawnZ);
        spawnPosition = new Vector3(spawnX, groundY + groundOffset, spawnZ);
        transform.position = spawnPosition;

        // Démarrage décalé pour éviter la synchronisation entre ennemis
        stateTimer = Random.Range(0f, 2f);
        EnterState(State.Chase);
    }

    void Update()
    {
        stateTimer -= Time.deltaTime;
        UpdateState();
    }

    void FixedUpdate()
    {
        ClampToBounds();

        float speed = currentState switch
        {
            State.Chase     => moveSpeed * chaseSpeedMultiplier,
            State.GuardCoin => moveSpeed,
            State.Pause     => 0f,
            _               => moveSpeed
        };

        rb.linearVelocity = new Vector3(moveDirection.x * speed, 0f, moveDirection.z * speed);
    }

    // ──────────────────────────────────────────────
    //  États
    // ──────────────────────────────────────────────

    void UpdateState()
    {
        switch (currentState)
        {
            case State.Chase:     UpdateChase();     break;
            case State.Circle:    UpdateCircle();    break;
            case State.Pause:     UpdatePause();     break;
            case State.GuardCoin: UpdateGuardCoin(); break;
        }
    }

    void UpdateChase()
    {
        if (player == null) return;

        // Fonce vers le joueur — sans limite de distance
        SetMoveDir(FlatDir(player.position - transform.position).normalized);

        if (stateTimer > 0f) return;

        float roll = Random.value;
        if (roll < 0.45f)     EnterState(State.Circle);
        else if (roll < 0.6f) EnterState(State.Pause);
        else                  EnterState(State.Chase);
    }

    void UpdateCircle()
    {
        if (player == null) { EnterState(State.Chase); return; }

        // Orbite autour du joueur
        circleAngle += circleDir * 80f * Time.deltaTime;
        Vector3 target = new(
            player.position.x + Mathf.Cos(circleAngle * Mathf.Deg2Rad) * circleRadius,
            0f,
            player.position.z + Mathf.Sin(circleAngle * Mathf.Deg2Rad) * circleRadius
        );

        Vector3 dir = FlatDir(target - transform.position);
        if (dir.sqrMagnitude > 0.01f)
            SetMoveDir(dir.normalized);

        if (stateTimer > 0f) return;

        float roll = Random.value;
        if (roll < 0.5f)      EnterState(State.Chase);
        else if (roll < 0.7f) EnterState(State.Pause);
        else                  EnterState(State.Circle);
    }

    void UpdatePause()
    {
        moveDirection = Vector3.zero;

        if (stateTimer > 0f) return;

        EnterState(Random.value < 0.65f ? State.Chase : State.Circle);
    }

    void UpdateGuardCoin()
    {
        Vector3 dir = FlatDir(coinGuardPos - transform.position);

        if (dir.magnitude > 0.4f)
        {
            // Se déplace vers la pièce
            SetMoveDir(dir.normalized);
        }
        else
        {
            // Sur la pièce : reste immobile pour bloquer le joueur
            moveDirection = Vector3.zero;
        }

        if (stateTimer > 0f) return;

        // Retour à la chasse après la durée de garde
        EnterState(State.Chase);
    }

    // ──────────────────────────────────────────────
    //  Transitions
    // ──────────────────────────────────────────────

    void EnterState(State next)
    {
        currentState = next;

        switch (next)
        {
            case State.Chase:
                stateTimer = Random.Range(1.5f, 3f);
                break;

            case State.Circle:
                stateTimer  = Random.Range(2f, 3.5f);
                circleAngle = Random.Range(0f, 360f);
                circleDir   = Random.value < 0.5f ? 1f : -1f;
                break;

            case State.Pause:
                stateTimer    = Random.Range(0.4f, 1.2f);
                moveDirection = Vector3.zero;
                break;

            case State.GuardCoin:
                stateTimer = coinGuardDuration + Random.Range(-1f, 2f);
                break;
        }
    }

    // ──────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────

    void SetMoveDir(Vector3 dir)
    {
        moveDirection = dir;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    static Vector3 FlatDir(Vector3 v) => new(v.x, 0f, v.z);

    static float SnapToGround(float x, float z)
    {
        return Physics.Raycast(new Vector3(x, 50f, z), Vector3.down, out RaycastHit hit, 100f)
               ? hit.point.y : 0f;
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
            // Rebond sur obstacle : changer de direction
            if (currentState != State.GuardCoin)
                EnterState(State.Chase);
            return;
        }

        if (Time.time - lastDamageTime >= damageCooldown)
        {
            lastDamageTime = Time.time;
            GameManager.Instance?.AddScore(-damagePoints);
        }
    }
}
