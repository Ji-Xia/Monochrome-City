using UnityEngine;

public class CollectibleRender : MonoBehaviour
{
    private Vector3 startPosition;

    public float frequency = 1f;
    public float amplitude = 0.5f;

    void Start()
    {
        // 记录物体的初始世界位置
        startPosition = transform.position;
    }

    void Update()
    {
        // 基于正弦波计算Y轴偏移
        float newY = startPosition.y + Mathf.Sin(Time.time * Mathf.PI * 2 * frequency) * amplitude;

        // 更新位置
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
