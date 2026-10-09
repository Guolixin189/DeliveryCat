using UnityEngine;
using TMPro;
using player;

// 新手教学：按顺序显示 4 条提示
public class TutorialManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI promptText;   // 场景里显示提示的 TMP 文本

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
        Show(stepPickup);
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
            promptText.gameObject.SetActive(false);
        else
            Show(text);
    }

    void Show(string text)
    {
        if (promptText == null) return;
        promptText.gameObject.SetActive(true);
        promptText.text = text;
    }
}
