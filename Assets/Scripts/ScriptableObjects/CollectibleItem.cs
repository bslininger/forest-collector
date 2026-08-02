using UnityEngine;

[CreateAssetMenu(fileName = "CollectibleItem", menuName = "Scriptable Objects/CollectibleItem")]
public class CollectibleItem : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;

    public string Id => id;
    public string DisplayName => displayName;
}
