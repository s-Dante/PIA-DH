using UnityEngine;

public class CountryHighlLighter : MonoBehaviour
{
    private Material originalMaterial;
    public Material highlightMaterial;
    public Material targetMaterial;

    private GameObject currentCountry;
    private GameObject targetCountry;

    void Update()
    {
        HighlightCurrentCountry();
    }

    void HighlightCurrentCountry()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject != currentCountry)
            {
                ResetHighlight();
                currentCountry = hit.collider.gameObject;
                originalMaterial = currentCountry.GetComponent<MeshRenderer>().material;
                currentCountry.GetComponent<MeshRenderer>().material = highlightMaterial;
            }
        }
        else
        {
            ResetHighlight();
        }
    }

    public void SetTargetCountry(GameObject target)
    {
        if (targetCountry != null)
        {
            targetCountry.GetComponent<MeshRenderer>().material = originalMaterial;
        }

        targetCountry = target;
        targetCountry.GetComponent<MeshRenderer>().material = targetMaterial;
    }

    void ResetHighlight()
    {
        if (currentCountry != null)
        {
            currentCountry.GetComponent<MeshRenderer>().material = originalMaterial;
            currentCountry = null;
        }
    }
}
