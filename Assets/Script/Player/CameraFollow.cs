using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("是否启用边界设置")]
    public bool enableBoundary = true;                // 是否启用边界限制
    public Bounds boundary;                           // 限制区域（矩形）
    public Collider boundCollider1;                   // 可视化边界的碰撞盒

    [Header("屏幕震动")]
    public bool shakeAffectsY = true;                 // 震动是否影响 Y 轴
    private float shakeDuration;                      // 震动持续时间
    private float shakeMagnitude;                     // 震动强度
    private float shakeTimer;                         // 震动计时器

    private void Start()
    {
        // 如果外部没有手动指定 player，自动从 PlayerRoot 单例获取
        if (player == null)
            AssignPlayerFromRoot();
    }
    void LateUpdate()
    {
        // 每帧兜底：如果 player 意外丢失，尝试重新获取
        if (player == null)
            AssignPlayerFromRoot();

        if (player == null) return;   // 实在找不到就放弃本帧

        if (boundCollider1 != null)
            boundary = boundCollider1.bounds;

        Vector3 desiredPosition = player.position + offset;

        if (enableBoundary)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, boundary.min.x, boundary.max.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, boundary.min.y, boundary.max.y);
            desiredPosition.z = Mathf.Clamp(desiredPosition.z, boundary.min.z, boundary.max.z);
        }

        // --- 计算震动偏移 ---
        Vector3 shakeOffset = Vector3.zero;
        if (shakeTimer < shakeDuration)
        {
            shakeTimer += Time.deltaTime;
            float progress = shakeTimer / shakeDuration;                // 0 → 1
            float currentIntensity = Mathf.Lerp(shakeMagnitude, 0f, progress);
            shakeOffset = Random.insideUnitSphere * currentIntensity;
            if (!shakeAffectsY)
                shakeOffset.y = 0f;
        }

        // 最终位置 = 基础跟随位置(含边界) + 震动偏移，Y 轴基于配置的固定 y 值叠加震动
        transform.position = new Vector3(
            desiredPosition.x + shakeOffset.x,
            desiredPosition.y + shakeOffset.y,
            desiredPosition.z + shakeOffset.z
        );
    }

    /// <summary>
    /// 获取 PlayerRoot 单例中的玩家 Transform 引用
    /// </summary>
    private void AssignPlayerFromRoot()
    {
        if (PlayerRoot.Instance != null)
            player = PlayerRoot.Instance.transform;
    }

    /// <summary>
    /// 触发屏幕震动
    /// </summary>
    /// <param name="duration">震动持续时间（秒）</param>
    /// <param name="magnitude">最大震动偏移量（单位与场景一致）</param>
    public void Shake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        shakeTimer = 0f;
    }
}

