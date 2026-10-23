using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using player;

// Tutorial: show the 4 prompts in order
public class TutorialManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI promptText;   // TMP text that shows the prompts in the scene
    public GameObject promptBackground;  // Speech-bubble background for the prompts (nullable); shown/hidden together with the text

    [Header("Bubble auto-fit")]
    public float bgPadX = 50f;   // horizontal padding inside the bubble
    public float bgPadY = 30f;   // vertical padding inside the bubble


    [Header("Flow parameters")]
    public float moveThreshold = 1.5f;   // step 2: how much accumulated movement counts as "moved"
    public float avoidDuration = 5f;      // step 3: how many seconds to show
    public float deliverDuration = 15f;   // step 4: max seconds to show (disappears early once delivered)

    [Header("Completion panel (shown after tutorial, nullable)")]
    public GameObject tutorialCompletePanel;

    [Header("Prompt text (English, editable)")]
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
                else if (!cat.hasPackage)      // delivered without moving much
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
        {
            SetVisible(false);
            if (tutorialCompletePanel != null)
                tutorialCompletePanel.SetActive(true);
        }
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

    // Wait one frame for TMP layout, then size the bubble to the actual rendered text
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

    public void TutorialComplete_BackToMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void TutorialComplete_PlayOfficial()
    {
        GameSettings.OpenCompanySelectOnMenu = true;
        SceneManager.LoadScene("Main Menu");
    }

    void SetVisible(bool v)
    {
        if (promptText != null)
            promptText.gameObject.SetActive(v);
        if (promptBackground != null)
            promptBackground.SetActive(v);
    }
}
