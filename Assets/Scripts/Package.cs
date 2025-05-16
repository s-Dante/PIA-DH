using UnityEngine;

public class Package : MonoBehaviour
{
    [Header("Materiales")]
    public Material correctMaterial;
    public Material incorrectMaterial;

    bool delivered = false;
    Rigidbody rb;
    Renderer rend;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (delivered) return;
        if (!other.CompareTag("Country")) return;

        delivered = true;

        // 1) Detén la física
        rb.linearVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;

        // 2) Decide acierto/fallo
        bool correcto = other.transform == GameManager.Instance.CurrentTarget;

        // 3) Cambia el material
        rend.material = correcto ? correctMaterial : incorrectMaterial;

        // 4) Notifica al GameManager
        GameManager.Instance.OnPackageDelivered(correcto);
    }
}
