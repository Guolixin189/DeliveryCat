using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Combo display on the right side of the screen. Always visible so
// the player can find it: x1 with "No combo yet" when there is no
// streak, and bright with the multiplier (COMBO x1.5 / x2) once the
// player has 2+ correct deliveries in a row. Pops (scales up briefly)
// whenever the combo goes up.
// Builds all UI at runtime. Attach to the GameManager in each level.
// (Builds in Start with a retry, like StarProgressHUD, so a Canvas
// that is not ready in Awake does not silently leave it unbuilt.)
public class ComboDisplay : MonoBehaviour
{
    [Header("Style")]
    public Color panelColor = new Color(0.12f, 0.12f, 0.12f, 0.82f);
    public Color comboColor = new Color(1f, 0.8f, 0.25f, 1f);
    public int labelFontSize = 22;
    public int multFontSize = 52;
    public int streakFontSize = 24;

    Canvas canvas;
    GameObject root;
    RectTransform panelRt;
    TMP_Text labelText;
    TMP_Text multText;
    TMP_Text streakText;
    Sprite whiteSprite;
    bool uiBuilt;

    int shownCombo = -1;
    float popTimer = 0f;
    Vector3 baseScale = Vector3.one;

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
        int combo = DeliveryManager.Instance != null ? DeliveryManager.Instance.Combo : 0;
        if (!root.activeSelf)
            root.SetActive(true);
        if (combo != shownCombo)
        {
            shownCombo = combo;
            Refresh(combo);
            if (combo >= 2)
                popTimer = 0.22f;
        }

        // Pop animation on combo increase.
        if (popTimer > 0f)
        {
            popTimer -= Time.unscaledDeltaTime;
            float k = 1f + 0.25f * Mathf.Clamp01(popTimer / 0.22f);
            panelRt.localScale = baseScale * k;
            if (popTimer <= 0f)
                panelRt.localScale = baseScale;
        }
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
            whiteSprite = MakeWhiteSprite();
            BuildUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError("[ComboDisplay] failed to build: " + e.Message);
            if (root != null) Destroy(root);
            root = null;
            panelRt = null;
            labelText = null;
            multText = null;
            streakText = null;
        }
        uiBuilt = true;
    }

    void Refresh(int combo)
    {
        float mult = DeliveryManager.Instance != null
            ? DeliveryManager.Instance.ComboMultiplier : 1f;
        multText.text = "x" + (mult % 1f == 0f ? ((int)mult).ToString() : mult.ToString("0.0"));
        streakText.text = combo >= 2 ? combo + " in a row" : "No combo yet";
        // Always fully visible (no dimming): the dimmed state was too
        // easy to miss. The multiplier text itself shows x1 vs x2.
        panelRt.GetComponent<Image>().color = panelColor;
        multText.color = comboColor;
        labelText.color = Color.white;
        streakText.color = Color.white;
    }

    void BuildUI()
    {
        root = new GameObject("ComboDisplay");
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRt = root.AddComponent<RectTransform>();
        rootRt.anchorMin = rootRt.anchorMax = new Vector2(1f, 0.5f);
        rootRt.pivot = new Vector2(1f, 0.5f);
        rootRt.anchoredPosition = new Vector2(-24f, 0f);
        rootRt.sizeDelta = new Vector2(190f, 170f);

        GameObject panelGo = new GameObject("Panel");
        panelGo.transform.SetParent(root.transform, false);
        panelRt = panelGo.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = Vector2.zero;
        panelRt.sizeDelta = Vector2.zero;
        Image panelImg = panelGo.AddComponent<Image>();
        panelImg.sprite = whiteSprite;
        panelImg.color = panelColor;
        panelImg.raycastTarget = false;

        labelText = MakeText(panelGo.transform, "Label", "COMBO",
            labelFontSize, Color.white, new Vector2(0f, 58f), new Vector2(170f, 30f));
        multText = MakeText(panelGo.transform, "Multiplier", "x2",
            multFontSize, comboColor, new Vector2(0f, 0f), new Vector2(170f, 70f));
        multText.fontStyle = FontStyles.Bold;
        streakText = MakeText(panelGo.transform, "Streak", "2 in a row",
            streakFontSize, Color.white, new Vector2(0f, -58f), new Vector2(170f, 30f));
    }

    TMP_Text MakeText(Transform parent, string name, string content,
        int fontSize, Color color, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        // TextMeshProUGUI is the concrete UI text class (TMP_Text is abstract).
        TMP_Text t = go.AddComponent<TextMeshProUGUI>();
        t.text = content;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.enableWordWrapping = true;
        t.raycastTarget = false;
        t.outlineWidth = 0.12f;
        t.outlineColor = Color.black;
        // A runtime-created TMP text has no font; borrow one from an
        // existing TMP text in the scene that already has one.
        TMP_FontAsset font = FindFont(t);
        if (font != null)
            t.font = font;
        return t;
    }

    TMP_FontAsset FindFont(TMP_Text exclude)
    {
        // Prefer the GameManager score text's font, the same source
        // StarProgressHUD uses (it is known to render correctly).
        // Grabbing any TMP text can pick a font whose atlas does not
        // match, which renders as garbled glyphs.
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

    Sprite MakeWhiteSprite()
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
    }
}
