using UnityEngine;
using System.Collections;

public class Portal : MonoBehaviour
{
    public Transform targetPortal;  // 另一个传送门的位置
    // 传送特效预制体
    public GameObject teleportEffectPrefab;

    public bool isTeleporting = false; // 防止重复传送

    private void Start()
    {
        if (targetPortal == null)
        {
            Debug.LogError("目标传送门未设置！");
        }
        isTeleporting = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (isTeleporting) return; // 如果正在传送，直接返回   
        if (other.CompareTag("Player"))
        {
            PlayerTeleportState state = other.GetComponent<PlayerTeleportState>();
            // 记录传送时间，防止连续传送
            state.RecordTeleport();

            // 生成传送特效
            if (teleportEffectPrefab != null)
            {
                GameObject effect = Instantiate(teleportEffectPrefab, targetPortal.position, Quaternion.identity);
                Destroy(effect, 1.5f); // 1.5秒后删除特效
            }

            // 计算相对于当前传送门的偏移
            Vector3 offset = other.transform.position - transform.position;
            // 将玩家移动到目标传送门的位置，并保持相对偏移
            other.transform.position = targetPortal.position + offset;
            // 启动协程，控制目标传送门的显示
            StartCoroutine(Teleport());
        }
    }

    private IEnumerator Teleport()
    {
        Portal portal = targetPortal.GetComponent<Portal>();
        if (portal != null)
        {
            portal.isTeleporting = true; // 目标传送门进入传送状态
        }

        isTeleporting = true;
        yield return new WaitForSeconds(1.5f); 
        isTeleporting = false;
        if (portal != null)
        {
            portal.isTeleporting = false; // 目标传送门退出传送状态
        }
    }
}
