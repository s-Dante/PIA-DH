using UnityEngine;

public class Package : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    [Header("Materiales")]
    public Material correctMaterial;
    public Material incorrectMaterial;

    private bool delivered = false;      // Para no procesar m�s de una vez
    private Rigidbody rb;
    private Renderer rend;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (delivered) return;           // ya procesado
        if (!other.CompareTag("Country")) return;

        delivered = true;

        // 1) Determinar acierto/fallo
        bool correcto = other.name == GameManager.Instance.CurrentTarget.name;

        // 2) �Pegar� el paquete en su posici�n actual
        rb.linearVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;

        // 3) Cambiar color
        rend.material = correcto
            ? correctMaterial
            : incorrectMaterial;

        // 4) Notificar al GameManager
        GameManager.Instance.OnPackageDelivered(correcto);
    }
}
