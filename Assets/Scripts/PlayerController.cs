using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 7f;
    public float acceleration = 40f;
    public float deceleration = 35f;

    [Header("Auto Run")]
    public bool autoRun = true;        // le joueur avance tout seul vers la droite
    public float autoRunSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 14f;
    public float jumpHoldMultiplier = 2.5f;   // gravity reduction while holding jump
    public float fallMultiplier = 3.5f;        // faster fall when descending
    public float maxFallSpeed = -20f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    // Coyote time : can still jump briefly after walking off a ledge
    private float coyoteTime = 0.12f;
    private float coyoteTimer;

    // Jump buffer : jump input registered slightly before landing
    private float jumpBufferTime = 0.12f;
    private float jumpBufferTimer;

    private Rigidbody rb;
    private Animator animator;
    private bool isGrounded;
    private bool wasGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;

        // Crée le groundCheck automatiquement s'il n'est pas assigné dans l'Inspector
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0f, -1f, 0f);
            groundCheck = gc.transform;
        }

        // Si aucune layer assignée, on détecte toutes les layers (mode permissif)
        if (groundLayer.value == 0)
            groundLayer = ~LayerMask.GetMask("Player");
    }

    void Update()
    {
        CheckGround();
        HandleCoyoteAndBuffer();
        HandleMovement();
        HandleJump();
        ApplyBetterGravity();
        UpdateAnimator();
    }

    void CheckGround()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void HandleCoyoteAndBuffer()
    {
        // Start coyote timer when player just left the ground
        if (wasGrounded && !isGrounded)
            coyoteTimer = coyoteTime;
        else if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        // Jump buffer countdown
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;
    }

    void HandleMovement()
    {
        float input = 0f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            input = -1f;
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            input = 1f;

        // Mode auto-run : avance tout seul vers la droite, gauche/droite modifie la vitesse
        if (autoRun)
        {
            float targetSpeed = input == -1f ? 0f : (input == 1f ? maxSpeed : autoRunSpeed);
            float newVX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, acceleration * Time.deltaTime);
            rb.linearVelocity = new Vector3(newVX, rb.linearVelocity.y, 0f);
            if (rb.linearVelocity.x > 0.1f)
                transform.localScale = new Vector3(1f, 1f, 1f);
            return;
        }

        float currentVX = rb.linearVelocity.x;

        if (input != 0f)
        {
            // Accelerate toward target speed
            float target = input * maxSpeed;
            float newVX = Mathf.MoveTowards(currentVX, target, acceleration * Time.deltaTime);
            rb.linearVelocity = new Vector3(newVX, rb.linearVelocity.y, 0f);

            // Flip character
            transform.localScale = new Vector3(input > 0 ? 1f : -1f, 1f, 1f);
        }
        else
        {
            // Decelerate to 0 (skidding feel)
            float newVX = Mathf.MoveTowards(currentVX, 0f, deceleration * Time.deltaTime);
            rb.linearVelocity = new Vector3(newVX, rb.linearVelocity.y, 0f);
        }
    }

    void HandleJump()
    {
        bool canJump = coyoteTimer > 0f && jumpBufferTimer > 0f;

        if (canJump)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, 0f);
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;

            if (animator != null) animator.SetTrigger("Jump");
        }

        // Cut jump short when button released (variable jump height)
        bool jumpHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
        if (!jumpHeld && rb.linearVelocity.y > 0f)
        {
            Vector3 v = rb.linearVelocity;
            v.y -= jumpHoldMultiplier * Physics.gravity.magnitude * Time.deltaTime;
            rb.linearVelocity = v;
        }
    }

    void ApplyBetterGravity()
    {
        // Extra gravity on the way down for snappy Mario feel
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;

            // Clamp fall speed
            if (rb.linearVelocity.y < maxFallSpeed)
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxFallSpeed, 0f);
        }
    }

    void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalSpeed", rb.linearVelocity.y);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
