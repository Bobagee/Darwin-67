using UnityEngine;

public abstract class Iteraccion : MonoBehaviour
{
    public abstract void Interactuar();
    public virtual void OnMirar(bool mirando) { }
}