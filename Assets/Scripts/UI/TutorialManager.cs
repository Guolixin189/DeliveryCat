using UnityEngine;
using UnityEngine.SceneManagement;
using player;

// Tutorial: spotlight + arrow highlights for each step. Instead of a
// text bubble, the screen dims except a framed box around the thing
// being explained, an arrow points at it, and the explanation sits
// at the arrow's tail (shown by TutorialHighlight).
public class TutorialManager : MonoBehaviour
{
    [Header("Highlight")]
    public TutorialHighlight highlight;

    [Header("Flow parameters")]
    public float moveThreshold = 1.5f;   // step 2: how much accumulated movement counts as "moved"
    public float minimapDuration = 5f;    // step 3: how many seconds to show
    public float avoidDuration = 5f;      // step 4: how many seconds to show
    public float deliverDuration = 15f;   // step 5: max seconds to show (disappears early once delivered)

    [Header("Completion panel (shown after tutorial, nullable)")]
    public GameObject tutorialCompletePanel;

    [Header("Prompt text (English, editable)")]
    public string stepPickup = "Press SPACE to pick up the package!";
    public string stepMove = "Move with W A S D!";
    public string stepMinimap = "Check the minimap (bottom-left): orange dot = package to pick up, blue dot = the house to deliver to.";
    public string stepAvoid = "Careful! Hitting obstacles slows you down — avoid pedestrians!";
    public string stepDeliver = "Deliver to the house with the bubble to earn points. Wrong house loses points.";

    private enum Step { Pickup, Move, Minimap, Avoid, Deliver, Done }
    private Step step = Step.Pickup;
    private CatController cat;
    private Vector3 lastPos;
    private float moved;
    private float timer;

    void Awake()
    {
        if (cat == null)
            cat = FindFirstObjectByType<CatController>();
        if (highlight == null)
            highlight = GetComponent<TutorialHighlight>();
        if (highlight == null)
            Debug.LogWarning("[Tutorial] TutorialHighlight not assigned!");
    }

    void Start()
    {
        if (cat != null)
            lastPos = cat.transform.position;
        ShowStep(stepPickup);
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
                    GoTo(Step.Minimap, stepMinimap);
                else if (!cat.hasPackage)      // delivered without moving much
                    GoTo(Step.Done, null);
                break;

            case Step.Minimap:
                timer += Time.deltaTime;
                if (timer >= minimapDuration)
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
        {
            if (highlight != null)
                highlight.Hide();
            if (tutorialCompletePanel != null)
                tutorialCompletePanel.SetActive(true);
        }
        else
            ShowStep(text);
    }

    void ShowStep(string text)
    {
        if (highlight == null)
            return;
        switch (step)
        {
            case Step.Pickup:
                // Frame the pickup station.
                if (DeliveryManager.Instance != null && DeliveryManager.Instance.pickupZone != null)
                    highlight.ShowWorld(DeliveryManager.Instance.pickupZone.transform, 80f, text);
                else if (cat != null)
                    highlight.ShowWorld(cat.transform, 70f, text);
                break;
            case Step.Move:
                // Frame the player.
                if (cat != null)
                    highlight.ShowWorld(cat.transform, 70f, text);
                break;
            case Step.Minimap:
                // Frame the minimap panel (bottom-left).
                Minimap minimap = FindFirstObjectByType<Minimap>();
                if (minimap != null && minimap.SmallPanel != null)
                    highlight.ShowUI(minimap.SmallPanel, text);
                else if (cat != null)
                    highlight.ShowWorld(cat.transform, 70f, text);
                break;
            case Step.Avoid:
                // Frame the nearest pedestrian.
                Transform npc = NearestNPC();
                if (npc != null)
                    highlight.ShowWorld(npc, 70f, text);
                else if (cat != null)
                    highlight.ShowWorld(cat.transform, 70f, text);
                break;
            case Step.Deliver:
                // Frame the target house.
                if (DeliveryManager.Instance != null && DeliveryManager.Instance.TargetHouseTransform != null)
                    highlight.ShowWorld(DeliveryManager.Instance.TargetHouseTransform, 90f, text);
                else if (cat != null)
                    highlight.ShowWorld(cat.transform, 70f, text);
                break;
        }
    }

    Transform NearestNPC()
    {
        if (cat == null)
            return null;
        Transform best = null;
        float bestDist = float.MaxValue;
        foreach (NPCMovement npc in FindObjectsByType<NPCMovement>(FindObjectsSortMode.None))
        {
            if (npc == null) continue;
            float d = Vector3.Distance(cat.transform.position, npc.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = npc.transform;
            }
        }
        foreach (CatNPCMovement npc in FindObjectsByType<CatNPCMovement>(FindObjectsSortMode.None))
        {
            if (npc == null) continue;
            float d = Vector3.Distance(cat.transform.position, npc.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = npc.transform;
            }
        }
        return best;
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
}
