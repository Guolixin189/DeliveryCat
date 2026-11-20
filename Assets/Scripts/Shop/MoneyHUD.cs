using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Coin + money readout under the TipHUD badge. Always visible so
// the player knows what they can afford at a supply station.
// Built at runtime (same Start-with-retry pattern as TipHUD).
// Attach to the GameManager in each level that has supply stations.
public class MoneyHUD : MonoBehaviour
{
    [Header("Style")]
    public float badgeSize = 44f;
    public int labelFontSize = 26;
    public Vector2 anchoredPosition = new Vector2(-150f, -278f);

    Canvas canvas;
    GameObject root;
    TMP_Text moneyText;
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
        if (root == null || moneyText == null)
            return;
        DeliveryManager dm = DeliveryManager.Instance;
        int money = dm != null ? dm.Money : 0;
        string text = "Coins: " + money;
        if (moneyText.text != text)
            moneyText.text = text;
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
                Debug.LogWarning("[MoneyHUD] coin_tip sprite not found in Resources.");
                return;
            }
            BuildUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError("[MoneyHUD] failed to build: " + e.Message);
            if (root != null) Destroy(root);
            root = null;
            moneyText = null;
        }
        uiBuilt = true;
    }

    void BuildUI()
    {
        float labelWidth = 200f;
        float gap = 8f;
        root = new GameObject("MoneyHUD");
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
        Image coinImg = coinGo.AddComponent<Image>();
        coinImg.sprite = coinSprite;
        coinImg.raycastTarget = false;

        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(root.transform, false);
        RectTransform labelRt = labelGo.AddComponent<RectTransform>();
        labelRt.anchorMin = labelRt.anchorMax = new Vector2(0f, 0.5f);
        labelRt.pivot = new Vector2(0f, 0.5f);
        labelRt.anchoredPosition = new Vector2(badgeSize + gap, 0f);
        labelRt.sizeDelta = new Vector2(labelWidth, 40f);
        moneyText = labelGo.AddComponent<TextMeshProUGUI>();
        moneyText.text = "Coins: 0";
        moneyText.fontSize = labelFontSize;
        moneyText.fontStyle = FontStyles.Bold;
        moneyText.color = new Color(1f, 0.85f, 0.3f, 1f);
        moneyText.alignment = TextAlignmentOptions.Left;
        moneyText.enableWordWrapping = false;
        moneyText.raycastTarget = false;
        moneyText.outlineWidth = 0.12f;
        moneyText.outlineColor = Color.black;
        TMP_FontAsset font = FindFont(moneyText);
        if (font != null)
            moneyText.font = font;
    }

    TMP_FontAsset FindFont(TMP_Text exclude)
    {
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
