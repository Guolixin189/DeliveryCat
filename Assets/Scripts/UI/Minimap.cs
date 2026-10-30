using UnityEngine;
using UnityEngine.UI;

// Bottom-left minimap: approximate building dots plus a red player dot.
// Click the small map to enlarge it; click the dark backdrop or the X to
// shrink it back. Builds all UI at runtime, so no scene setup is needed.
// Building dots come from the DeliveryManager zones: houses in gray, the
// pickup station in orange, the current target house in blue.
public class Minimap : MonoBehaviour
{
    [Header("Size")]
    public float smallSize = 200f;
    public float largeSize = 600f;
    public Vector2 smallAnchoredPosition = new Vector2(20f, 150f);

    [Header("Dots")]
    public float houseDotSize = 10f;
    public float playerDotSize = 14f;
    public float boundsPadding = 0.15f;

    [Header("Colors")]
    public Color backgroundColor = new Color(0.35f, 0.35f, 0.35f, 0.6f);
    public Color houseColor = new Color(0.85f, 0.85f, 0.85f, 1f);
    public Color pickupColor = new Color(1f, 0.72f, 0.30f, 1f);
    public Color targetColor = new Color(0.40f, 0.80f, 1f, 1f);
    public Color playerColor = new Color(1f, 0.2f, 0.2f, 1f);

    Sprite whiteSprite;
    Canvas canvas;
    DeliveryManager deliveryManager;
    bool uiBuilt;

    RectTransform smallPanel;
    RectTransform largeRoot;
    RectTransform largePanel;
    RectTransform content;
    RectTransform playerDot;
    RectTransform pickupDot;
    System.Collections.Generic.List<DropoffZone> houses =
        new System.Collections.Generic.List<DropoffZone>();
    System.Collections.Generic.List<RectTransform> houseDots =
        new System.Collections.Generic.List<RectTransform>();

    Vector2 boundsMin;
    Vector2 boundsSize;
    float currentSize;
    bool enlarged;

    void Start()
    {
        canvas = FindScreenCanvas();
        deliveryManager = DeliveryManager.Instance;
        TryBuild();
    }

    void Update()
    {
        if (!uiBuilt)
        {
            if (canvas == null)
                canvas = FindScreenCanvas();
            if (deliveryManager == null)
                deliveryManager = DeliveryManager.Instance;
            TryBuild();
            return;
        }

        PlaceDot(playerDot, transform.position, playerDotSize * (currentSize / smallSize), currentSize);

        DropoffZone target = FindTargetDropoff();
        for (int i = 0; i < houses.Count; i++)
        {
            Image img = houseDots[i].GetComponent<Image>();
            if (img != null)
                img.color = (houses[i] == target) ? targetColor : houseColor;
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
        if (uiBuilt || canvas == null || deliveryManager == null)
            return;
        if (deliveryManager.dropOffZones == null || deliveryManager.dropOffZones.Length == 0)
            return;

        try
        {
            whiteSprite = MakeWhiteSprite();
            ComputeBounds();
            BuildUI();
            LayoutDots(smallSize);
            currentSize = smallSize;
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Minimap] failed to build: " + e.Message);
            if (smallPanel != null) Destroy(smallPanel.gameObject);
            if (largeRoot != null) Destroy(largeRoot.gameObject);
            smallPanel = null;
            largeRoot = null;
            content = null;
            playerDot = null;
            pickupDot = null;
            houses.Clear();
            houseDots.Clear();
        }
        uiBuilt = true;
    }

    void ComputeBounds()
    {
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;
        if (deliveryManager.pickupZone != null)
            Include(deliveryManager.pickupZone.transform.position, ref minX, ref minY, ref maxX, ref maxY);
        foreach (var z in deliveryManager.dropOffZones)
        {
            if (z != null)
                Include(z.transform.position, ref minX, ref minY, ref maxX, ref maxY);
        }
        float padX = (maxX - minX) * boundsPadding + 1f;
        float padY = (maxY - minY) * boundsPadding + 1f;
        boundsMin = new Vector2(minX - padX, minY - padY);
        boundsSize = new Vector2((maxX - minX) + padX * 2f, (maxY - minY) + padY * 2f);
    }

    void Include(Vector3 p, ref float minX, ref float minY, ref float maxX, ref float maxY)
    {
        if (p.x < minX) minX = p.x;
        if (p.y < minY) minY = p.y;
        if (p.x > maxX) maxX = p.x;
        if (p.y > maxY) maxY = p.y;
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

    void BuildUI()
    {
        // Small map, bottom-left, just above the corner compass.
        smallPanel = MakeRT("MinimapSmall", canvas.transform);
        smallPanel.anchorMin = smallPanel.anchorMax = Vector2.zero;
        smallPanel.pivot = Vector2.zero;
        smallPanel.anchoredPosition = smallAnchoredPosition;
        smallPanel.sizeDelta = new Vector2(smallSize, smallSize);
        Image smallBg = MakeImage(smallPanel, backgroundColor, true);
        Button smallBtn = smallPanel.gameObject.AddComponent<Button>();
        smallBtn.targetGraphic = smallBg;
        smallBtn.transition = Selectable.Transition.None;
        smallBtn.onClick.AddListener(Enlarge);

        content = MakeRT("MinimapContent", smallPanel);
        content.anchorMin = Vector2.zero;
        content.anchorMax = Vector2.one;
        content.pivot = Vector2.zero;
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        foreach (var z in deliveryManager.dropOffZones)
        {
            if (z == null) continue;
            houses.Add(z);
            houseDots.Add(MakeDot(houseColor));
        }
        if (deliveryManager.pickupZone != null)
            pickupDot = MakeDot(pickupColor);
        playerDot = MakeDot(playerColor);

        // Large map, hidden until enlarged.
        largeRoot = MakeRT("MinimapLarge", canvas.transform);
        largeRoot.anchorMin = Vector2.zero;
        largeRoot.anchorMax = Vector2.one;
        largeRoot.pivot = new Vector2(0.5f, 0.5f);
        largeRoot.anchoredPosition = Vector2.zero;
        largeRoot.sizeDelta = Vector2.zero;

        RectTransform backdrop = MakeRT("Backdrop", largeRoot);
        backdrop.anchorMin = Vector2.zero;
        backdrop.anchorMax = Vector2.one;
        backdrop.anchoredPosition = Vector2.zero;
        backdrop.sizeDelta = Vector2.zero;
        Image backdropImg = MakeImage(backdrop, new Color(0f, 0f, 0f, 0.5f), true);
        Button backdropBtn = backdrop.gameObject.AddComponent<Button>();
        backdropBtn.targetGraphic = backdropImg;
        backdropBtn.transition = Selectable.Transition.None;
        backdropBtn.onClick.AddListener(Shrink);

        largePanel = MakeRT("MinimapLargePanel", largeRoot);
        largePanel.anchorMin = largePanel.anchorMax = new Vector2(0.5f, 0.5f);
        largePanel.pivot = new Vector2(0.5f, 0.5f);
        largePanel.anchoredPosition = Vector2.zero;
        largePanel.sizeDelta = new Vector2(largeSize, largeSize);
        MakeImage(largePanel, backgroundColor, true);

        RectTransform closeBtn = MakeRT("CloseButton", largePanel);
        closeBtn.anchorMin = closeBtn.anchorMax = Vector2.one;
        closeBtn.pivot = Vector2.one;
        closeBtn.anchoredPosition = new Vector2(-8f, -8f);
        closeBtn.sizeDelta = new Vector2(36f, 36f);
        Image closeImg = MakeImage(closeBtn, new Color(0.2f, 0.2f, 0.2f, 1f), true);
        Button close = closeBtn.gameObject.AddComponent<Button>();
        close.targetGraphic = closeImg;
        close.transition = Selectable.Transition.None;
        close.onClick.AddListener(Shrink);

        // X icon from two rotated bars (no font dependency).
        for (int i = 0; i < 2; i++)
        {
            GameObject barGo = new GameObject("XBar");
            barGo.transform.SetParent(closeBtn, false);
            RectTransform barRt = barGo.AddComponent<RectTransform>();
            barRt.anchorMin = barRt.anchorMax = new Vector2(0.5f, 0.5f);
            barRt.pivot = new Vector2(0.5f, 0.5f);
            barRt.anchoredPosition = Vector2.zero;
            barRt.sizeDelta = new Vector2(24f, 4f);
            barRt.localRotation = Quaternion.Euler(0f, 0f, i == 0 ? 45f : -45f);
            Image barImg = barGo.AddComponent<Image>();
            barImg.sprite = whiteSprite;
            barImg.color = Color.white;
            barImg.raycastTarget = false;
        }

        largeRoot.gameObject.SetActive(false);
    }

    RectTransform MakeDot(Color color)
    {
        RectTransform rt = MakeRT("Dot", content);
        rt.anchorMin = rt.anchorMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
        MakeImage(rt, color, false);
        return rt;
    }

    void LayoutDots(float mapSize)
    {
        float s = mapSize / smallSize;
        for (int i = 0; i < houses.Count; i++)
            PlaceDot(houseDots[i], houses[i].transform.position, houseDotSize * s, mapSize);
        if (pickupDot != null && deliveryManager.pickupZone != null)
            PlaceDot(pickupDot, deliveryManager.pickupZone.transform.position, houseDotSize * s, mapSize);
        PlaceDot(playerDot, transform.position, playerDotSize * s, mapSize);
    }

    void PlaceDot(RectTransform dot, Vector3 worldPos, float dotSize, float mapSize)
    {
        float nx = Mathf.Clamp01((worldPos.x - boundsMin.x) / boundsSize.x);
        float ny = Mathf.Clamp01((worldPos.y - boundsMin.y) / boundsSize.y);
        dot.anchoredPosition = new Vector2(nx * mapSize, ny * mapSize);
        dot.sizeDelta = new Vector2(dotSize, dotSize);
    }

    void Enlarge()
    {
        if (enlarged) return;
        enlarged = true;
        Rect canvasRect = ((RectTransform)canvas.transform).rect;
        float ls = Mathf.Min(largeSize, canvasRect.width * 0.85f, canvasRect.height * 0.85f);
        largePanel.sizeDelta = new Vector2(ls, ls);
        content.SetParent(largePanel, false);
        largeRoot.gameObject.SetActive(true);
        smallPanel.gameObject.SetActive(false);
        LayoutDots(ls);
        currentSize = ls;
    }

    void Shrink()
    {
        if (!enlarged) return;
        enlarged = false;
        content.SetParent(smallPanel, false);
        largeRoot.gameObject.SetActive(false);
        smallPanel.gameObject.SetActive(true);
        LayoutDots(smallSize);
        currentSize = smallSize;
    }

    DropoffZone FindTargetDropoff()
    {
        if (deliveryManager == null)
            return null;
        var zones = deliveryManager.dropOffZones;
        if (zones == null)
            return null;
        foreach (var z in zones)
        {
            if (z == null || z.indicator == null)
                continue;
            if (z.indicator.activeSelf)
                return z;
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
