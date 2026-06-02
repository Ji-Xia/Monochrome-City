using System.Collections;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class NPCRewardSpeak : MonoBehaviour
{
    [Header("UI 组件")]
    public GameObject dialogueBox;        // 对话框面板
    public Text dialogueText;             // 显示对话内容的Text组件
    [Header("npc动画")]
    public NPCRender NPCRender;
    [Header("开始对话内容")]
    public string[] firstDialogueLines;   // 存储所有对话行的数组
    [Header("阶段对话内容")]
    public string[] stageLines;           // 所有阶段对话行合并后的数组
    [Header("打字机效果")]
    public float typingSpeed = 0.05f;     // 每个字符出现的间隔时间

    private int currentStage = 0;
    private PlayerRoot playerRoot;
    private int currentLineIndex;         // 当前正在显示的行索引
    private string[] currentLines;        // 本次对话的所有行
    private bool waitingForAdvance;       // 是否正在等待玩家按交互键推进
    public bool isSpeaking = false;

    // 打字机相关
    private string fullText;              // 当前正在显示的完整文本
    private Coroutine typingCoroutine;    // 打字协程引用

    private void Start()
    {
        // 初始状态：隐藏提示和对话框
        if (dialogueBox != null) dialogueBox.SetActive(false);
        playerRoot = PlayerRoot.Instance;
        if (playerRoot != null)
            currentStage = playerRoot.GetNPCRewardStage();  // 从全局读取阶段
        else
            Debug.LogError("PlayerRoot 不存在，对话阶段无法同步！");
    }

    /// <summary>
    /// 外部调用，奖励系统在玩家获得奖励后调用此方法以更新对话阶段
    /// </summary>
    public void AdvanceDialogueStage()
    {
        if (playerRoot != null)
        {
            playerRoot.AdvanceNPCRewardStage();
            currentStage = playerRoot.GetNPCRewardStage();
        }
        Debug.Log($"对话阶段已更新至：{currentStage}");
    }

    /// <summary>
    /// 供交互系统调用，获取当前应该显示的对话行数组
    /// </summary>
    public string[] GetCurrentDialogueLines()
    {
        // 阶段0：还未获得过奖励，使用 firstDialogueLines
        if (currentStage == 0)
            return firstDialogueLines;

        // 阶段 >0：从 stageDialogueLines 中取对应数组（索引 = currentStage - 1）
        int index = currentStage - 1;
        if (stageLines != null && index < stageLines.Length)
            return new string[] { stageLines[index] };

        // 兜底：返回空数组
        return new string[0];
    }

    /// <summary>
    /// 由玩家控制器（或其他交互系统）在按下按键时调用
    /// </summary>
    public void StartDialogue()
    {
        // 正在对话中则忽略
        if (isSpeaking) return;
        isSpeaking = true;

        currentLines = GetCurrentDialogueLines();
        // ui显示
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
            // 启动缩放动画（框和文字一起从 0.8 放大到 1）
            StartCoroutine(ScaleUp(dialogueBox));

        }
        StartCoroutine(WaitForDialogueEnd());
        Debug.Log("开始对话：" + string.Join(", ", currentLines));
    }

    /// <summary>
    /// 由玩家按下交互键时调用，推进到下一句
    /// </summary>
    public void NextLine()
    {
        // 让协程继续
        waitingForAdvance = false;
    }

    private IEnumerator WaitForDialogueEnd()
    {
        // 播放说话动画
        NPCRender.StartSpeaking();
        // 文本字符化，启动文本打字机效果
        fullText = currentLines[currentLineIndex];
        dialogueText.text = "";
        typingCoroutine = StartCoroutine(TypeText(fullText));

        // 等待玩家操作继续
        waitingForAdvance = true;
        yield return new WaitWhile(() => waitingForAdvance);

        //停止说话动画
        NPCRender.StopSpeaking();
        // 对话结束，隐藏对话框，重置状态
        if (dialogueBox != null) dialogueBox.SetActive(false);
        dialogueText.text = null;
        isSpeaking = false;
    }
    /// <summary>
    /// 打字机效果协程，逐字显示文本
    /// </summary>
    private IEnumerator TypeText(string text)
    {    
        for (int i = 0; i < text.Length; i++)
        {
            dialogueText.text += text[i];
            yield return new WaitForSeconds(typingSpeed);
        }
    }
    /// <summary>
    /// 缩放动画协程
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private IEnumerator ScaleUp(GameObject target)
    {
        Transform t = target.transform;
        t.localScale = Vector3.one * 0.8f;
        float elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(0.8f, 1f, elapsed / 0.1f);
            t.localScale = Vector3.one * scale;
            yield return null;
        }
        t.localScale = Vector3.one;
    }
}
