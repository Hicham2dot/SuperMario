using UnityEngine;

public class AttachToParent : MonoBehaviour
{
    public GameObject targetParent;

    void Awake()
    {
        if (targetParent != null)
        {
            transform.SetParent(targetParent.transform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }
}
