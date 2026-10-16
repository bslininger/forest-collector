using System;
using UnityEngine;

[Serializable]
public class InventoryStartingEntry
{
    [SerializeField] private Item _item;
    [SerializeField] [Min(1)] private int _quantity = 1;

    public Item Item => _item;
    public int Quantity => _quantity;
}

