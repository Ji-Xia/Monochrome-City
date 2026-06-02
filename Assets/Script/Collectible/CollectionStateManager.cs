using UnityEngine;
using System.Collections.Generic;

public class CollectionStateManager : MonoBehaviour
{
    public static CollectionStateManager Instance;

    private HashSet<string> collectedIDs = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void MarkAsCollected(string id) => collectedIDs.Add(id);
    public bool IsCollected(string id) => collectedIDs.Contains(id);
}
