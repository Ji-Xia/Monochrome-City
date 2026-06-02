using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    public GameObject playerPrefab;
    private void Awake()
    {
        if (PlayerRoot.Instance == null)
        {
            Instantiate(playerPrefab);
        }
    }
}