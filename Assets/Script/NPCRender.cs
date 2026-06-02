using UnityEngine;
using System.Collections;

public class NPCRender : MonoBehaviour
{
    private Pathfinding pathfinding;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    public bool isRunning = true;
    private Coroutine speakCoroutine = null;
    private bool isSpeaking = false;

    private void Start()
    {
        pathfinding = GetComponent<Pathfinding>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (pathfinding != null)
        {
            Vector3 movingDirection = pathfinding.movingDirection;
            if (isRunning)
            {
                RunningAnimation(movingDirection);
            }
        }
        if (!isRunning)
        {
            // 已经在说话动画中，不重复启动
            if (isSpeaking) return;

            // 启动新协程前，确保旧的已停止（安全兜底）
            if (speakCoroutine != null)
            {
                StopCoroutine(speakCoroutine);
                speakCoroutine = null;
            }

            speakCoroutine = StartCoroutine(SpeakAnimation());
        }
    }

    /// <summary>
    /// 移动动画更新方法，根据移动方向切换动画状态和翻转精灵
    /// </summary>
    /// <param name="direction"></param>
    private void RunningAnimation(Vector3 direction)
    {
        if (animator == null) return;

        float horizontal = 0f;

        if (direction != Vector3.zero)
        {
             horizontal = direction.x;
        }

        if (horizontal != 0)
        {
            animator.SetBool("isRunning", true);
            if (horizontal > 0)
            {
                // 向右移动
                spriteRenderer.flipX = true;
            }
            else if (horizontal < 0)
            {
                // 向左移动
                spriteRenderer.flipX = false;
            }
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
    }

    /// <summary>
    /// 协程：随机间隔播放待机动画，随机持续时间
    /// </summary>
    private IEnumerator SpeakAnimation()
    {
        isSpeaking = true;
        // 随机等待 2 到 8 秒
        float waitTime = Random.Range(2f, 8f);
        yield return new WaitForSeconds(waitTime);

        // 播放说话动画
        StartSpeaking();
        // 随机播放 2 到 3 秒
        float animDuration = Random.Range(2f, 3f);
        yield return new WaitForSeconds(animDuration);

        // 停止说话动画
        StopSpeaking();

        yield return new WaitForSeconds(1f);
        isSpeaking = false;
        speakCoroutine = null;   // 协程正常结束，清除引用
    }

   public void StartSpeaking()
    {
        if (animator != null)
            animator.SetBool("isSpeak", true);
    }

   public void StopSpeaking()
    {
        if (animator != null)
            animator.SetBool("isSpeak", false);
    }
}
