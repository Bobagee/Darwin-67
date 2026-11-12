using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ConitoGenerador : MonoBehaviour
{
    [Header("Vinculacion de Luz")]
    [Tooltip("Arrastra aqui la luz Spot de Unity para que el cono copie su rango y angulo")]
    [SerializeField] private Light targetSpotlight;

    [Header("Ajustes Manuales / Visuales")]
    [Tooltip("Si esta activo, se usaran el Range y Spot Angle del Spotlight automaticamente")]
    [SerializeField] private bool autoSyncWithLight = true;
    
    [SerializeField] private float startRadius = 0.05f; 
    [SerializeField] private float manualLength = 5.0f;  
    [SerializeField] private float manualEndRadius = 1.8f;
    [SerializeField] private int segments = 24;      

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