using UnityEngine;

[CreateAssetMenu(fileName = "NewCollectible", menuName = "收集物数据")]
public class CollectibleData : ScriptableObject
{
    public string uniqueID;      // 全局唯一ID，如 "Forest_Radish_01"
    public string itemName;
    public Sprite icon;
}
