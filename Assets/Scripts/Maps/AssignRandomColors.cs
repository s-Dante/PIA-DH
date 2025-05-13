using UnityEngine;

public class AssignRandomColors : MonoBehaviour
{
    [Header("Base color (RGB)")]
    public bool isRandom = false;  // Si los colores son aleatorios o no
    public Color baseColor = Color.blue;  // Que color base es
    [Range(0f, 1f)]
    public float shadeRange = 0.3f;  // Rango de variación para las tonalidades

    [Header("Shade range")]
    [Range(0f, 1f)] public float metallic = 0.5f;
    [Range(0f, 1f)] public float roughness = 0.5f;



    void Start()
    {
        // Obtener todos los hijos directos e indirectos del objeto principal
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in renderers)
        {
            
            if (isRandom)
            {
                if (rend.material.shader.name == "Standard")
                {
                    rend.material.SetFloat("_Metallic", metallic);
                    rend.material.SetFloat("_Glossiness", roughness);
                }
                // Asigna un color aleatorio a cada hijo con Renderer
                rend.material.color = new Color(
                    Random.value,  // Rojo
                    Random.value,  // Verde
                    Random.value   // Azul
                );
            }
            else
            {
                // Genera tonalidades del color base
                float r = Mathf.Clamp(baseColor.r + Random.Range(-shadeRange, shadeRange), 0f, 1f);
                float g = Mathf.Clamp(baseColor.g + Random.Range(-shadeRange, shadeRange), 0f, 1f);
                float b = Mathf.Clamp(baseColor.b + Random.Range(-shadeRange, shadeRange), 0f, 1f);
                rend.material.color = new Color(r, g, b);
            }
        }
    }
}
