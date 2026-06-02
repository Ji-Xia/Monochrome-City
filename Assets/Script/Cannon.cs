using System.Collections;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    [Header("发射设置")]
    public Vector3 launchDirection;   // 自定义发射方向
    public float flight = 15f;             
    public float fireDelay = 0.5f;               // 进入大炮后等待多久发射
    public Animator cannonAnimator;

    [Header("特效（可选）")]
    public GameObject launchEffectPrefab;

    private bool isProcessing = false;

    // 这个触发器要放在炮口的子物体上，并且勾选 Is Trigger
    private void OnTriggerEnter(Collider other)
    {
        if (!isProcessing && other.CompareTag("Player"))
        {
            Rigidbody rb = other.attachedRigidbody;
            PlayerManager playerManager = other.GetComponent<PlayerManager>();
            StartCoroutine(CannonSequence(other.gameObject,playerManager,rb));         
        }
    }

    IEnumerator CannonSequence(GameObject player, PlayerManager playerManager,Rigidbody rb)
    {
        isProcessing = true;

        Transform firePoint = transform;
        // 隐藏玩家
        player.SetActive(false);

        if (launchEffectPrefab != null)
        {
            GameObject eff = Instantiate(launchEffectPrefab, firePoint.position, firePoint.rotation);
            Destroy(eff, 0.5f);
        }

        // 播放发射动画
        if (cannonAnimator != null)
            cannonAnimator.SetTrigger("isFire");

        // 等待一段时间（等大炮的发射动画播放）
        yield return new WaitForSeconds(fireDelay);

        // 发射玩家
        firePoint = transform;
        player.transform.position = firePoint.position;
        player.SetActive(true);

        // 发射特效
        if (launchEffectPrefab != null)
        {
            GameObject eff = Instantiate(launchEffectPrefab, firePoint.position, firePoint.rotation);
            Destroy(eff, 1.5f);
        }

        playerManager.preserveMomentum = true; // 保持原有动量，防止被重置
        rb.velocity = Vector3.zero;
        Vector3 thrust = launchDirection.normalized * flight;
        rb.AddForce(thrust, ForceMode.VelocityChange);
        // 空中动作重置（解锁二段跳等）
        playerManager.ResetAirActions();

        // 冷却一会儿，防止立刻再次触发
        yield return new WaitForSeconds(1f);
        playerManager.preserveMomentum = false; // 取消保持动量，恢复正常控制
        isProcessing = false;
    }
}
