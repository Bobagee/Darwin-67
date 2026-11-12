using UnityEngine;

public class Escondite : Iteraccion
{
    [Header("Puntos de Teletransporte")]
    [SerializeField] private Transform insideSpot;
    [SerializeField] private Transform exitSpot;

    public static bool IsPlayerHidden { get; private set; } = false;

    public override void Interactuar()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null && Camera.main != null)
        {
            player = Camera.main.transform.root.gameObject;
        }

        if (player == null) return;

        CharacterController cc = player.GetComponent<CharacterController>();

        IsPlayerHidden = !IsPlayerHidden;

        if (cc != null) cc.enabled = false;

        if (IsPlayerHidden)
        {
            if (insideSpot != null)
                player.transform.position = insideSpot.position;
        }
        else
        {
            if (exitSpot != null)
                player.transform.position = exitSpot.position;
        }

        if (cc != null) cc.enabled = true;
    }
}