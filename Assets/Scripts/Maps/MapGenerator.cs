using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.U2D;

public class MapGenerator : MonoBehaviour
{
    public string filePath = "Resources/WorldData/countries-node.txt";
    public Material outlineMaterial;  // Material para el contorno
    public Material fillMaterial;     // Material para el relleno

    private void Start()
    {
        LoadMap();
    }

    void LoadMap()
    {
        Dictionary<int, List<List<Vector3>>> polygons = new Dictionary<int, List<List<Vector3>>>();

        // Leer el archivo de coordenadas
        TextAsset textAsset = Resources.Load<TextAsset>("WorldData/countries-node");
        if (textAsset == null)
        {
            Debug.LogError("Archivo no encontrado: " + filePath);
            return;
        }

        string[] lines = textAsset.text.Split('\n');

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');
            if (parts.Length < 4) continue;

            int shapeId = int.Parse(parts[0]);
            int partId = int.Parse(parts[1]);
            float x = float.Parse(parts[2]);
            float z = float.Parse(parts[3]);

            if (!polygons.ContainsKey(shapeId))
            {
                polygons[shapeId] = new List<List<Vector3>>();
            }

            while (polygons[shapeId].Count <= partId)
            {
                polygons[shapeId].Add(new List<Vector3>());
            }

            polygons[shapeId][partId].Add(new Vector3(x, 0, z));
        }

        // Dibujar el contorno y el relleno de los países
        foreach (var shape in polygons)
        {
            foreach (var part in shape.Value)
            {
                CloseShapeIfNeeded(part);
                DrawOutline(part);
                DrawFill(part);
            }
        }
    }

    void DrawOutline(List<Vector3> points)
    {
        GameObject lineObj = new GameObject("Outline");
        lineObj.transform.parent = transform;

        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());

        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = outlineMaterial;
        lineRenderer.loop = true;  // Cierra el contorno
    }

    void DrawFill(List<Vector3> points)
    {
        if (points.Count < 3) return; // Necesitamos al menos 3 puntos para formar un triángulo

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.parent = transform;

        MeshFilter meshFilter = fillObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = fillObj.AddComponent<MeshRenderer>();
        meshRenderer.material = fillMaterial;

        Mesh mesh = new Mesh();

        // Convertimos a 2D para la triangulación
        Vector2[] points2D = new Vector2[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            points2D[i] = new Vector2(points[i].x, points[i].z);
        }

        // Usamos Triangulator para obtener los triángulos
        Triangulator triangulator = new Triangulator(points2D);
        int[] triangles = triangulator.Triangulate();

        // Convertimos de nuevo a Vector3
        Vector3[] vertices = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            vertices[i] = new Vector3(points[i].x, 0, points[i].z);
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;

        // Agregar colisiones opcionales
        MeshCollider collider = fillObj.AddComponent<MeshCollider>();
        //collider.isTrigger = true;
        collider.sharedMesh = mesh;

        //// Añadimos un SpriteShapeController para el relleno
        //SpriteShapeController spriteShapeController = fillObj.AddComponent<SpriteShapeController>();
        //spriteShapeController.spriteShape = Resources.Load<SpriteShape>("WorldData/SpriteShapes/Hexagon");
        //spriteShapeController.spline.Clear();
        //spriteShapeController.spline.InsertPointsAt(0, points.ToArray());
        //spriteShapeController.spline.isOpenEnded = false;
        //spriteShapeController.spline.isOpenEnded = true;

    }

    void CloseShapeIfNeeded(List<Vector3> points)
    {
        if (points.Count > 2 && points[0] != points[points.Count - 1])
        {
            // Agregar el primer punto al final para cerrar la forma
            points.Add(points[0]);
        }
    }

}
