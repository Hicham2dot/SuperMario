using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Mouvement")]
    public float speed = 5f;

    [Header("Saut")]
    public float jumpForce = 6f;

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("[Player] ERREUR : Rigidbody manquant sur le joueur !");
            return;
        }

        rb.freezeRotation = true;

        // Diagnostic au démarrage
        Debug.Log($"[Player] Start OK | isKinematic={rb.isKinematic} | constraints={rb.constraints}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            isGrounded = false;
        }
    }

    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Log visible dès qu'une touche est pressée
        if (h != 0f || v != 0f)
            Debug.Log($"[Player] Touche détectée h={h} v={v} | vitesse appliquée={h * speed} / {v * speed}");

        rb.linearVelocity = new Vector3(h * speed, rb.linearVelocity.y, v * speed);

        Vector3 dir = new Vector3(h, 0f, v);
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir);
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
