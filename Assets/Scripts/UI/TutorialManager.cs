using UnityEngine;
using System.Collections;
using TMPro;
using player;

// 新手教学：按顺序显示 4 条提示
public class TutorialManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI promptText;   // 场景里显示提示的 TMP 文本
    public GameObject promptBackground;  // 提示的背景气泡（可空），会跟文字一起显示/隐藏

    [Header("背景气泡自适应")]
    public float bgPadX = 50f;   // 气泡左右留白
    public float bgPadY = 30f;   // 气泡上下留白

    [Header("描边（运行时自动应用，不影响其他文本）")]
    public float outlineWidth = 0.2f;
    public Color outlineColor = new Color(0.42f, 0.31f, 0.23f, 1f); // 暖棕

    [Header("流程参数")]
    public float moveThreshold = 1.5f;   // 第2步：累计移动多少距离算"动过了"
    public float avoidDuration = 5f;      // 第3步：显示几秒
    public float deliverDuration = 15f;   // 第4步：最多显示几秒（送达后会提前消失）

    [Header("提示文案（英文，可直接改）")]
    public string stepPickup = "Press SPACE to pick up the package!";
    public string stepMove = "Move with W A S D!";
    public string stepAvoid = "Careful! Hitting obstacles slows you down — avoid pedestrians!";
    public string stepDeliver = "Deliver to the house with the bubble: +100! Wrong house: -100.";

    private enum Step { Pickup, Move, Avoid, Deliver, Done }
    private Step step = Step.Pickup;
    private CatController cat;
    private Vector3 lastPos;
    private float moved;
    private float timer;

    void Awake()
    {
        if (cat == null)
            cat = FindFirstObjectByType<CatController>();
        if (promptText == null)
            Debug.LogWarning("[Tutorial] promptText not assigned!");
    }

    void Start()
    {
        if (cat != null)
            lastPos = cat.transform.position;
        ApplyOutline();
        Show(stepPickup);
    }

    // 给提示文本创建一个独立材质并设置描边：只影响自己，不污染其他文本的共享材质
    void ApplyOutline()
    {
        if (promptText == null || promptText.fontMaterial == null)
            return;
        Material m = new Material(promptText.fontMaterial);
        m.SetFloat("_OutlineWidth", outlineWidth);
        m.SetColor("_OutlineColor", outlineColor);
        promptText.fontMaterial = m;
        Debug.Log($"[Tutorial] outline applied: width={outlineWidth}, color={outlineColor}, mat={m.name}, shader={m.shader.name}");
    }

    void Update()
    {
        if (step == Step.Done || cat == null)
            return;

        switch (step)
        {
            case Step.Pickup:
                if (cat.hasPackage)
                    GoTo(Step.Move, stepMove);
                break;

            case Step.Move:
                moved += Vector3.Distance(cat.transform.position, lastPos);
                if (moved >= moveThreshold)
                    GoTo(Step.Avoid, stepAvoid);
                else if (!cat.hasPackage)      // 没怎么动就直接送达了
                    GoTo(Step.Done, null);
                break;

            case Step.Avoid:
                timer += Time.deltaTime;
                if (timer >= avoidDuration)
                    GoTo(Step.Deliver, stepDeliver);
                else if (!cat.hasPackage)
                    GoTo(Step.Done, null);
                break;

            case Step.Deliver:
                timer += Time.deltaTime;
                if (!cat.hasPackage || timer >= deliverDuration)
                    GoTo(Step.Done, null);
                break;
        }

        lastPos = cat.transform.position;
    }

    void GoTo(Step next, string text)
    {
        step = next;
        moved = 0f;
        timer = 0f;
        if (next == Step.Done)
            SetVisible(false);
        else
            Show(text);
    }

    Coroutine fitRoutine;

    void Show(string text)
    {
        if (promptText == null) return;
        SetVisible(true);
        promptText.text = text;
        if (fitRoutine != null) StopCoroutine(fitRoutine);
        fitRoutine = StartCoroutine(FitBackgroundNextFrame());
    }

    // 等一帧让 TMP 完成排版，再按文字实际渲染大小缩放背景气泡
    IEnumerator FitBackgroundNextFrame()
    {
        yield return null;
        if (promptBackground == null || promptText == null) yield break;
        promptText.ForceMeshUpdate();
        Vector2 rendered = promptText.GetRenderedValues(false);
        if (rendered.x <= 0f || rendered.y <= 0f) yield break;
        RectTransform bgRt = promptBackground.GetComponent<RectTransform>();
        RectTransform txtRt = promptText.rectTransform;
        if (bgRt == null || txtRt == null) yield break;
        bgRt.anchorMin = txtRt.anchorMin;
        bgRt.anchorMax = txtRt.anchorMax;
        bgRt.pivot = txtRt.pivot;
        bgRt.anchoredPosition = txtRt.anchoredPosition;
        bgRt.sizeDelta = new Vector2(rendered.x + bgPadX * 2f, rendered.y + bgPadY * 2f);
    }

    void SetVisible(bool v)
    {
        if (promptText != null)
            promptText.gameObject.SetActive(v);
        if (promptBackground != null)
            promptBackground.SetActive(v);
    }
}
