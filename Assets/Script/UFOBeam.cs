using UnityEngine;

public class UFOBeam : MonoBehaviour
{
    [Header("悬停目标")]
    [Tooltip("UFO 底部的悬停位置（可拖入空物体）")]
    public Transform targetPoint;

    [Header("牵引参数")]
    [Tooltip("弹簧强度：越高拉力越强，归位越快")]
    public float springStrength = 10f;

    [Tooltip("阻尼系数：抑制震荡，越大越稳但可能变慢")]
    public float damping = 5f;

    [Tooltip("光束最大作用距离（世界单位）")]
    public float maxDistance = 5f;

    [Header("力模式")]
    [Tooltip("推荐 Acceleration，忽略质量，效果一致")]
    public ForceMode forceMode = ForceMode.Acceleration;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"{name} 的 Collider 已自动设为 Trigger");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        // 计算高度差
        float heightDiff = targetPoint.position.y - rb.worldCenterOfMass.y;
        float absDiff = Mathf.Abs(heightDiff);

        // 超出最大作用范围不施加力
        if (absDiff > maxDistance) return;

        // 当前垂直速度
        float velocityY = rb.velocity.y;

        // PD 控制：期望的向上加速度
        float desiredAccel = heightDiff * springStrength - velocityY * damping;

        // 施加竖直方向力（正值向上，负值向下）
        rb.AddForce(Vector3.up * desiredAccel, forceMode);
    }

}
