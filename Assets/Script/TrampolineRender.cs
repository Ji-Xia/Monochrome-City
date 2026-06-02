using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TrampolineRender : MonoBehaviour
{
    private Vector3 originalScale;
    private Coroutine currentCoroutine;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    /// <summary>
    /// 外部调用：触发弹簧压缩动画（协程会先恢复原始大小，再压缩，再恢复）
    /// </summary>
    public void TriggerCompress()
    {
        // 如果有正在运行的协程，先停止它（防止冲突）
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        // 启动新协程
        currentCoroutine = StartCoroutine(CompressRoutine());
    }

    private IEnumerator CompressRoutine()
    {
        transform.localScale = originalScale;

        transform.localScale = new Vector3(originalScale.x, originalScale.y - 0.2f, originalScale.z);
        // 等待 0.15 秒
        yield return new WaitForSeconds(0.15f);

        transform.localScale = originalScale;

        currentCoroutine = null;
    }
}
