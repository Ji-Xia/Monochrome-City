using System.Collections.Generic;
using UnityEngine;

public class SceneStateManager : MonoBehaviour
{
    public static SceneStateManager Instance;

    // 物体状态字典（ID → 状态对象）
    private Dictionary<string, object> states = new Dictionary<string, object>();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 保存、读取、清除方法（与之前加在 PlayerRoot 里的完全一致）
    public void SetState(string id, object state) => states[id] = state;
    public T GetState<T>(string id, T defaultValue = default)
    {
        if (states.TryGetValue(id, out object obj) && obj is T)
            return (T)obj;
        return defaultValue;
    }
    public void ClearState(string id) => states.Remove(id);
}
