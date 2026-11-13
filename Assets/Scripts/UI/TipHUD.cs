using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Dollar-coin badge under the Score panel, with a "TIP ORDER" label
// next to it. Always visible: bright gold while the current order is
// a tip order, dimmed grey when it is not, so the player always knows
// where to look. Builds all UI at runtime. Attach to the GameManager
// in each level. (Same Start-with-retry pattern as StarProgressHUD.)
public class TipHUD : MonoBehaviour
{
    [Header("Style")]
    public float badgeSize = 56f;
    public int labelFontSize = 26;
    public Color labelColor = new Color(1f, 0.8f, 0.25f, 1f);
    public Color dimCoinColor = new Color(0.5f, 0.5f, 0.5f, 0.35f);
    public Color dimLabelColor = new Color(0.6f, 0.6f, 0.6f, 0.4f);
    // Score panel sits at (-150, -70), 340x120; the company label hangs
    // at its bottom edge, so the badge goes just below it.
    public Vector2 anchoredPosition = new Vector2(-150f, -195f);

    Canvas canvas;
    GameObject root;
    Image badgeImage;
    TMP_Text labelText;
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
        // Always visible; bright for a tip order, dimmed otherwise.
        DeliveryManager dm = DeliveryManager.Instance;
        bool isTip = dm != null && dm.IsTipOrder;
        if (badgeImage != null)
            badgeImage.color = isTip ? Color.white : dimCoinColor;
        if (labelText != null)
            labelText.color = isTip ? labelColor : dimLabelColor;
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
            labelText = null;
        }
        uiBuilt = true;
    }

    void BuildUI()
    {
        // Root holds the coin on the left and the label on the right,
        // centred under the Score panel.
        float labelWidth = 150f;
        float gap = 8f;
        root = new GameObject("TipHUD");
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRt = root.AddComponent<RectTransform>();
        rootRt.anchorMin = rootRt.anchorMax = new Vector2(1f, 1f);
        rootRt.pivot = new Vector2(0.5f, 0.5f);
        rootRt.anchoredPosition = anchoredPosition;
        rootRt.sizeDelta = new Vector2(badgeSize + gap + labelWidth, badgeSize);

        GameObject coinGo = new GameObject("Coin");
        coinGo.transform.SetParent(root.transform, false);
        RectTransform coinRt = coinGo.AddComponent<RectTransform>();
        coinRt.anchorMin = coinRt.anchorMax = new Vector2(0f, 0.5f);
        coinRt.pivot = new Vector2(0f, 0.5f);
        coinRt.anchoredPosition = new Vector2(0f, 0f);
        coinRt.sizeDelta = new Vector2(badgeSize, badgeSize);
        badgeImage = coinGo.AddComponent<Image>();
        badgeImage.sprite = coinSprite;
        badgeImage.raycastTarget = false;

        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(root.transform, false);
        RectTransform labelRt = labelGo.AddComponent<RectTransform>();
        labelRt.anchorMin = labelRt.anchorMax = new Vector2(0f, 0.5f);
        labelRt.pivot = new Vector2(0f, 0.5f);
        labelRt.anchoredPosition = new Vector2(badgeSize + gap, 0f);
        labelRt.sizeDelta = new Vector2(labelWidth, 40f);
        labelText = labelGo.AddComponent<TextMeshProUGUI>();
        labelText.text = "TIP ORDER";
        labelText.fontSize = labelFontSize;
        labelText.fontStyle = FontStyles.Bold;
        labelText.color = labelColor;
        labelText.alignment = TextAlignmentOptions.Left;
        labelText.raycastTarget = false;
        labelText.outlineWidth = 0.12f;
        labelText.outlineColor = Color.black;
        TMP_FontAsset font = FindFont(labelText);
        if (font != null)
            labelText.font = font;
        // Always visible from the start; Update dims it when the
        // current order is not a tip order.
    }

    TMP_FontAsset FindFont(TMP_Text exclude)
    {
        // Prefer the GameManager score text's font, the same source
        // StarProgressHUD uses (it is known to render correctly).
        GameManager gm = GameManager.Instance;
        if (gm != null && gm.finalScoreText != null && gm.finalScoreText.font != null)
            return gm.finalScoreText.font;
        foreach (TextMeshProUGUI t in FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
        {
            if (t != null && t != exclude && t.font != null)
                return t.font;
        }
        return null;
    }
}
