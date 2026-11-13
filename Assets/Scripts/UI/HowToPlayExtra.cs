using UnityEngine;
using UnityEngine.UI;
using TMPro;

// How To Play hub: the How To Play page shows three buttons, each
// opening its own panel:
//   1. BASIC CONTROLS - the original rules (goal, move, pick up /
//      deliver, avoid NPCs) with the keycap images.
//   2. GAME MECHANICS - companies, combo, tip orders, perfect
//      delivery, stars. Section titles in uppercase, roomy spacing,
//      scrollable.
//   3. TRAPS - every trap with its own image and a short line.
//      Scrollable, so it never runs into the Back button.
// The controls content is moved (reparented) from the How To Play
// panel into its own panel at runtime, so the scene text and keycap
// images stay exactly as the team made them. Builds all new UI at
// runtime. Attach to the MainMenu GameObject in the Main Menu scene
// (it is always active, unlike the How To Play panel itself).
public class HowToPlayExtra : MonoBehaviour
{
    [Header("Style")]
    public Color panelColor = new Color(0.798f, 0.953f, 0.652f, 1f);
    public Color buttonColor = new Color(0.98f, 0.93f, 0.78f, 1f);
    public Color titleColor = new Color(0.02f, 0.03f, 0.02f, 1f);
    public Color bodyColor = new Color(0.1f, 0.12f, 0.08f, 1f);
    public int titleFontSize = 60;
    public int bodyFontSize = 34;
    public int buttonFontSize = 36;
    public int hubButtonFontSize = 40;

    Canvas canvas;
    GameObject howToPlayPanel;
    GameObject controlsPanel;
    GameObject mechanicsPanel;
    GameObject trapsPanel;
    // The How To Play panel's own background sprite and color, read
    // at runtime so the new panels match it exactly.
    Sprite panelSprite;
    Color panelBgColor;

    // The basic-controls content that lives on the How To Play panel
    // in the scene; moved into the controls panel at runtime.
    static readonly string[] controlsChildren =
        { "Goal", "Move", "Action", "Warning", "WASDKeys", "SpacebarKey" };

    // Trap rows: sprite in Resources/Traps, name, one short line.
    struct TrapRow { public string sprite; public string name; public string text; }
    static readonly TrapRow[] trapRows = new TrapRow[]
    {
        new TrapRow { sprite = "Traps/puddle", name = "PUDDLE", text = "Slippery! Your steering wobbles." },
        new TrapRow { sprite = "Traps/mud", name = "MUD", text = "Slows you down." },
        new TrapRow { sprite = "Traps/banana_peel", name = "BANANA PEEL", text = "Knocks you back and stuns you for 1s." },
        new TrapRow { sprite = "Traps/car_door", name = "CAR DOOR", text = "Swings open and knocks you back." },
        new TrapRow { sprite = "Traps/sprinkler", name = "SPRINKLER", text = "A rotating jet of water slows you down." },
        new TrapRow { sprite = "Traps/barrier_arm", name = "BARRIER GATE", text = "Blocks the road when the light is red." },
        new TrapRow { sprite = "Traps/box_pile", name = "BOX PILE", text = "Pushes you around." },
        new TrapRow { sprite = "Traps/boost_pad", name = "BOOST PAD", text = "Speeds you up! This one helps you." },
    };

    void Start()
    {
        canvas = FindScreenCanvas();
        // GameObject.Find cannot find inactive objects, and the How To
        // Play panel starts hidden. Use the reference MainMenu already
        // has wired up (this script sits on the same Canvas object).
        MainMenu mainMenu = GetComponent<MainMenu>();
        if (mainMenu != null)
            howToPlayPanel = mainMenu.howToPlayPanel;
        if (howToPlayPanel == null)
            howToPlayPanel = GameObject.Find("HowtoPlayPanel");
        if (canvas == null || howToPlayPanel == null)
        {
            Debug.LogWarning("[HowToPlayExtra] Canvas or HowtoPlayPanel not found.");
            return;
        }
        // Match the How To Play panel's background exactly.
        Image srcBg = howToPlayPanel.GetComponent<Image>();
        if (srcBg != null)
        {
            panelSprite = srcBg.sprite;
            panelBgColor = srcBg.color;
        }
        else
        {
            panelSprite = MakeWhiteSprite();
            panelBgColor = panelColor;
        }
        try
        {
            controlsPanel = BuildControlsPanel();
            mechanicsPanel = BuildMechanicsPanel();
            trapsPanel = BuildTrapsPanel();
            BuildHubButtons();
        }
        catch (System.Exception e)
        {
            Debug.LogError("[HowToPlayExtra] failed to build: " + e.Message);
        }
    }

    // The hub: three big buttons stacked in the middle of the
    // How To Play page. The page keeps its title and Back button.
    void BuildHubButtons()
    {
        MakePanelButton(howToPlayPanel.transform, "ControlsButton", "BASIC CONTROLS",
            new Vector2(0f, 150f), new Vector2(480f, 80f), hubButtonFontSize,
            () => ShowPanel(controlsPanel));
        MakePanelButton(howToPlayPanel.transform, "MechanicsButton", "GAME MECHANICS",
            new Vector2(0f, 0f), new Vector2(480f, 80f), hubButtonFontSize,
            () => ShowPanel(mechanicsPanel));
        MakePanelButton(howToPlayPanel.transform, "TrapsButton", "TRAPS",
            new Vector2(0f, -150f), new Vector2(480f, 80f), hubButtonFontSize,
            () => ShowPanel(trapsPanel));
    }

    // The controls panel reuses the scene's own rules content: move
    // the Goal / Move / Action / Warning texts and the keycap images
    // from the How To Play panel into this panel. Both panels are the
    // same size and centred, so the layout is unchanged.
    GameObject BuildControlsPanel()
    {
        GameObject panel = NewPanel("ControlsPanel");
        TMP_Text titleText = MakeText(panel.transform, "Title", "BASIC CONTROLS",
            titleFontSize, titleColor, new Vector2(0f, 353f), new Vector2(1000f, 100f));
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        MakePanelButton(panel.transform, "BackButton", "BACK",
            new Vector2(0f, -341f), new Vector2(260f, 70f), buttonFontSize,
            () => ShowHowToPlay());
        foreach (string name in controlsChildren)
        {
            Transform child = howToPlayPanel.transform.Find(name);
            if (child != null)
                child.SetParent(panel.transform, false);
            else
                Debug.LogWarning("[HowToPlayExtra] controls child not found: " + name);
        }
        panel.SetActive(false);
        return panel;
    }

    void ShowPanel(GameObject panel)
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
        if (panel != null)
            panel.SetActive(true);
    }

    void ShowHowToPlay()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
        if (mechanicsPanel != null)
            mechanicsPanel.SetActive(false);
        if (trapsPanel != null)
            trapsPanel.SetActive(false);
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(true);
    }

    GameObject NewPanel(string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(canvas.transform, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(1200f, 1000f);
        Image bg = panel.AddComponent<Image>();
        bg.sprite = panelSprite != null ? panelSprite : MakeWhiteSprite();
        bg.color = panelBgColor;
        bg.raycastTarget = true;
        return panel;
    }

    // Scroll area inside a panel: title sits above it, Back below it,
    // so the text can never run into the button. Scroll with the
    // mouse wheel or by dragging.
    ScrollRect BuildScrollArea(Transform panel, Vector2 pos, Vector2 size)
    {
        GameObject scrollGo = new GameObject("ScrollArea");
        scrollGo.transform.SetParent(panel, false);
        RectTransform scrollRt = scrollGo.AddComponent<RectTransform>();
        scrollRt.anchorMin = scrollRt.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRt.pivot = new Vector2(0.5f, 0.5f);
        scrollRt.anchoredPosition = pos;
        scrollRt.sizeDelta = size;

        GameObject viewportGo = new GameObject("Viewport");
        viewportGo.transform.SetParent(scrollGo.transform, false);
        RectTransform vpRt = viewportGo.AddComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.pivot = new Vector2(0.5f, 0.5f);
        vpRt.anchoredPosition = Vector2.zero;
        vpRt.sizeDelta = Vector2.zero;
        viewportGo.AddComponent<RectMask2D>();
        viewportGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

        GameObject contentGo = new GameObject("Content");
        contentGo.transform.SetParent(viewportGo.transform, false);
        RectTransform contentRt = contentGo.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(0f, 1f);
        contentRt.pivot = new Vector2(0f, 1f);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(size.x, size.y);

        ScrollRect scroll = scrollGo.AddComponent<ScrollRect>();
        scroll.viewport = vpRt;
        scroll.content = contentRt;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;
        return scroll;
    }

    GameObject BuildMechanicsPanel()
    {
        GameObject panel = NewPanel("MechanicsPanel");
        TMP_Text titleText = MakeText(panel.transform, "Title", "GAME MECHANICS",
            titleFontSize, titleColor, new Vector2(0f, 353f), new Vector2(1000f, 100f));
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        MakePanelButton(panel.transform, "BackButton", "BACK",
            new Vector2(0f, -341f), new Vector2(260f, 70f), buttonFontSize,
            () => ShowHowToPlay());

        ScrollRect scroll = BuildScrollArea(panel.transform,
            new Vector2(0f, 0f), new Vector2(1020f, 560f));
        RectTransform contentRt = scroll.content;
        TMP_Text bodyText = MakeText(contentRt, "Body", MechanicsText(),
            bodyFontSize, bodyColor, Vector2.zero, new Vector2(1020f, 560f));
        bodyText.alignment = TextAlignmentOptions.TopLeft;
        bodyText.enableWordWrapping = true;
        // Top-anchored inside the content, then size the content to
        // the text so the scroll knows how far it can go.
        RectTransform bodyRt = bodyText.rectTransform;
        bodyRt.anchorMin = new Vector2(0f, 1f);
        bodyRt.anchorMax = new Vector2(0f, 1f);
        bodyRt.pivot = new Vector2(0f, 1f);
        bodyRt.anchoredPosition = Vector2.zero;
        bodyText.ForceMeshUpdate();
        float h = Mathf.Max(560f, bodyText.preferredHeight + 20f);
        contentRt.sizeDelta = new Vector2(1020f, h);
        bodyRt.sizeDelta = new Vector2(1020f, h);

        panel.SetActive(false);
        return panel;
    }

    GameObject BuildTrapsPanel()
    {
        GameObject panel = NewPanel("TrapsPanel");
        TMP_Text titleText = MakeText(panel.transform, "Title", "TRAPS",
            titleFontSize, titleColor, new Vector2(0f, 353f), new Vector2(1000f, 100f));
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        MakePanelButton(panel.transform, "BackButton", "BACK",
            new Vector2(0f, -341f), new Vector2(260f, 70f), buttonFontSize,
            () => ShowHowToPlay());

        ScrollRect scroll = BuildScrollArea(panel.transform,
            new Vector2(0f, 0f), new Vector2(1020f, 560f));
        RectTransform contentRt = scroll.content;
        float rowHeight = 110f;
        float spacing = 16f;
        float iconSize = 96f;
        float totalH = trapRows.Length * rowHeight + (trapRows.Length - 1) * spacing + 20f;
        contentRt.sizeDelta = new Vector2(1020f, Mathf.Max(560f, totalH));
        for (int i = 0; i < trapRows.Length; i++)
        {
            TrapRow row = trapRows[i];
            GameObject rowGo = new GameObject("Trap_" + row.name);
            rowGo.transform.SetParent(contentRt, false);
            RectTransform rowRt = rowGo.AddComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0f, 1f);
            rowRt.anchorMax = new Vector2(0f, 1f);
            rowRt.pivot = new Vector2(0f, 1f);
            rowRt.anchoredPosition = new Vector2(0f, -i * (rowHeight + spacing));
            rowRt.sizeDelta = new Vector2(1020f, rowHeight);

            Sprite icon = Resources.Load<Sprite>(row.sprite);
            if (icon != null)
            {
                GameObject iconGo = new GameObject("Icon");
                iconGo.transform.SetParent(rowGo.transform, false);
                RectTransform iconRt = iconGo.AddComponent<RectTransform>();
                iconRt.anchorMin = iconRt.anchorMax = new Vector2(0f, 0.5f);
                iconRt.pivot = new Vector2(0f, 0.5f);
                iconRt.anchoredPosition = new Vector2(0f, 0f);
                iconRt.sizeDelta = new Vector2(iconSize, iconSize);
                Image img = iconGo.AddComponent<Image>();
                img.sprite = icon;
                img.preserveAspect = true;
                img.raycastTarget = false;
            }

            TMP_Text text = MakeText(rowGo.transform, "Text",
                "<b>" + row.name + "</b>\n" + row.text,
                bodyFontSize, bodyColor,
                new Vector2(iconSize + 20f, 0f), new Vector2(1020f - iconSize - 20f, rowHeight));
            text.alignment = TextAlignmentOptions.Left;
            text.enableWordWrapping = true;
            // Left-anchored inside the row.
            RectTransform textRt = text.rectTransform;
            textRt.anchorMin = new Vector2(0f, 0.5f);
            textRt.anchorMax = new Vector2(0f, 0.5f);
            textRt.pivot = new Vector2(0f, 0.5f);
            textRt.anchoredPosition = new Vector2(iconSize + 20f, 0f);
        }

        panel.SetActive(false);
        return panel;
    }

    void MakePanelButton(Transform parent, string name, string label,
        Vector2 pos, Vector2 size, int fontSize, System.Action onClick)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Image img = go.AddComponent<Image>();
        img.sprite = MakeWhiteSprite();
        img.color = buttonColor;
        img.raycastTarget = true;
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(() => onClick());
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = colors;

        TMP_Text text = MakeText(go.transform, "Text (TMP)", label,
            fontSize, titleColor, Vector2.zero, size);
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
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
        TMP_Text t = go.AddComponent<TextMeshProUGUI>();
        t.text = content;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.enableWordWrapping = true;
        t.raycastTarget = false;
        TMP_FontAsset font = FindFont(t);
        if (font != null)
            t.font = font;
        return t;
    }

    TMP_FontAsset FindFont(TMP_Text exclude)
    {
        foreach (TextMeshProUGUI t in FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
        {
            if (t != null && t != exclude && t.font != null)
                return t.font;
        }
        return null;
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

    Sprite MakeWhiteSprite()
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
    }

    string MechanicsText()
    {
        return
            "<b><size=42>COMPANIES</size></b>\n" +
            "(official level)\n" +
            "UPS: Hold SPACE at the right door for 3s to sign. +200. Wrong house -100.\n" +
            "USPS: Drop off at the target house, it delivers by itself. Wrong house -100.\n" +
            "FedEx: 15s per order. +200 on time, +100 late, -100 wrong.\n" +
            "Freelancer: Classic. +100 right, -100 wrong.\n" +
            "\n\n" +
            "<b><size=42>COMBO</size></b>\n" +
            "Deliver correctly in a row to multiply points.\n" +
            "x1.5 at 2 in a row, x2 at 3 or more.\n" +
            "\n\n" +
            "<b><size=42>TIP ORDER</size></b>\n" +
            "A gold coin marks a tip order. Deliver within 20s for +50.\n" +
            "\n\n" +
            "<b><size=42>PERFECT DELIVERY</size></b>\n" +
            "No pedestrian or trap hit in one order for +50.\n" +
            "\n\n" +
            "<b><size=42>STARS</size></b>\n" +
            "1 star at 1 delivery, 2 stars at 3, 3 stars at 5.";
    }

    string TrapsText()
    {
        return "";
    }
}
