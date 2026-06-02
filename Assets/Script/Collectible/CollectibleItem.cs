using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [SerializeField] private CollectibleData data;   // 拖入对应的数据资产
    // 标记是否已被收集，防止重复触发
    private bool isCollected = false;

    public string UniqueID => data.uniqueID;

    private void Start()
    {
        // 如果该收集物已被全局管理器记录为“已收集”，则直接销毁
        if (CollectionStateManager.Instance.IsCollected(data.uniqueID))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 检测是否是玩家（需要玩家身上有 "Player" 标签）
        if (!isCollected && other.CompareTag("Player"))
        {
            isCollected = true;
            Collect();
        }
    }

    /// <summary>
    /// 当玩家接触并收集时调用
    /// </summary>
    public void Collect()
    {
        // 1. 标记为已收集
        CollectionStateManager.Instance.MarkAsCollected(data.uniqueID);

        // 2. 更新玩家收集数量（可传递数据用于UI显示）
        CollectionUI player = PlayerRoot.Instance?.GetComponent<CollectionUI>();
        if (player != null)
        {
            player.AddCollection(1, data);
        }

        // 3. 销毁自身
        Destroy(gameObject);
    }
}
