using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    private Rigidbody rb;
    public PlayerRender playerRender;

    [Header("交互UI")]
    public GameObject InteractiveUI; // 靠近NPC时显示的提示

    [Header("地面检测")]
    public Transform groundCheck;            // 脚底检测点（放在角色底部）
    public float radius = 0.12f;             // 检测半径
    public LayerMask groundLayer;            // 地面层级
    public LayerMask npcLayer;               // 敌人层级
    public LayerMask Trampoline;             // 弹簧层级

    [Header("移动参数")]
    public float moveSpeed = 1f;     // 基础移动速度
    public float airControl = 8f;   // 空中力的大小，越大转向越快
    public float groundControl = 30f; // 地面力的大小，响应更灵敏
    [Header("跳跃参数")]
    public float jumpHeight = 2f;
    public float diveSpeed = 20f;
    // 跳跃速度
    private float jumpVelocity;

    // --- 状态变量 ---
    // 这些状态变量用于跟踪玩家的当前状态，确保跳跃和俯冲逻辑正确执行
    private bool isGrounded;                // 是否在地面上
    private bool isTouchingNPC;             // 是否接触到敌人
    private bool isTrampoline;              // 是否在弹簧上

    private bool canDoubleJump;             // 是否可以二段跳
    private bool canTripleJump;             // 是否可以三段跳
    private bool canDive;                   // 是否可以俯冲
    private bool jumpPressed;               // 跳跃按键状态
    private bool divePressed;               // 俯冲按键状态
    public bool InteractionPressed;         // 交互按键状态
    private bool isDiving;                  // 是否正在俯冲
    public bool hasTeleported = false;     // 是否刚刚传送过，防止传送后立即被地面检测误判为着地

    // 外部调用设置
    public bool preserveMomentum = false;

    [Header("反弹保护")]
    public float reboundProtectionTime = 0.15f;  // 保护时间，根据手感调节
    private float lastReboundTime = -1f;         // 记录上次反弹的时间

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 计算跳跃速度
        jumpVelocity = Mathf.Sqrt(jumpHeight * 2f * Mathf.Abs(Physics.gravity.y));

        rb.freezeRotation = true;

        if (InteractiveUI != null) InteractiveUI.SetActive(false);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.started) jumpPressed = true;
    }

    public void Dive(InputAction.CallbackContext context)
    {
        if (context.started) divePressed = true;
    }

    public void Interaction(InputAction.CallbackContext context)
    {
        if (context.started)
            InteractionPressed = true;
        else if (context.canceled)          // 按键松开时立刻重置
            InteractionPressed = false;
    }

    private void Update()
    {
        // 地面检测
        isGrounded = Physics.CheckSphere(groundCheck.position, radius, groundLayer);
        // NPC检测
        isTouchingNPC = Physics.CheckSphere(groundCheck.position, radius, npcLayer);
        // 弹簧检测
        isTrampoline = Physics.CheckSphere(groundCheck.position, radius, Trampoline);
    }

    /// <summary>
    /// 交互检测：当玩家在可交互物范围内，显示交互提示，并在按下交互键时触发交互逻辑
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            // 尝试获取NPCRewardSpeak组件并触发对话
            NPCRewardSpeak npc = other.GetComponent<NPCRewardSpeak>();
            if (npc == null) return;
            // 显示交互提示
            if (!npc.isSpeaking)
            {
                InteractiveUI.SetActive(true);
            }
            else
            {
                InteractiveUI.SetActive(false);
            }

            if (InteractionPressed)
            {
                InteractionPressed = false;
                npc.StartDialogue();
            } 
        }
        else if (other.CompareTag("ScenesPortal") && !hasTeleported)
        {
            ScenesPortal portal = other.GetComponent<ScenesPortal>();
            if (portal == null) return;

            InteractiveUI.SetActive(true);

            if (!InteractionPressed) return;

            if (portal != null)
            {
                InteractionPressed = false;
                Debug.Log("触发传送门交互");
                hasTeleported = true;
                portal.Teleport();
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            NPCRewardSpeak Interaction = other.GetComponent<NPCRewardSpeak>();
            if (Interaction == null) return;
            InteractiveUI.SetActive(false);
            Interaction.NextLine();
        }
        else if (other.CompareTag("ScenesPortal"))
        {
            InteractiveUI.SetActive(false);
            hasTeleported = false;       // 离开传送门区域后重置标志，允许再次交互
        }
    }

    /// <summary>
    /// 物理更新：处理跳跃、二段跳、三段跳、俯冲以及着地后的状态重置
    /// </summary>
    private void FixedUpdate()
    {
        // 着地和反弹处理
        if (isGrounded)
        {
 
            if (isDiving)
            {
                PerformRebound(jumpVelocity, true);
            }
            else if (!isDiving)
            {
                // 普通着地
                if (Time.fixedTime - lastReboundTime > reboundProtectionTime)
                {
                    canDoubleJump = false;
                    canTripleJump = false;
                    canDive = false;
                    isDiving = false;
                }
            }
        }
        else if (isTouchingNPC)
        {
            if (isDiving)
            {
                PerformRebound(jumpVelocity, true);
            }
            else if (!isDiving)
            {
                PerformRebound(jumpVelocity, false);
            }
        }
        else if (isTrampoline)
        {
            // --- 触发所有被踩弹簧的压扁动画 ---
            Collider[] trampolines = Physics.OverlapSphere(groundCheck.position, radius, Trampoline);
            foreach (Collider col in trampolines)
            {
                TrampolineRender trampRender = col.GetComponent<TrampolineRender>();
                if (trampRender != null)
                    trampRender.TriggerCompress();
            }

            float trampolineJumpVelocity = jumpVelocity * 1.5f; // 弹簧跳跃速度更高
            if (isDiving)
            {
                PerformRebound(trampolineJumpVelocity, true);
            }
            else if (!isDiving)
            {
                PerformRebound(jumpVelocity, false);
            }
        }

        // 水平移动：只有没被发射时才执行
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        // 计算向量
        Vector3 moveInput = new Vector3(x, 0f, z).normalized;

        if ( preserveMomentum)
        {
            if (moveInput.sqrMagnitude > 0.01f)
            {
                ApplyHorizontalForce(moveInput);
            }
            else
            {
                if (isGrounded)
                    rb.velocity = new Vector3(0, rb.velocity.y, 0);
            }

        }
        else
        {
            // 不保留惯性
            ApplyHorizontalForce(moveInput);
        }

        // 跳跃
        if (jumpPressed)
        {
            if (isGrounded)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
                canDoubleJump = true;
                canTripleJump = false;   // 清除残留
                canDive = false;         // 清除残留
                playerRender.Jump();     // 触发跳跃动画
            }
            else if (!isGrounded && canDoubleJump)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);

                canDive = true;            // 解锁俯冲
                canDoubleJump = false;     // 使用双跳后禁用
                playerRender.DoubleJump(); // 触发二段跳动画
            }
            else if (!isGrounded && !canDoubleJump && canTripleJump)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
               
                canDive = true;            // 解锁俯冲
                canTripleJump = false;     // 使用三段跳后禁用
                playerRender.TripleJump(); // 触发三段跳动画
            }
            jumpPressed = false;
        }

        if (divePressed)
        {
            if (!isGrounded && canDive && !isDiving)
            {
                rb.velocity = new Vector3(rb.velocity.x, -diveSpeed, rb.velocity.z);
                canDive = false;
                isDiving = true;
            }
            divePressed = false;
        }
    }
    /// <summary>
    /// 状态重置并执行反弹（跳跃）的方法
    /// </summary>
    /// <param name="upwardVelocity"></param>
    /// <param name="playSlamGround"></param>
    public void PerformRebound(float upwardVelocity, bool playSlamGround)
    {
        // 跳跃动画
        playerRender.Jump();
        // 直接设置速度而不是加力，确保反弹的强烈感
        rb.velocity = new Vector3(rb.velocity.x, upwardVelocity, rb.velocity.z);
        ResetAirActions();
        // 播放猛击地面动画
        if (playSlamGround)
        {
            playerRender.SlamGround();
        }
    }
    /// <summary>
    /// 重置空中动作状态（解锁二段跳/三段跳，停止俯冲）
    /// 不改变速度，不播放动画
    /// </summary>
    public void ResetAirActions()
    {
        canDoubleJump = true;
        canTripleJump = true;
        isDiving = false;
        canDive = false;          // 避免刚发射就能俯冲，通常落地后才解锁
        lastReboundTime = Time.fixedTime;  // 防止落地立即重置状态
    }
    /// <summary>
    /// 根据输入方向施加速度修正力，让角色接近目标速度
    /// </summary>
    private void ApplyHorizontalForce(Vector3 input)
    {
        float controlForce = isGrounded ? groundControl : airControl;
        Vector3 targetVelocity = input * moveSpeed;
        Vector3 currentHorizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 velocityDiff = targetVelocity - currentHorizontalVelocity;
        rb.AddForce(velocityDiff * controlForce, ForceMode.Acceleration);
    }
    /// <summary>
    /// 可视化地面检测范围，帮助调整检测点和半径
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, radius);
        }
    }
}
