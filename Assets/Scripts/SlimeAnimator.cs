using UnityEngine;

// Pilote l'Animator du Slime en fonction de la vitesse de l'ennemi parent
public class SlimeAnimator : MonoBehaviour
{
    private Animator anim;
    private Rigidbody parentRb;

    void Start()
    {
        anim = GetComponent<Animator>();
        parentRb = GetComponentInParent<Rigidbody>();
    }

    void Update()
    {
        if (anim == null || parentRb == null) return;

        float speed = new Vector3(parentRb.linearVelocity.x, 0f, parentRb.linearVelocity.z).magnitude;
        anim.SetFloat("Speed", speed);
    }
}
