using System.Collections;
using UnityEngine;

public class NPCKnockback : MonoBehaviour
{
    [Header("击飞参数")]
    public float knockbackForce = 5f;       // 水平击飞力度
    public float upwardForce = 3f;          // 向上力度（模拟抛物线）
    public float knockbackDuration = 0.5f;  // 击飞持续时长（物理运动时间）
    public float recoveryTime = 0.3f;       // 落地后额外恢复时间
    public bool justKnockY = false;         // 仅击飞Y轴（适用于某些特殊NPC）

    private Rigidbody rb;
    private Pathfinding pathfinding;
    private bool isKnockedBack = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (pathfinding != null)
            pathfinding = GetComponent<Pathfinding>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            // 计算击飞方向：从攻击者指向 NPC
            Vector3 direction = (transform.position - other.transform.position).normalized;
            // 限制y轴分量
            direction.y = 0;
            direction.Normalize();

            ApplyKnockback(direction);
        }
    }

    /// <summary>
    /// 击飞方法
    /// </summary>
    /// <param name="direction"></param>
    public void ApplyKnockback(Vector3 direction)
    {
        // 防止击飞过程中重复触发
        if (isKnockedBack) return;

        StartCoroutine(KnockbackCoroutine(direction));
    }

    private IEnumerator KnockbackCoroutine(Vector3 direction)
    {
        // 标记击飞状态，禁用寻路
        isKnockedBack = true;
        if (pathfinding != null)
            pathfinding.enabled = false;

        // 停止当前移动速度（避免叠加）
        rb.velocity = Vector3.zero;

        // 计算击飞速度：水平方向 + 向上分量
        Vector3 knockbackVelocity = direction * knockbackForce;
        if (justKnockY)
        {
           knockbackVelocity = Vector3.zero; // 水平速度为0
        }
        knockbackVelocity.y = upwardForce;
        rb.velocity = knockbackVelocity;

        // 等待物理运动主要阶段结束
        float elapsed = 0f;
        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 等待角色完全稳定（再缓冲一小段时间）
        yield return new WaitForSeconds(recoveryTime);

        // 恢复寻路，清除残留速度
        rb.velocity = Vector3.zero;
        if (pathfinding != null)
            pathfinding.enabled = true;
        isKnockedBack = false;
    }

}
