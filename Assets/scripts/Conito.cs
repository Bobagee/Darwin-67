using UnityEngine;

public class LightCone : MonoBehaviour
{
    [Header("Configuración del Cono")]
    [SerializeField] private Light spotLight;
    [SerializeField] private LayerMask obstacleLayer;
    public Light SpotLight => spotLight;
    public bool IsOn => spotLight != null && spotLight.enabled;

    public bool IsPositionInCone(Vector3 targetPosition)
    {
        if (!IsOn || spotLight == null) return false;

        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget > spotLight.range) return false;

        float angle = Vector3.Angle(transform.forward, directionToTarget);
        if (angle > spotLight.spotAngle * 0.5f) return false;

        if (Physics.Raycast(transform.position, directionToTarget, out RaycastHit hit, distanceToTarget, obstacleLayer))
        {
            return false;
        }

        return true;
    }
}