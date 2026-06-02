using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CollectionUI : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private Text quantityText;
    [SerializeField] private GameObject ui;

    // 收集物品的数量
    private int collectionQuantity;
    private Coroutine uiCoroutine;
    private Coroutine blinkCoroutine;

    // 只读属性，供外部查询当前收集数量
    public int CollectionCount => collectionQuantity;

    private void Start()
    {
        if (ui == null)
            Debug.LogError("Ui 字段为空！请在 Inspector 中拖入 UI GameObject！");
        if (quantityText == null)
            Debug.LogError("Quantity Text 字段为空！请拖入 Text 组件！");

        collectionQuantity = 0; // 初始化收集数量
        ui.SetActive(false); // 隐藏UI
    }

    public void AddCollection(int amount, CollectibleData data)
    {
        collectionQuantity += amount;

        if (uiCoroutine != null) StopCoroutine(uiCoroutine);
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);

        StartCoroutine(StartUI(data));
    }

    /// <summary>
    /// UI打开并显示当前收集数量，同时启动闪烁效果吸引玩家注意，最后自动关闭UI
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartUI(CollectibleData data)
    {
        ui.SetActive(true);
        quantityText.text = collectionQuantity.ToString();
        blinkCoroutine = StartCoroutine(BlinkText(quantityText, 0.15f));

        yield return new WaitForSeconds(1f);
        StopCoroutine(blinkCoroutine);
        quantityText.color = Color.white;
        
        yield return new WaitForSeconds(2f);
        ui.SetActive(false);
    }

    /// <summary>
    /// UI闪烁效果，交替改变文本颜色以吸引玩家注意
    /// </summary>
    /// <param name="text"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    private IEnumerator BlinkText(Text text, float interval)
    {
        bool useRed = true;
        while (true)
        {
            text.color = useRed ? Color.red : Color.white;
            useRed = !useRed;
            yield return new WaitForSeconds(interval);
        }
    }
}
