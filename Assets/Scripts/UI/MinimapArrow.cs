using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Small arrow + note that points at the bottom-left minimap, shown
// only during the tutorial's Minimap step. No dimming, no frame —
// just an arrow above the minimap (pointing down at it) with the
// explanation text above the arrow, so the player's eye is drawn
// to the minimap. Builds its UI at runtime.
public class MinimapArrow : MonoBehaviour
{
    [Header("Arrow")]
    public Sprite arrowSprite;

    [Header("Style")]
    public int textFontSize = 30;
    public float arrowWidth = 56f;
    public float arrowHeight = 112f;
    public float gap = 14f;

    Canvas canvas;
    RectTransform canvasRt;
    GameObject root;
    RectTransform arrowRt;
    Image arrowImg;
    RectTransform textBgRt;
    TMP_Text textLabel;
    Sprite whiteSprite;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[MinimapArrow] no Canvas found.");
            return;
        }
        canvasRt = canvas.GetComponent<RectTransform>();
        whiteSprite = MakeWhiteSprite();
        BuildUI();
    }

    // Show the arrow + note above the minimap.
    public void Show(string text)
    {
        if (root == null)
            return;
        textLabel.text = text;
        Layout();
        root.SetActive(true);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }

    void Layout()
    {
        Minimap minimap = FindFirstObjectByType<Minimap>();
        RectTransform panel = minimap != null ? minimap.SmallPanel : null;
        if (panel == null)
            return;

        // Minimap top-center in canvas-local coordinates.
        Vector3[] corners = new Vector3[4];
        panel.GetWorldCorners(corners);
        Vector2 topCenterScreen = (RectTransformUtility.WorldToScreenPoint(null, corners[1])
            + RectTransformUtility.WorldToScreenPoint(null, corners[2])) * 0.5f;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRt, topCenterScreen, null, out Vector2 topCenter);

        float scale = canvas != null ? canvas.scaleFactor : 1f;
        float aW = arrowWidth;
        float aH = arrowHeight;

        // Arrow just above the minimap, pointing down at it.
        // The sprite points up, so rotate 180 degrees.
        arrowRt.anchoredPosition = new Vector2(topCenter.x, topCenter.y + aH * 0.5f + gap);
        arrowRt.sizeDelta = new Vector2(aW, aH);
        arrowRt.localEulerAngles = new Vector3(0f, 0f, 180f);

        // Text above the arrow, sized to fit.
        textLabel.ForceMeshUpdate();
        Vector2 pref = textLabel.GetPreferredValues(textLabel.text, 720f, 0f);
        float bgW = Mathf.Min(pref.x + 44f, 760f);
        float bgH = pref.y + 28f;
        float textY = topCenter.y + aH + gap * 2f + bgH * 0.5f;
        // Keep the text on screen.
        float ch = canvasRt.rect.height;
        textY = Mathf.Clamp(textY, -ch * 0.5f + bgH * 0.5f + 8f, ch * 0.5f - bgH * 0.5f - 8f);
        textBgRt.anchoredPosition = new Vector2(topCenter.x, textY);
        textBgRt.sizeDelta = new Vector2(bgW, bgH);
        textLabel.rectTransform.sizeDelta = new Vector2(bgW - 32f, bgH - 20f);
        textLabel.rectTransform.anchoredPosition = Vector2.zero;
        _ = scale;
    }

    void BuildUI()
    {
        root = new GameObject("MinimapArrow");
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRt = root.AddComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.pivot = new Vector2(0.5f, 0.5f);
        rootRt.anchoredPosition = Vector2.zero;
        rootRt.sizeDelta = Vector2.zero;
        root.SetActive(false);

        GameObject arrowGo = new GameObject("Arrow");
        arrowGo.transform.SetParent(root.transform, false);
        arrowRt = arrowGo.AddComponent<RectTransform>();
        arrowRt.anchorMin = arrowRt.anchorMax = new Vector2(0.5f, 0.5f);
        arrowRt.pivot = new Vector2(0.5f, 0.5f);
        arrowImg = arrowGo.AddComponent<Image>();
        arrowImg.sprite = arrowSprite != null ? arrowSprite : whiteSprite;
        arrowImg.raycastTarget = false;

        GameObject bgGo = new GameObject("TextBackground");
        bgGo.transform.SetParent(root.transform, false);
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
        // TextMeshProUGUI is the concrete UI text class (TMP_Text is abstract).
        textLabel = textGo.AddComponent<TextMeshProUGUI>();
        textLabel.fontSize = textFontSize;
        textLabel.color = Color.white;
        textLabel.alignment = TextAlignmentOptions.Center;
        textLabel.enableWordWrapping = true;
        textLabel.raycastTarget = false;
        textLabel.outlineWidth = 0.15f;
        textLabel.outlineColor = Color.black;
    }

    Sprite MakeWhiteSprite()
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
    }
}
