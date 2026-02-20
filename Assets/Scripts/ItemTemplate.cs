using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Template")]
public class ItemTemplate : ScriptableObject
{
    [Header("Datos del Item")]
    public int ID;                  // ID único (debe coincidir con la DB)
    public string Nombre;
    [TextArea(3, 5)]                // Para descripción multiline
    public string Descripcion;
    public Sprite Imagen;           // Icono para UI
    public int MaxStack = 99;       // Máximo acumulable (1 = no acumulable)
}