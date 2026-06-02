using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesPortal : MonoBehaviour
{
    #region 字段
    [Header("传送目标场景")]
    [SerializeField] private string targetSceneName;      // 场景名称

    [Header("玩家在目标场景的出生点")]
    [SerializeField] private Vector3 spawnPosition;   // 目标世界坐标
    [SerializeField] private Vector3 spawnRotation;   // 目标欧拉角（角度制）
    #endregion

    public void Teleport()
    {
        Quaternion rot = Quaternion.Euler(spawnRotation);
        // 把出生点告诉玩家单例
        PlayerRoot.SetSpawnPoint(spawnPosition, rot);
        SceneManager.LoadScene(targetSceneName);
    }
}
