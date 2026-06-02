using System.Collections;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    [Header("移动范围")]
    public BoxCollider areaBox;

    [Header("移动参数")]
    public float moveSpeed = 3f;
    public float arriveDistance = 0.5f;
    public float minWaitTime = 5f;
    public float maxWaitTime = 10f;

    [Header("超时参数")]
    public float maxMoveTime = 5f; // 超时时间，单位秒

    private float moveTimer = 0f;
    private bool isWaiting = false;

    public Vector3 movingDirection;
    private Rigidbody rb;
    private Vector3 targetPosition;
    private Coroutine waitCoroutine;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        targetPosition = transform.position; // 初始目标位置为当前所在位置
    }

    private void Update()
    {
        if(areaBox == null) return;
        if (isWaiting) return;

        // === 到达检测 ===
        float dist = Vector3.Distance(transform.position, targetPosition);
        if (dist <= arriveDistance)
        {
            StartWaitCoroutine();
            moveTimer = 0f;
            return;
        }

        // === 超时检测 ===
        moveTimer += Time.deltaTime;
        if (moveTimer >= maxMoveTime)
        {
            GenerateTarget();
            moveTimer = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (isWaiting) return;
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // 保持水平移动
        movingDirection = direction; // 记录当前移动方向

        rb.MovePosition(transform.position + direction * moveSpeed * Time.fixedDeltaTime);
    }

    /// <summary>
    /// 寻找一个新的目标位置
    /// </summary>
    private void GenerateTarget()
    {
        if (areaBox == null) return;
        Bounds bounds = areaBox.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float z = Random.Range(bounds.min.z, bounds.max.z);
        targetPosition = new Vector3(x, transform.position.y, z);
    }

    /// <summary>
    /// 等待一段随机时间后生成新的目标位置
    /// </summary>
    /// <returns></returns>
    void StartWaitCoroutine()
    {
        // 安全地停止旧的等待协程（如果有）
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }
        waitCoroutine = StartCoroutine(WaitAndGenerateNewTarget());
    }

    private IEnumerator WaitAndGenerateNewTarget()
    {
        isWaiting = true;
        movingDirection = Vector3.zero; // 停止移动
        float wait = Random.Range(minWaitTime, maxWaitTime); 
        yield return new WaitForSeconds(wait);

        GenerateTarget();
        isWaiting = false;
        waitCoroutine = null;   // 协程执行完毕，清空引用
    }
}
