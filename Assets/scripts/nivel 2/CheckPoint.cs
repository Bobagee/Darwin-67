using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [Header("Puntos de Control")]
    [SerializeField] private Transform initialCheckpoint;
    [SerializeField] private GameObject playerObject;

    private Vector3 currentCheckpointPosition;
    private Quaternion currentCheckpointRotation;

    private void OnEnable()
    {
        TiempoBajoLuz.OnPlayerCaught += RespawnAtCheckpoint;
    }

    private void OnDisable()
    {
        TiempoBajoLuz.OnPlayerCaught -= RespawnAtCheckpoint;
    }

    private void Start()
    {
        if (initialCheckpoint != null)
        {
            SetCheckpoint(initialCheckpoint.position, initialCheckpoint.rotation);
        }
    }

    public void SetCheckpoint(Vector3 pos, Quaternion rot)
    {
        currentCheckpointPosition = pos;
        currentCheckpointRotation = rot;
    }

    public void RespawnAtCheckpoint()
    {
        if (playerObject == null) return;

        CharacterController cc = playerObject.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        playerObject.transform.position = currentCheckpointPosition;
        playerObject.transform.rotation = currentCheckpointRotation;

        if (cc != null) cc.enabled = true;

        Debug.Log("Jugador atrapado. Respawn en el último Checkpoint.");
    }
}