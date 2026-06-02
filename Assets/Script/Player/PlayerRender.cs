using UnityEngine;

public class PlayerRender : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer sprite;
    public PlayerManager playerManager;

    public GameObject doubleJumpVFX;
    public GameObject tripleJumpVFX;
    public GameObject slamGroundVFX;
    public GameObject slamCollider;      // 猛击地面时的碰撞体

    // ---------- 动画状态标志 ----------
    private bool isDoubleJumping = false;
    private bool isTripleJumping = false;
    private bool isSlamming = false;

    // ---------- 计时器 ----------
    private float doubleJumpTimer = 0f;
    private float tripleJumpTimer = 0f;
    private float slamTimer = 0f;

    private const float jumpAnimDuration = 0.5f;   // 二/三段跳旋转时长
    private const float slamDuration = 0.5f;       // 猛击特效总时长
    private const float slamColliderDeactivateTime = 0.8f; // 碰撞体关闭的进度比例

    private void Start()
    {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        if (doubleJumpVFX != null) doubleJumpVFX.SetActive(false);
        if (tripleJumpVFX != null) tripleJumpVFX.SetActive(false);
        if (slamGroundVFX != null) slamGroundVFX.SetActive(false);
        if (slamCollider != null) slamCollider.SetActive(false);
    }

    private void Update()
    {
        // 根据玩家移动状态切换跑步动画（保持不变）
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            animator.SetBool("isRunning", true);
            sprite.flipX = Input.GetAxis("Horizontal") > 0;
        }
        else
        {
            animator.SetBool("isRunning", false);
        }

        // ---------- 逐帧处理二段跳动画 ----------
        if (isDoubleJumping)
        {
            doubleJumpTimer += Time.deltaTime;
            float progress = doubleJumpTimer / jumpAnimDuration;

            // 旋转（360度）
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Clamp01(progress) * 360f);

            if (progress >= 1f)
            {
                // 动画结束，重置状态
                isDoubleJumping = false;
                doubleJumpTimer = 0f;
                transform.rotation = Quaternion.identity;
                if (doubleJumpVFX != null) doubleJumpVFX.SetActive(false);
            }
        }

        // ---------- 逐帧处理三段跳动画 ----------
        if (isTripleJumping)
        {
            tripleJumpTimer += Time.deltaTime;
            float progress = tripleJumpTimer / jumpAnimDuration;

            // 反向旋转（-360度）
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Clamp01(progress) * -360f);

            if (progress >= 1f)
            {
                isTripleJumping = false;
                tripleJumpTimer = 0f;
                transform.rotation = Quaternion.identity;
                if (tripleJumpVFX != null) tripleJumpVFX.SetActive(false);
            }
        }

        // ---------- 逐帧处理猛击地面动画 ----------
        if (isSlamming)
        {
            slamTimer += Time.deltaTime;
            float progress = slamTimer / slamDuration;

            // 在 80% 进度时关闭碰撞体
            if (progress >= slamColliderDeactivateTime && slamCollider != null && slamCollider.activeSelf)
            {
                slamCollider.SetActive(false);
            }

            // 动画完全结束时关闭特效，重置状态
            if (progress >= 0.5f)
            {
                isSlamming = false;
                slamTimer = 0f;
                if (slamGroundVFX != null) slamGroundVFX.SetActive(false);
                if (slamCollider != null) slamCollider.SetActive(false); // 确保关闭
            }
        }
    }

    /// <summary>
    /// 普通跳跃动画（只触发一次，不需要状态控制）
    /// </summary>
    public void Jump()
    {
        animator.SetTrigger("isJumping");
    }

    /// <summary>
    /// 开始二段跳动画（由 PlayerManager 事件触发）
    /// </summary>
    public void DoubleJump()
    {
        isDoubleJumping = true;
        doubleJumpTimer = 0f;
        transform.rotation = Quaternion.identity;
        if (doubleJumpVFX != null) doubleJumpVFX.SetActive(true);

        animator.SetTrigger("isJumping");
    }

    /// <summary>
    /// 开始三段跳动画
    /// </summary>
    public void TripleJump()
    {
        isTripleJumping = true;
        tripleJumpTimer = 0f;
        transform.rotation = Quaternion.identity;
        if (tripleJumpVFX != null) tripleJumpVFX.SetActive(true);

        animator.SetTrigger("isJumping");
    }

    /// <summary>
    /// 开始猛击地面效果
    /// </summary>
    public void SlamGround()
    {
        isSlamming = true;
        slamTimer = 0f;
        if (slamGroundVFX != null) slamGroundVFX.SetActive(true);
        if (slamCollider != null) slamCollider.SetActive(true);
    }
}