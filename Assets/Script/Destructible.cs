using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    public bool isDestruct = false;
    public GameObject dropped;

    private bool triggered = false;

    private void Start()
    {
        if (dropped != null) 
        {
            dropped.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;   // 已经触发过，不再执行
        if (other.CompareTag("PlayerAttack"))
        {
            triggered = true;
            StartCoroutine(SpawnDroppedItem());
        }
    }

    private IEnumerator SpawnDroppedItem()
    {
        // 生成掉落物，记录原始位置
        dropped.SetActive(true);
        Vector3 startPos = dropped.transform.position;

        // 关闭碰撞体
        Collider[] cols = dropped.GetComponentsInChildren<Collider>();
        foreach (var col in cols) col.enabled = false;

        // 动画部分
        // 计算向上偏移的位置
        Vector3 highPos = startPos + Vector3.up * 2f;
        yield return StartCoroutine(MoveOverTime(dropped.transform, highPos, 0.5f));
        // 再落回原位
        yield return StartCoroutine(MoveOverTime(dropped.transform, startPos, 0.5f));

        yield return new WaitForSeconds(1f); // 等待一段时间，确保动画完成
        // 启用碰撞器
        foreach (var col in cols) col.enabled = true;

        // 如果需要，销毁自身
        if (isDestruct)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 掉落物动画
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="target"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator MoveOverTime(Transform obj, Vector3 target, float duration)
    {
        if (duration <= 0)
        {
            obj.position = target;
            yield break;
        }
        float elapsed = 0;
        Vector3 start = obj.position;
        while (elapsed < duration)
        {
            obj.position = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        obj.position = target;
    }
}
