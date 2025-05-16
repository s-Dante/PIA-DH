using UnityEngine;

public class BoundaryTeleporter : MonoBehaviour
{
    void OnTriggerExit(Collider other)
    {
        // Asume que tu jugador lleva la etiqueta "Player"
        if (!other.CompareTag("Player")) return;

        // Calcula la dirección desde el centro
        Vector3 dir = other.transform.position.normalized;
        float r = GetComponent<SphereCollider>().radius;

        // Teletransporta al lado opuesto
        other.transform.position = -dir * r;

        // Opcional: gira 180° al jugador para que mire hacia dentro
        other.transform.Rotate(0, 180f, 0);
    }
}
