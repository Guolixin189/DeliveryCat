using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Top-center star progress HUD: three stars that fill as packages are
// delivered (1-4 = 1 star, 5-7 = 2 stars, 8+ = 3 stars), with a progress
// bar underneath showing progress toward the next star. Builds all UI at
// runtime, so no scene setup is needed. Attach to the GameManager object.
// The thresholds reuse GameManager.CalculateStars, so the HUD always
// matches the end-screen star rating. The HUD hides once the game is over.
//
// Animation: a newly earned star pops (scales up and settles), the bar
// fills smoothly toward its target instead of jumping, the count text
// gives a small punch when it changes, and the next star to earn gently
// pulses so the player can see what to aim for.
public class StarProgressHUD : MonoBehaviour
{
    [Header("Layout")]
    public Vector2 anchoredPosition = new Vector2(0f, -10f);
    public float starSize = 48f;
    public float starSpacing = 12f;
    public float barWidth = 220f;
    public float barHeight = 10f;
    public float panelWidth = 260f;
    public float panelHeight = 116f;
    public float fontSize = 18f;

    [Header("Colors")]
    public Color panelColor = new Color(0.2f, 0.2f, 0.2f, 0.45f);
    public Color barBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.6f);
    public Color barFillColor = new Color(1f, 0.8f, 0.25f, 1f);
    public Color countColor = new Color(1f, 1f, 1f, 1f);

    [Header("Animation")]
    public float popDuration = 0.4f;
    public float popScale = 0.45f;
    public float barFillSpeed = 1.2f;
    public float pulseAmplitude = 0.05f;
    public float pulseSpeed = 3f;
    public float textPunchDuration = 0.3f;
    public float textPunchScale = 0.25f;

    Sprite whiteSprite;
    Canvas canvas;
    GameManager gameManager;
    bool uiBuilt;

    RectTransform panel;
    Image[] starImages = new Image[3];
    Image barFill;
    TMP_Text countText;

    float displayedFill;
    int lastEarned;
    int lastDelivered;
    float[] popTimers = new float[3];
    float textPunchTimer;

    void Start()
    {
        canvas = FindScreenCanvas();
        gameManager = GameManager.Instance;
        TryBuild();
    }

    void Update()
    {
        if (!uiBuilt)
        {
            if (canvas == null)
                canvas = FindScreenCanvas();
            if (gameManager == null)
                gameManager = GameManager.Instance;
            TryBuild();
            return;
        }
        if (gameManager == null || panel == null)
            return;

        panel.gameObject.SetActive(!gameManager.isGameOver);

        int delivered = gameManager.deliveredPackages;
        int earned = gameManager.CalculateStars();
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null)
                continue;
            Sprite filled = gameManager.filledStarSprite;
            Sprite empty = gameManager.emptyStarSprite;
            starImages[i].sprite = (i < earned)
                ? (filled != null ? filled : whiteSprite)
                : (empty != null ? empty : whiteSprite);
        }

        int nextThreshold;
        float targetFill;
        if (delivered >= 8)
        {
            nextThreshold = 8;
            targetFill = 1f;
        }
        else if (delivered >= 5)
        {
            nextThreshold = 8;
            targetFill = (delivered - 5) / 3f;
        }
        else if (delivered >= 1)
        {
            nextThreshold = 5;
            targetFill = (delivered - 1) / 4f;
        }
        else
        {
            nextThreshold = 1;
            targetFill = 0f;
        }

        // Smooth bar: glide toward the target instead of jumping.
        displayedFill = Mathf.MoveTowards(displayedFill, targetFill, barFillSpeed * Time.deltaTime);
        if (barFill != null)
            barFill.fillAmount = Mathf.Clamp01(displayedFill);

        // A newly earned star pops.
        if (earned > lastEarned)
        {
            for (int i = lastEarned; i < earned && i < popTimers.Length; i++)
                popTimers[i] = popDuration;
            lastEarned = earned;
        }
        if (delivered != lastDelivered)
        {
            textPunchTimer = textPunchDuration;
            lastDelivered = delivered;
        }
        if (countText != null)
            countText.text = delivered + "/" + nextThreshold;

        AnimateStars(earned);
        AnimateCountText();
    }

    void AnimateStars(int earned)
    {
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null)
                continue;
            RectTransform rt = starImages[i].rectTransform;
            if (popTimers[i] > 0f)
            {
                popTimers[i] -= Time.deltaTime;
                float t = Mathf.Clamp01(1f - popTimers[i] / popDuration);
                float s = 1f + popScale * Mathf.Sin(Mathf.PI * t);
                rt.localScale = new Vector3(s, s, 1f);
            }
            else if (i == earned && earned < 3)
            {
                // The next star to earn gently pulses.
                float s = 1f + pulseAmplitude * Mathf.Sin(Time.time * pulseSpeed);
                rt.localScale = new Vector3(s, s, 1f);
            }
            else
            {
                rt.localScale = Vector3.one;
            }
        }
    }

    void AnimateCountText()
    {
        if (countText == null)
            return;
        RectTransform rt = countText.rectTransform;
        if (textPunchTimer > 0f)
        {
            textPunchTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(1f - textPunchTimer / textPunchDuration);
            float s = 1f + textPunchScale * Mathf.Sin(Mathf.PI * t);
            rt.localScale = new Vector3(s, s, 1f);
        }
        else
        {
            rt.localScale = Vector3.one;
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
        if (uiBuilt || canvas == null || gameManager == null)
            return;

        try
        {
            whiteSprite = MakeWhiteSprite();
            BuildUI();
            lastEarned = gameManager.CalculateStars();
            lastDelivered = gameManager.deliveredPackages;
            displayedFill = 0f;
        }
        catch (System.Exception e)
        {
            Debug.LogError("[StarProgressHUD] failed to build: " + e.Message);
            if (panel != null) Destroy(panel.gameObject);
            panel = null;
            starImages = new Image[3];
            barFill = null;
            countText = null;
        }
        uiBuilt = true;
    }

    void BuildUI()
    {
        // Panel, top-center. The timer sits top-left and the score top-right,
        // so the middle of the top edge is free.
        panel = MakeRT("StarProgressHUD", canvas.transform);
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 1f);
        panel.pivot = new Vector2(0.5f, 1f);
        panel.anchoredPosition = anchoredPosition;
        panel.sizeDelta = new Vector2(panelWidth, panelHeight);
        MakeImage(panel, panelColor, false);

        // Three stars in a row, centered.
        float starTop = 10f;
        float starCenterY = -(starTop + starSize * 0.5f);
        for (int i = 0; i < 3; i++)
        {
            RectTransform star = MakeRT("Star" + (i + 1), panel);
            star.anchorMin = star.anchorMax = new Vector2(0.5f, 1f);
            star.pivot = new Vector2(0.5f, 0.5f);
            star.anchoredPosition = new Vector2((i - 1) * (starSize + starSpacing), starCenterY);
            star.sizeDelta = new Vector2(starSize, starSize);
            Image img = MakeImage(star, Color.white, false);
            img.sprite = gameManager.emptyStarSprite != null ? gameManager.emptyStarSprite : whiteSprite;
            starImages[i] = img;
        }

        // Progress bar under the stars.
        float barTop = starTop + starSize + 8f;
        float barCenterY = -(barTop + barHeight * 0.5f);
        RectTransform barBg = MakeRT("StarProgressBar", panel);
        barBg.anchorMin = barBg.anchorMax = new Vector2(0.5f, 1f);
        barBg.pivot = new Vector2(0.5f, 0.5f);
        barBg.anchoredPosition = new Vector2(0f, barCenterY);
        barBg.sizeDelta = new Vector2(barWidth, barHeight);
        MakeImage(barBg, barBackgroundColor, false);

        RectTransform fillRt = MakeRT("StarProgressFill", barBg);
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.pivot = new Vector2(0f, 0.5f);
        fillRt.anchoredPosition = Vector2.zero;
        fillRt.sizeDelta = Vector2.zero;
        barFill = fillRt.gameObject.AddComponent<Image>();
        barFill.sprite = whiteSprite;
        barFill.color = barFillColor;
        barFill.raycastTarget = false;
        barFill.type = Image.Type.Filled;
        barFill.fillMethod = Image.FillMethod.Horizontal;
        barFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        barFill.fillAmount = 0f;

        // Delivered count, e.g. 3/5 (toward the next star).
        float textTop = barTop + barHeight + 6f;
        RectTransform textRt = MakeRT("StarProgressText", panel);
        textRt.anchorMin = textRt.anchorMax = new Vector2(0.5f, 1f);
        textRt.pivot = new Vector2(0.5f, 0.5f);
        textRt.anchoredPosition = new Vector2(0f, -(textTop + 11f));
        textRt.sizeDelta = new Vector2(barWidth, 22f);
        countText = textRt.gameObject.AddComponent<TextMeshProUGUI>();
        countText.text = "0/1";
        countText.fontSize = fontSize;
        countText.alignment = TextAlignmentOptions.Center;
        countText.color = countColor;
        countText.raycastTarget = false;
        TMP_FontAsset font = FindFont();
        if (font != null)
            countText.font = font;
    }

    TMP_FontAsset FindFont()
    {
        if (gameManager != null && gameManager.finalScoreText != null
            && gameManager.finalScoreText.font != null)
            return gameManager.finalScoreText.font;
        TextMeshProUGUI any = FindFirstObjectByType<TextMeshProUGUI>();
        return any != null ? any.font : null;
    }

    RectTransform MakeRT(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }

    Image MakeImage(RectTransform rt, Color color, bool raycast)
    {
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = whiteSprite;
        img.color = color;
        img.raycastTarget = raycast;
        return img;
    }

    Sprite MakeWhiteSprite()
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
    }
}
