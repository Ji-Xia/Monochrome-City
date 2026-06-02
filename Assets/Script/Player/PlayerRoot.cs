using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRoot : MonoBehaviour
{
    private static PlayerRoot instance;
    public static PlayerRoot Instance => instance;

    // 跨场景传递的出生点数据
    private static Vector3 nextSpawnPos;
    private static Quaternion nextSpawnRot;
    private static bool hasSpawnPoint = false;

    /// <summary> 由传送门在加载场景前调用 </summary>
    public static void SetSpawnPoint(Vector3 position, Quaternion rotation)
    {
        nextSpawnPos = position;
        nextSpawnRot = rotation;
        hasSpawnPoint = true;
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        // 每次场景加载后，自动移到出生点
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (hasSpawnPoint)
        {
            transform.position = nextSpawnPos;
            transform.rotation = nextSpawnRot;
            hasSpawnPoint = false;   // 用完清除，避免重复传送

            // 2. 强制隐藏交互提示（传送后残留修复）
            PlayerManager pm = GetComponent<PlayerManager>();
            if (pm != null && pm.InteractiveUI != null)
            {
                pm.hasTeleported = false;           // 重置传送门标志
                if (pm.InteractiveUI != null)
                    pm.InteractiveUI.SetActive(false); // 同时确保 UI 隐藏
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 对话保存阶段，供 NPCRewardSpeak 查询和推进
    /// </summary>
    #region NPC对话阶段管理
    private int npcRewardStage = 0;   // 存储对话阶段（0 = 未获得任何奖励）

    public int GetNPCRewardStage()
    {
        return npcRewardStage;
    }

    public void AdvanceNPCRewardStage()
    {
        npcRewardStage++;
        Debug.Log($"全局对话阶段推进至：{npcRewardStage}");
    }
    #endregion
}
