using UnityEngine;
using System.Collections.Generic;

public class Escape : Iteraccion
{
    [Header("Requisitos de Escape")]
    [SerializeField] private ItemTipo requiredItem = ItemTipo.Desarmador;
    [SerializeField] private int requiredClicks = 3;

    private int currentClicks = 0;
    private static HashSet<ItemTipo> inventoryReference = new HashSet<ItemTipo>();

    public static void RegisterItemInInventory(ItemTipo tipo)
    {
        inventoryReference.Add(tipo);
    }

    public override void Interactuar()
    {
        if (!inventoryReference.Contains(requiredItem))
        {
            Debug.Log("Necesitas el Desarmador para abrir la escotilla.");
            return;
        }

        currentClicks++;
        Debug.Log($"Desatornillando escotilla: {currentClicks}/{requiredClicks}");

        if (currentClicks >= requiredClicks)
        {
            Debug.Log("¡Escotilla abierta! Escape completado.");
        }
    }
}