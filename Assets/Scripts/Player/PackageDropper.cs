using UnityEngine;

public class PackageDropper : MonoBehaviour
{
    [Header("Prefab & Punto de ca�da")]
    [Tooltip("Prefab del paquete a soltar")]
    public GameObject packagePrefab;
    [Tooltip("Empty child: posici�n exacta donde sale el paquete")]
    public Transform dropPoint;

    [Header("Opcional: impulso inicial")]
    [Tooltip("Si quieres que el paquete salga con velocidad")]
    public float dropForce = 0f;

    public void DropPackage()
    {
        if (packagePrefab == null || dropPoint == null)
        {
            Debug.LogWarning("PackagePrefab o DropPoint no asignados en PackageDropper");
            return;
        }

        // Instanciar el paquete justo debajo del avi�n
        GameObject pkg = Instantiate(
            packagePrefab,
            dropPoint.position,
            dropPoint.rotation
        );

        // Si quieres un peque�o empuj�n inicial
        Rigidbody rb = pkg.GetComponent<Rigidbody>();
        if (rb != null && dropForce != 0f)
        {
            // Empuja hacia abajo en el eje local �abajo� del avi�n
            rb.AddForce(-dropPoint.up * dropForce, ForceMode.Impulse);
        }
    }
}
