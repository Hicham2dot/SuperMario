using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Mouvement")]
    public float speed = 5f;

    [Header("Saut")]
    public float jumpForce = 6f;

    [Header("Animations (optionnel)")]
    public Animator animator;   // glisser PlayerModel ici dans l'Inspector

    private Rigidbody rb;
    private bool isGrounded;

    // IDs précalculés pour éviter les string lookups chaque frame
    private static readonly int HashRunning  = Animator.StringToHash("isRunning");
    private static readonly int HashGrounded = Animator.StringToHash("isGrounded");
    private static readonly int HashJump     = Animator.StringToHash("jumpTrigger");

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("[Player] ERREUR : Rigidbody manquant sur le joueur !");
            return;
        }

        rb.freezeRotation = true;
        Debug.Log($"[Player] Start OK | isKinematic={rb.isKinematic} | constraints={rb.constraints}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            isGrounded = false;
            if (animator != null) animator.SetTrigger(HashJump);
        }
    }

    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        rb.linearVelocity = new Vector3(h * speed, rb.linearVelocity.y, v * speed);

        Vector3 dir = new Vector3(h, 0f, v);
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir);

        // Pilote l'Animator seulement s'il est assigné
        if (animator != null)
        {
            animator.SetBool(HashRunning,  dir.sqrMagnitude > 0.01f);
            animator.SetBool(HashGrounded, isGrounded);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        foreach (ContactPoint c in col.contacts)
            if (c.normal.y > 0.5f) { isGrounded = true; return; }
    }

    void OnCollisionStay(Collision col)
    {
        foreach (ContactPoint c in col.contacts)
            if (c.normal.y > 0.5f) { isGrounded = true; return; }
    }

    void OnCollisionExit(Collision _) => isGrounded = false;
}
