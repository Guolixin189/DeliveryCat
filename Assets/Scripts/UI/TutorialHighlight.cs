using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Spotlight tutorial overlay: dims the whole screen except a framed
// box around the target, points an arrow at it, and shows the
// explanation text at the arrow's tail. Builds all UI at runtime.
// Attach to the same GameObject as TutorialManager (or any object in
// the tutorial scene); it finds the Canvas itself.
public class TutorialHighlight : MonoBehaviour
{
    [Header("Arrow")]
    public Sprite arrowSprite;

    [Header("Style")]
    public Color dimColor = new Color(0f, 0f, 0f, 0.62f);
    public Color frameColor = new Color(1f, 0.85f, 0.25f, 1f);
    public float frameThickness = 6f;
    public float framePadding = 14f;
    public float arrowLength = 110f;
    public float textGap = 16f;
    public int textFontSize = 34;

    Canvas canvas;
    RectTransform canvasRt;
    GameObject overlay;
    Image dimTop, dimBottom, dimLeft, dimRight;
    Image frameTop, frameBottom, frameLeft, frameRight;
    RectTransform arrowRt;
    Image arrowImg;
    RectTransform textBgRt;
    TMP_Text textLabel;
    Sprite whiteSprite;

    // Current target (world mode follows the Transform every frame).
    Transform worldTarget;
    float worldRadius = 70f;
    RectTransform uiTarget;
    Rect currentRect;
    bool showing;

    public bool IsShowing => showing;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[TutorialHighlight] no Canvas found.");
            return;
        }
        canvasRt = canvas.GetComponent<RectTransform>();
        whiteSprite = MakeWhiteSprite();
        BuildOverlay();
    }

    void Update()
    {
        if (!showing)
            return;
        // Follow a moving world target.
        if (worldTarget != null)
        {
            Vector3 sp = Camera.main != null
                ? Camera.main.WorldToScreenPoint(worldTarget.position)
                : Vector3.zero;
            if (sp.z < 0f)
            {
                Hide();
                return;
            }
            currentRect = new Rect(sp.x - worldRadius, sp.y - worldRadius,
                worldRadius * 2f, worldRadius * 2f);
        }
        Layout();
    }

    // Highlight a UI element (e.g. the minimap panel).
    public void ShowUI(RectTransform target, string text)
    {
        if (target == null || overlay == null)
            return;
        worldTarget = null;
        uiTarget = target;
        currentRect = GetScreenRect(target);
        Show(text);
    }

    // Highlight a world object (e.g. player, pickup station, house, NPC).
    // Follows the object while it moves. radius is the half-size of the
    // highlight box in screen pixels.
    public void ShowWorld(Transform target, float radius, string text)
    {
        if (target == null || overlay == null)
            return;
        uiTarget = null;
        worldTarget = target;
        worldRadius = radius;
        Show(text);
    }

    public void Hide()
    {
        showing = false;
        worldTarget = null;
        uiTarget = null;
        if (overlay != null)
            overlay.SetActive(false);
    }

    void Show(string text)
    {
        showing = true;
        textLabel.text = text;
        overlay.SetActive(true);
        Layout();
    }

    void Layout()
    {
        // Target rect in screen pixels -> canvas-local coordinates.
        Rect r = currentRect;
        Vector2 centerLocal = ScreenToCanvas(new Vector2(r.center.x, r.center.y));
        Vector2 sizeLocal = new Vector2(r.width, r.height) / CanvasScale();
        float halfW = sizeLocal.x * 0.5f + framePadding;
        float halfH = sizeLocal.y * 0.5f + framePadding;

        float cw = canvasRt.rect.width;
        float ch = canvasRt.rect.height;

        // Dim panels: cover everything except the framed hole.
        // (pivot is bottom-left, so x starts at the canvas left edge.)
        SetRect(dimTop, new Vector2(-cw * 0.5f, centerLocal.y + halfH), new Vector2(cw, ch * 0.5f - (centerLocal.y + halfH)));
        SetRect(dimBottom, new Vector2(-cw * 0.5f, -ch * 0.5f), new Vector2(cw, (centerLocal.y - halfH) + ch * 0.5f));
        SetRect(dimLeft, new Vector2(-cw * 0.5f, centerLocal.y - halfH), new Vector2((centerLocal.x - halfW) + cw * 0.5f, halfH * 2f));
        SetRect(dimRight, new Vector2(centerLocal.x + halfW, centerLocal.y - halfH), new Vector2(cw * 0.5f - (centerLocal.x + halfW), halfH * 2f));

        // Frame around the target.
        float t = frameThickness;
        SetRect(frameTop, new Vector2(centerLocal.x - halfW - t * 0.5f, centerLocal.y + halfH - t * 0.5f), new Vector2(halfW * 2f + t, t));
        SetRect(frameBottom, new Vector2(centerLocal.x - halfW - t * 0.5f, centerLocal.y - halfH - t * 0.5f), new Vector2(halfW * 2f + t, t));
        SetRect(frameLeft, new Vector2(centerLocal.x - halfW - t * 0.5f, centerLocal.y - halfH), new Vector2(t, halfH * 2f));
        SetRect(frameRight, new Vector2(centerLocal.x + halfW - t * 0.5f, centerLocal.y - halfH), new Vector2(t, halfH * 2f));

        // Text above the target, or below it when the target is high up.
        bool textAbove = centerLocal.y < ch * 0.1f;
        float textHalfH = 34f;
        float textY = textAbove
            ? centerLocal.y + halfH + arrowLength + textGap + textHalfH
            : centerLocal.y - halfH - arrowLength - textGap - textHalfH;
        // Keep the text on screen.
        textY = Mathf.Clamp(textY, -ch * 0.5f + textHalfH + 8f, ch * 0.5f - textHalfH - 8f);
        float textX = Mathf.Clamp(centerLocal.x, -cw * 0.5f + 220f, cw * 0.5f - 220f);

        // Size the text background to the text.
        textLabel.ForceMeshUpdate();
        Vector2 pref = textLabel.GetPreferredValues(textLabel.text, 760f, 0f);
        float bgW = Mathf.Min(pref.x + 44f, 800f);
        float bgH = pref.y + 28f;
        textBgRt.anchoredPosition = new Vector2(textX, textY);
        textBgRt.sizeDelta = new Vector2(bgW, bgH);
        textLabel.rectTransform.sizeDelta = new Vector2(bgW - 32f, bgH - 20f);
        textLabel.rectTransform.anchoredPosition = Vector2.zero;

        // Arrow between the text and the target, pointing at the target.
        // Arrow sprite points up; flip it when the text is above.
        float arrowY = textAbove
            ? centerLocal.y + halfH + arrowLength * 0.5f
            : centerLocal.y - halfH - arrowLength * 0.5f;
        arrowRt.anchoredPosition = new Vector2(textX, arrowY);
        arrowRt.sizeDelta = new Vector2(arrowLength * 0.5f, arrowLength);
        arrowRt.localEulerAngles = new Vector3(0f, 0f, textAbove ? 180f : 0f);
    }

    Rect GetScreenRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        for (int i = 0; i < 4; i++)
        {
            Vector2 sp = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
            min = Vector2.Min(min, sp);
            max = Vector2.Max(max, sp);
        }
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }

    float CanvasScale()
    {
        return canvas != null ? canvas.scaleFactor : 1f;
    }

    Vector2 ScreenToCanvas(Vector2 screenPoint)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRt, screenPoint, null, out Vector2 local);
        return local;
    }

    void SetRect(Image img, Vector2 anchoredPos, Vector2 size)
    {
        RectTransform rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0f, 0f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
    }

    void BuildOverlay()
    {
        overlay = new GameObject("TutorialHighlightOverlay");
        overlay.transform.SetParent(canvas.transform, false);
        RectTransform root = overlay.AddComponent<RectTransform>();
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = Vector2.zero;
        root.sizeDelta = Vector2.zero;
        overlay.SetActive(false);

        dimTop = MakePanel("DimTop", overlay.transform, dimColor);
        dimBottom = MakePanel("DimBottom", overlay.transform, dimColor);
        dimLeft = MakePanel("DimLeft", overlay.transform, dimColor);
        dimRight = MakePanel("DimRight", overlay.transform, dimColor);
        frameTop = MakePanel("FrameTop", overlay.transform, frameColor);
        frameBottom = MakePanel("FrameBottom", overlay.transform, frameColor);
        frameLeft = MakePanel("FrameLeft", overlay.transform, frameColor);
        frameRight = MakePanel("FrameRight", overlay.transform, frameColor);

        GameObject arrowGo = new GameObject("Arrow");
        arrowGo.transform.SetParent(overlay.transform, false);
        arrowRt = arrowGo.AddComponent<RectTransform>();
        arrowRt.anchorMin = arrowRt.anchorMax = new Vector2(0.5f, 0.5f);
        arrowRt.pivot = new Vector2(0.5f, 0.5f);
        arrowImg = arrowGo.AddComponent<Image>();
        arrowImg.sprite = arrowSprite != null ? arrowSprite : whiteSprite;
        arrowImg.raycastTarget = false;

        GameObject bgGo = new GameObject("TextBackground");
        bgGo.transform.SetParent(overlay.transform, false);
        textBgRt = bgGo.AddComponent<RectTransform>();
        textBgRt.anchorMin = textBgRt.anchorMax = new Vector2(0.5f, 0.5f);
        textBgRt.pivot = new Vector2(0.5f, 0.5f);
        Image bgImg = bgGo.AddComponent<Image>();
        bgImg.sprite = whiteSprite;
        bgImg.color = new Color(0.12f, 0.12f, 0.12f, 0.85f);
        bgImg.raycastTarget = false;

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(bgGo.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = textRt.anchorMax = new Vector2(0.5f, 0.5f);
        textRt.pivot = new Vector2(0.5f, 0.5f);
        // TMP_Text is abstract and cannot be added as a component;
        // TextMeshProUGUI is the concrete UI text class.
        textLabel = textGo.AddComponent<TextMeshProUGUI>();
        textLabel.fontSize = textFontSize;
        textLabel.color = Color.white;
        textLabel.alignment = TextAlignmentOptions.Center;
        textLabel.enableWordWrapping = true;
        textLabel.raycastTarget = false;
        textLabel.outlineWidth = 0.15f;
        textLabel.outlineColor = Color.black;
    }

    Image MakePanel(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0f, 0f);
        Image img = go.AddComponent<Image>();
        img.sprite = whiteSprite;
        img.color = color;
        img.raycastTarget = false;
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
