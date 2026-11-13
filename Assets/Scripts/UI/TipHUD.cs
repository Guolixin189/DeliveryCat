using UnityEngine;
using UnityEngine.UI;

// Dollar-coin badge under the Score panel. Visible while the current
// order is a tip order, so the player knows this delivery pays extra
// if it is on time. Builds all UI at runtime. Attach to the GameManager
// in each level. (Same Start-with-retry pattern as StarProgressHUD.)
public class TipHUD : MonoBehaviour
{
    [Header("Style")]
    public float badgeSize = 56f;
    // Score panel sits at (-150, -70), 340x120; the company label hangs
    // at its bottom edge, so the badge goes just below it.
    public Vector2 anchoredPosition = new Vector2(-150f, -195f);

    Canvas canvas;
    GameObject root;
    Image badgeImage;
    Sprite coinSprite;
    bool uiBuilt;

    void Start()
    {
        canvas = FindScreenCanvas();
        TryBuild();
    }

    void Update()
    {
        if (!uiBuilt)
        {
            if (canvas == null)
                canvas = FindScreenCanvas();
            TryBuild();
            return;
        }
        if (root == null)
            return;
        DeliveryManager dm = DeliveryManager.Instance;
        bool show = dm != null && dm.IsTipOrder;
        if (root.activeSelf != show)
            root.SetActive(show);
    }

    Canvas FindScreenCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas c in canvases)
        {
            if (c != null && c.isRootCanvas && c.renderMode != RenderMode.WorldSpace)
                return c;
        }
        return null;
    }

    void TryBuild()
    {
        if (uiBuilt || canvas == null)
            return;
        try
        {
            coinSprite = Resources.Load<Sprite>("coin_tip");
            if (coinSprite == null)
            {
                Debug.LogWarning("[TipHUD] coin_tip sprite not found in Resources.");
                return;
            }
            BuildUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError("[TipHUD] failed to build: " + e.Message);
            if (root != null) Destroy(root);
            root = null;
            badgeImage = null;
        }
        uiBuilt = true;
    }

    void BuildUI()
    {
        root = new GameObject("TipHUD");
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRt = root.AddComponent<RectTransform>();
        rootRt.anchorMin = rootRt.anchorMax = new Vector2(1f, 1f);
        rootRt.pivot = new Vector2(0.5f, 0.5f);
        rootRt.anchoredPosition = anchoredPosition;
        rootRt.sizeDelta = new Vector2(badgeSize, badgeSize);

        badgeImage = root.AddComponent<Image>();
        badgeImage.sprite = coinSprite;
        badgeImage.raycastTarget = false;
        root.SetActive(false);
    }
}
