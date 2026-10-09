using UnityEngine;
using TMPro;
using player;

public class TutorialManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI promptText;   

    [Header("Parameter")]
    public float moveThreshold = 1.5f;   
    public float avoidDuration = 5f;     
    public float deliverDuration = 15f;  

    [Header("Prompt")]
    public string stepPickup = "Press SPACE to pick up the package!";
    public string stepMove = "Move with W A S D!";
    public string stepAvoid = "Careful! Hitting obstacles slows you down \u2014, avoid pedestrians!";
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
                else if (!cat.hasPackage)      
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
