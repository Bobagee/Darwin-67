using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LightConeMesh : MonoBehaviour
{
    [Header("Sincronización con Luz")]
    [Tooltip("Arrastra aquí el Spotlight para adaptar el largo y apertura del cono automáticamente")]
    [SerializeField] private Light targetSpotlight;
    [Tooltip("Si está activo, se usarán el Range y Spot Angle del Spotlight")]
    [SerializeField] private bool autoSyncWithLight = true;

    [Header("Dimensiones Manuales")]
    [SerializeField] private float startRadius = 0.05f;    // Radio en la boca de la linterna
    [SerializeField] private float manualLength = 5.0f;     // Largo del cono sin Spotlight
    [SerializeField] private float manualEndRadius = 1.8f;  // Radio final sin Spotlight

    [Header("Calidad Malla")]
    [Range(8, 64)]
    [SerializeField] private int segments = 24;             // Suavidad de la circunferencia

    private void Update()
    {
        SyncAndGenerate();
    }

    private void OnValidate()
    {
        SyncAndGenerate();
    }

    public void SyncAndGenerate()
    {
        float length = manualLength;
        float endRadius = manualEndRadius;

        if (autoSyncWithLight && targetSpotlight != null)
        {
            length = targetSpotlight.range;
            float halfAngleRad = (targetSpotlight.spotAngle * 0.5f) * Mathf.Deg2Rad;
            endRadius = Mathf.Tan(halfAngleRad) * length;
        }

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) return;

        Mesh mesh = new Mesh { name = "HazDeLuzConeMesh" };

        int numVertices = (segments + 1) * 2;
        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[segments * 6];

        float angleStep = 2f * Mathf.PI / segments;
        int vIndex = 0;
        int tIndex = 0;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            vertices[vIndex] = new Vector3(cos * startRadius, sin * startRadius, 0f);
            vertices[vIndex + 1] = new Vector3(cos * endRadius, sin * endRadius, length);

            if (i < segments)
            {
                triangles[tIndex++] = vIndex;
                triangles[tIndex++] = vIndex + 1;
                triangles[tIndex++] = vIndex + 2;

                triangles[tIndex++] = vIndex + 1;
                triangles[tIndex++] = vIndex + 3;
                triangles[tIndex++] = vIndex + 2;
            }

            vIndex += 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }
}