using UnityEngine;

public class InventoryContainer : MonoBehaviour
{
    [SerializeField] private Inventory _contents; // Inventory representing just what is inside this container

    public Inventory Contents => _contents;
}
