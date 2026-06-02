using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class RewardSwitch : MonoBehaviour
{
    [System.Serializable]
    public class RewardConfig
    {
        [Tooltip("兑换所需收集物数量")]
        public int requiredAmount;
        [Tooltip("对应的奖励预制体")]
        public GameObject rewardPrefab;
    }
    public Animator SwitchAnimator;
    // 收集系统
    public CollectionUI collectionUI;
    public NPCRewardSpeak npcSpeak;

    [Header("奖励配置")]
    public RewardConfig[] rewardConfigs;
    public GameObject pos;
    public Quaternion rot;

    // 记录所有尚未生成的奖励（初始为全部配置的副本）
    private List<RewardConfig> remainingRewards;

    private bool isOnCooldown = false;
    private float cooldownTime = 0.2f;

    private void Start()
    {
        // 初始化剩余奖励列表
        if (rewardConfigs != null)
        {
            remainingRewards = new List<RewardConfig>(rewardConfigs);
        }
        else
        {
            remainingRewards = new List<RewardConfig>();
        }

        if (PlayerRoot.Instance != null)
        {
            collectionUI = PlayerRoot.Instance.GetComponent<CollectionUI>();
            if (collectionUI == null)
                Debug.LogError("玩家预制体上未找到 CollectionUI 组件！");
        }
        else
        {
            Debug.LogError("PlayerRoot 单例不存在！请检查 Boot 场景是否正确生成玩家。");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            // 尝试根据收集数量生成奖励
            if (collectionUI != null)
                StartCoroutine(TryGiveRewardWithCooldown());
            else
                Debug.LogWarning("collectionUI 未赋值！");
        }
    }

    private IEnumerator TryGiveRewardWithCooldown()
    {
        // 如果正在冷却，则忽略本次触发
        if (isOnCooldown) yield break;

        // 播放开关动画
        if (SwitchAnimator != null)
            SwitchAnimator.SetTrigger("isOpen");

        isOnCooldown = true;
        yield return new WaitForSeconds(1.5f); // 确保动画完成后再生成奖励
        TryGiveReward();                       // 生成奖励
        yield return new WaitForSeconds(cooldownTime);
        isOnCooldown = false;
    }

    private void TryGiveReward()
    {
        if (collectionUI == null) return;

        int current = collectionUI.CollectionCount;

        // 从 remainingRewards 中筛选出当前满足数量条件的奖励
        List<RewardConfig> validNow = new List<RewardConfig>();
        foreach (var config in remainingRewards)
        {
            if (config.rewardPrefab != null && current >= config.requiredAmount)
                validNow.Add(config);
        }

        if (validNow.Count == 0)
        {
            Debug.Log("数量不够，无法生成任何奖励");
            return;
        }

        // 随机选一个
        RewardConfig chosen = validNow[Random.Range(0, validNow.Count)];

        Vector3 pos1 = pos.transform.position;
        Instantiate(chosen.rewardPrefab, pos1, rot);

        // 从剩余列表中移除已生成的奖励
        remainingRewards.Remove(chosen);

        // 更新NPC对话阶段
        if (npcSpeak != null)
        {
            npcSpeak.AdvanceDialogueStage();
        }

        Debug.Log($"随机生成奖励：所需 {chosen.requiredAmount}，当前收集数量 {current}");
    }
}
